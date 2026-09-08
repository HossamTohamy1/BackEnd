using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using loxxking_backend_clean.Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using loxxking_backend_clean.Infrastructure.Persistence.Seeder;

namespace loxxking_backend_clean.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddIdentityCore<User>(options =>
        {
            options.User.RequireUniqueEmail = false;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IPasswordHasher<User>, loxxking_backend_clean.Infrastructure.Authentication.LegacyBCryptPasswordHasher>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "SmartScheme";
            options.DefaultChallengeScheme = "SmartScheme";
        })
        .AddPolicyScheme("SmartScheme", "JWT or Cookie", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                string authorization = context.Request.Headers.Authorization.ToString();
                if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                {
                    return JwtBearerDefaults.AuthenticationScheme;
                }

                return CookieAuthenticationDefaults.AuthenticationScheme;
            };
        })
        .AddJwtBearer(options =>
        {
            var jwtSecret = configuration["Jwt:Secret"] ?? "SUPER_SECRET_KEY_NEEDS_TO_BE_LONG_ENOUGH";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"] ?? "LoxxKing",
                ValidAudience = configuration["Jwt:Audience"] ?? "LoxxKingClient",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        })
        .AddCookie(options =>
        {
            options.LoginPath = "/api/auth/login";
            options.AccessDeniedPath = "/api/auth/access-denied";
        });

        services.AddSignalR();
        
        services.AddHttpClient();
        services.AddHttpClient("LegacyCrmClient", client => 
        { 
            client.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("LegacyCrm:TimeoutSeconds", 15)); 
        });

        services.AddScoped<IInvoicePdfGenerator, loxxking_backend_clean.Infrastructure.Services.QuestPdfInvoiceGenerator>();
        services.AddScoped<IOrderNotificationService, loxxking_backend_clean.Infrastructure.Services.OrderNotificationService>();
        services.AddScoped<IJwtProvider, loxxking_backend_clean.Infrastructure.Authentication.JwtProvider>();
        services.AddScoped<IFileStorageService, loxxking_backend_clean.Infrastructure.Services.CloudinaryFileStorageService>();
        
        services.AddScoped<ILegacyCrmSyncService, loxxking_backend_clean.Infrastructure.Services.LegacyCrmSyncService>();
        services.AddHostedService<loxxking_backend_clean.Infrastructure.Services.OrderSyncBackgroundService>();

        services.AddSeeders();

        return services;
    }
}
