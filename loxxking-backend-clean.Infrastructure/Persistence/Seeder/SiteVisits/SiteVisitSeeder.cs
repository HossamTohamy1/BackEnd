using loxxking_backend_clean.Domain.Entities.SiteVisits;
using loxxking_backend_clean.Infrastructure.Persistence.Seeder.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace loxxking_backend_clean.Infrastructure.Persistence.Seeder.SiteVisits;

public class SiteVisitSeeder : IDataSeeder
{
    public int Order => 13;

    public async Task SeedAsync(SeedContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<SiteVisitSeeder>>();

        var countryId = context.DefaultCountry.Id;

        if (!await dbContext.SiteVisits.AnyAsync(cancellationToken))
        {
            var pages = new[] { "/home", "/products", "/categories/mens-apparel", "/offers" };
            foreach (var page in pages)
            {
                var visit = SiteVisit.Create(countryId, page);
                await dbContext.SiteVisits.AddAsync(visit, cancellationToken);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded sample SiteVisits for dashboard metrics.");
        }
    }
}
