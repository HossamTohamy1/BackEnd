using loxxking_backend_clean.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/v1/context")]
public class ContextController : ControllerBase
{
    private readonly IIpResolverService _ipResolver;
    private readonly IGeolocationService _geolocationService;
    private readonly IApplicationDbContext _context;

    public ContextController(
        IIpResolverService ipResolver, 
        IGeolocationService geolocationService,
        IApplicationDbContext context)
    {
        _ipResolver = ipResolver;
        _geolocationService = geolocationService;
        _context = context;
    }

    [HttpGet("init")]
    [AllowAnonymous]
    public async Task<IActionResult> InitContext(CancellationToken cancellationToken)
    {
        var storeDefault = await _context.Countries
            .OrderByDescending(c => c.IsDefault)
            .ThenBy(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var defaultCurrency = storeDefault?.Currency ?? "USD";
        var defaultCountryName = storeDefault?.Name ?? "Unknown";

        try
        {
            var ip = _ipResolver.GetClientIpAddress();
            if (!string.IsNullOrEmpty(ip) && _ipResolver.IsValidPublicIp(ip))
            {
                var geo = await _geolocationService.GetGeoLocationAsync(ip, cancellationToken);
                if (geo != null && !string.IsNullOrEmpty(geo.CountryCode))
                {
                    var country = await _context.Countries
                        .FirstOrDefaultAsync(c => 
                            c.Name == geo.CountryName || 
                            c.Name == geo.CountryCode || 
                            EF.Functions.Like(c.Name, geo.CountryName + "%"), 
                            cancellationToken);

                    if (country == null)
                    {
                        country = loxxking_backend_clean.Domain.Entities.Countries.Country.Create(geo.CountryName, geo.Currency, "en", isDefault: false);
                        _context.Countries.Add(country);
                        await _context.SaveChangesAsync(cancellationToken);
                    }

                    return Ok(new
                    {
                        country = country.Name,
                        countryCode = geo.CountryCode,
                        countryId = country.Id,
                        currency = !string.IsNullOrWhiteSpace(country.Currency) ? country.Currency : geo.Currency
                    });
                }
            }
        }
        catch
        {
            // Suppress exceptions and fallback
        }

        if (storeDefault != null)
        {
            return Ok(new
            {
                country = storeDefault.Name,
                countryCode = storeDefault.Name,
                countryId = storeDefault.Id,
                currency = !string.IsNullOrWhiteSpace(storeDefault.Currency) ? storeDefault.Currency : defaultCurrency
            });
        }

        return Ok(new
        {
            country = defaultCountryName,
            countryCode = defaultCountryName,
            countryId = (Guid?)null,
            currency = defaultCurrency
        });
    }
}
