using loxxking_backend_clean.Domain.Entities.Favorites;
using loxxking_backend_clean.Infrastructure.Persistence.Seeder.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace loxxking_backend_clean.Infrastructure.Persistence.Seeder.Favorites;

public class FavoriteSeeder : IDataSeeder
{
    public int Order => 10;

    public async Task SeedAsync(SeedContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<FavoriteSeeder>>();

        var watch = context.Products.FirstOrDefault(p => p.Slug == "minimalist-chronograph-watch");
        var loafers = context.Products.FirstOrDefault(p => p.Slug == "italian-leather-loafers");
        var customer = context.CustomerUser;

        if (watch != null && customer != null)
        {
            var userFavExists = await dbContext.FavoriteItems.AnyAsync(
                f => f.ProductId == watch.Id && f.UserId == customer.Id, 
                cancellationToken);

            if (!userFavExists)
            {
                var fav = FavoriteItem.Create(watch.Id, customer.Id, null);
                await dbContext.FavoriteItems.AddAsync(fav, cancellationToken);
                logger.LogInformation("Seeded customer favorite for product '{WatchSlug}'.", watch.Slug);
            }
        }

        if (loafers != null)
        {
            const string guestId = "guest-sess-abc-123";
            var guestFavExists = await dbContext.FavoriteItems.AnyAsync(
                f => f.ProductId == loafers.Id && f.GuestId == guestId, 
                cancellationToken);

            if (!guestFavExists)
            {
                var guestFav = FavoriteItem.Create(loafers.Id, null, guestId);
                await dbContext.FavoriteItems.AddAsync(guestFav, cancellationToken);
                logger.LogInformation("Seeded guest favorite for product '{LoafersSlug}'.", loafers.Slug);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
