using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using loxxking_backend_clean.Application.Common.Interfaces;
using loxxking_backend_clean.Infrastructure.Persistence;
using loxxking_backend_clean.Domain.Enums;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("sso")]
public class SsoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ISsoJtiValidator _jtiValidator;
    private readonly IConfiguration _config;

    public SsoController(
        ApplicationDbContext context, 
        ISsoJtiValidator jtiValidator,
        IConfiguration config)
    {
        _context = context;
        _jtiValidator = jtiValidator;
        _config = config;
    }

    [HttpPost("loxxking-token")]
    [AllowAnonymous]
    public async Task<IActionResult> LaunchLoxxking([FromForm] string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token)) return Unauthorized(new { error = "Token is empty or null" });

        var publicKeyPath = _config["Sso:RsaPublicKeyPath"] ?? "App_Data/Keys/sso_public_key.pem";
        var envPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), publicKeyPath);
        if (!System.IO.File.Exists(envPath)) return Unauthorized(new { error = "SSO Public Key not found at " + envPath });
        
        var publicKeyPem = await System.IO.File.ReadAllTextAsync(envPath, ct);
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);

        var issuer = _config["Sso:Issuer"] ?? "LuxiraCRM";
        var audience = _config["Sso:Audience"] ?? "LoxxkingApp";
        var clockSkewSeconds = _config.GetValue<int>("Sso:ClockSkewSeconds", 30);

        var tokenValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(clockSkewSeconds),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsa) { KeyId = Guid.NewGuid().ToString() },
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
        };

        var handler = new JwtSecurityTokenHandler();
        handler.InboundClaimTypeMap.Clear();
        ClaimsPrincipal principal;
        try
        {
            principal = handler.ValidateToken(token, tokenValidationParams, out _);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { error = "Token validation failed", details = ex.Message, stack = ex.StackTrace });
        }

        var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (string.IsNullOrWhiteSpace(jti)) return Unauthorized(new { error = "JTI is missing" });

        if (!_jtiValidator.TryConsume(jti, TimeSpan.FromSeconds(120)))
        {
            return Unauthorized(new { error = "Token already consumed" });
        }

        var email = principal.FindFirstValue(JwtRegisteredClaimNames.Email) ?? principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized(new { error = "Email is missing" });

        // 5. DB Query using NormalizedEmail ONLY
        var normalizedEmail = email.ToUpperInvariant();
        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, ct);

        // Authorization Checks
        if (userInDb == null) return StatusCode(403, new { error = $"User not found in Loxxking database for email: {email} (normalized: {normalizedEmail})" });
        if (userInDb.IsDeleted) return StatusCode(403, new { error = "User account is deleted in Loxxking." });
        if (!userInDb.IsActive) return StatusCode(403, new { error = "User account is not active in Loxxking." });
        if (userInDb.Role != loxxking_backend_clean.Domain.Enums.UserRole.Admin) return StatusCode(403, new { error = $"User role is {userInDb.Role}, but Admin is required." });

        // 6. Issue Cookie (.Loxxking.Session)
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userInDb.Id.ToString()));
        identity.AddClaim(new Claim(ClaimTypes.Name, userInDb.Name ?? ""));
        identity.AddClaim(new Claim(ClaimTypes.Email, userInDb.Email ?? ""));
        identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            authProperties);

        // 7. Hardcoded 302 Local Redirect
        return LocalRedirect("/admin");
    }
}
