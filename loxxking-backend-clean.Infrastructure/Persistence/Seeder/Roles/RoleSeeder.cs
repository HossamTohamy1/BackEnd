using loxxking_backend_clean.Domain.Enums;
using loxxking_backend_clean.Infrastructure.Persistence.Seeder.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace loxxking_backend_clean.Infrastructure.Persistence.Seeder.Roles;

public class RoleSeeder : IDataSeeder
{
    public int Order => 1;

    public async Task SeedAsync(SeedContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var logger = serviceProvider.GetRequiredService<ILogger<RoleSeeder>>();

        var roles = new[]
        {
            UserRole.Admin.ToString(),
            UserRole.StoreManager.ToString(),
            UserRole.SalesEmployee.ToString(),
            UserRole.Customer.ToString()
        };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                });

                if (result.Succeeded)
                {
                    logger.LogInformation("Seeded role '{RoleName}'.", roleName);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError("Failed to seed role '{RoleName}': {Errors}", roleName, errors);
                    throw new InvalidOperationException($"Failed to create role '{roleName}': {errors}");
                }
            }
        }
    }
}
