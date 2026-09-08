using loxxking_backend_clean.Domain.Entities.Offers;
using loxxking_backend_clean.Domain.ValueObjects;
using loxxking_backend_clean.Infrastructure.Persistence.Seeder.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace loxxking_backend_clean.Infrastructure.Persistence.Seeder.Offers;

public class OfferSeeder : IDataSeeder
{
    public int Order => 7;

    public async Task SeedAsync(SeedContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<OfferSeeder>>();

        var shirt = context.Products.FirstOrDefault(p => p.Slug == "oxford-cotton-shirt");
        var loafers = context.Products.FirstOrDefault(p => p.Slug == "italian-leather-loafers");

        if (shirt != null)
        {
            var offerExists = await dbContext.Offers.AnyAsync(o => o.ProductId == shirt.Id, cancellationToken);
            if (!offerExists)
            {
                var offer = Offer.Create(
                    productId: shirt.Id,
                    discount: Percentage.FromDecimal(20m),
                    activePeriod: DateRange.Create(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(30)));

                await dbContext.Offers.AddAsync(offer, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                var offerProduct = new OfferProduct
                {
                    OfferId = offer.Id,
                    ProductId = shirt.Id,
                    Quantity = 1,
                    DiscountPercentage = 20m
                };
                await dbContext.OfferProducts.AddAsync(offerProduct, cancellationToken);

                logger.LogInformation("Seeded single offer for product '{ShirtSlug}' (20% discount).", shirt.Slug);
            }
        }

        if (shirt != null && loafers != null)
        {
            const string bundleTitle = "باقة الأناقة الكاملة | Complete Elegance Bundle";
            var bundleExists = await dbContext.BundleOffers.AnyAsync(b => b.Title == bundleTitle, cancellationToken);
            if (!bundleExists)
            {
                var bundleOffer = BundleOffer.Create(
                    title: bundleTitle,
                    subtitle: "قميص أكسفورد + حذاء إيطالي فاخر مع خصم إضافي 15%",
                    bundlePrice: Money.FromDecimal(3400m),
                    imageUrl: "https://images.unsplash.com/photo-1490481651871-ab68de25d43d",
                    activePeriod: DateRange.Create(DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(60)));

                bundleOffer.AddItem(shirt.Id, 1);
                bundleOffer.AddItem(loafers.Id, 1);

                await dbContext.BundleOffers.AddAsync(bundleOffer, cancellationToken);
                logger.LogInformation("Seeded BundleOffer '{BundleTitle}'.", bundleTitle);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
