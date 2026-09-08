using loxxking_backend_clean.Domain.Entities.Categories;
using loxxking_backend_clean.Domain.Entities.Products;
using loxxking_backend_clean.Domain.Entities.Countries;
using loxxking_backend_clean.Domain.Entities.BankTransfers;
using loxxking_backend_clean.Domain.Entities.Inventory;
using loxxking_backend_clean.Domain.Entities.Invoices;
using loxxking_backend_clean.Domain.Entities.Notifications;
using loxxking_backend_clean.Domain.Entities.Offers;
using loxxking_backend_clean.Domain.Entities.Orders;
using loxxking_backend_clean.Domain.Entities.Reviews;
using loxxking_backend_clean.Domain.Entities.SiteVisits;
using loxxking_backend_clean.Domain.Entities.Support;
using loxxking_backend_clean.Domain.Entities.Users;
using loxxking_backend_clean.Domain.Entities.Favorites;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace loxxking_backend_clean.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<Country> Countries { get; }
    DbSet<BankTransfer> BankTransfers { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Offer> Offers { get; }
    DbSet<BundleOffer> BundleOffers { get; }
    DbSet<BundleOfferItem> BundleOfferItems { get; }
    DbSet<OffersPageConfig> OffersPageConfigs { get; }
    DbSet<OfferProduct> OfferProducts { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderEditLog> OrderEditLogs { get; }
    DbSet<ProductPrice> ProductPrices { get; }
    DbSet<Review> Reviews { get; }
    DbSet<SiteVisit> SiteVisits { get; }
    DbSet<SupportConversation> SupportConversations { get; }
    DbSet<SupportMessage> SupportMessages { get; }
    DbSet<User> Users { get; }
    DbSet<FavoriteItem> FavoriteItems { get; }
    DbSet<FavoritesPageConfig> FavoritesPageConfigs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class;

    DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

    DatabaseFacade Database { get; }
}
