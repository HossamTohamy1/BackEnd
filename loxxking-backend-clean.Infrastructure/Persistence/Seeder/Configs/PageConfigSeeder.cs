using loxxking_backend_clean.Domain.Entities.Favorites;
using loxxking_backend_clean.Domain.Entities.Offers;
using loxxking_backend_clean.Infrastructure.Persistence.Seeder.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace loxxking_backend_clean.Infrastructure.Persistence.Seeder.Configs;

public class PageConfigSeeder : IDataSeeder
{
    public int Order => 6;

    public async Task SeedAsync(SeedContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<PageConfigSeeder>>();

        if (!await dbContext.OffersPageConfigs.AnyAsync(cancellationToken))
        {
            var offersConfig = new OffersPageConfig
            {
                HeroTitle = "عروض الموسم الحصرية | Exclusive Seasonal Offers",
                HeroSubtitle = "اكتشف أفضل الخصومات على تشكيلاتنا المختارة بعناية",
                ShowHero = true,
                CurrentOffersTitle = "عروض محدودة الوقت",
                BundlesTitle = "باقات التوفير المميزة",
                BundlesSubtitle = "وفّر أكثر عند شراء مجموعات متناسقة معًا"
            };
            await dbContext.OffersPageConfigs.AddAsync(offersConfig, cancellationToken);
            logger.LogInformation("Seeded OffersPageConfig.");
        }

        if (!await dbContext.FavoritesPageConfigs.AnyAsync(cancellationToken))
        {
            var favoritesConfig = new FavoritesPageConfig
            {
                ShowTitle = true,
                HeaderTitle = "قائمة أمنياتي",
                HeaderSubtitle = "منتجاتك المفضلة المحفوظة للشراء لاحقاً",
                ShowAddAllToCart = true,
                AddAllToCartText = "إضافة الكل إلى سلة التسوق",
                ShowToolbar = true,
                ShowSort = true,
                ShowCount = true,
                ShowProductColor = true,
                ShowProductSize = true,
                ShowProductPrice = true,
                ShowProductOldPrice = true,
                ShowProductStock = true,
                ShowRemoveAction = true,
                ShowMoveToCartAction = true,
                EmptyStateTitle = "قائمة المفضلة فارغة",
                EmptyStateSubtitle = "تصفح متجرنا وأضف ما يعجبك بضغطة واحدة!",
                EmptyStateButtonText = "ابدأ التسوق الآن",
                ShowEmptyStateIllustration = true,
                ShowTrustBadges = true,
                TrustBadgesJson = "[]"
            };
            await dbContext.FavoritesPageConfigs.AddAsync(favoritesConfig, cancellationToken);
            logger.LogInformation("Seeded FavoritesPageConfig.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
