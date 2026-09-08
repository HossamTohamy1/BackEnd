using loxxking_backend_clean.Domain.Entities.Countries;
using loxxking_backend_clean.Infrastructure.Persistence.Seeder.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace loxxking_backend_clean.Infrastructure.Persistence.Seeder.Countries;

public class CountrySeeder : IDataSeeder
{
    public int Order => 2;

    public async Task SeedAsync(SeedContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<CountrySeeder>>();

        var targetCountries = new (string Name, string Currency, string DefaultLanguage, bool IsDefault)[]
        {
            ("Egypt", "EGP", "ar", true),
            ("Saudi Arabia", "SAR", "ar", false),
            ("United Arab Emirates", "AED", "en", false)
        };

        foreach (var (name, currency, defaultLang, isDefault) in targetCountries)
        {
            var existing = await dbContext.Countries.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
            if (existing == null)
            {
                var country = Country.Create(name, currency, defaultLang, isDefault);
                await dbContext.Countries.AddAsync(country, cancellationToken);
                logger.LogInformation("Seeded country '{CountryName}' ({Currency}).", name, currency);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var allCountries = await dbContext.Countries.ToListAsync(cancellationToken);
        context.Countries = allCountries;
        context.DefaultCountry = allCountries.FirstOrDefault(c => c.IsDefault) ?? allCountries.First();
    }
}
