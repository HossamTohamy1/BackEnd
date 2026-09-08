using loxxking_backend_clean.Domain.Entities.Products;
using loxxking_backend_clean.Domain.Entities.Users;
namespace loxxking_backend_clean.Domain.Entities.Favorites;

public class FavoriteItem : BaseEntity {
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }
    public string? GuestId { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public DateTime AddedAt { get; private set; }

    protected FavoriteItem() { }

    private FavoriteItem(Guid? userId, string? guestId, Guid productId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        GuestId = guestId;
        ProductId = productId;
        AddedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public static FavoriteItem Create(Guid productId, Guid? userId, string? guestId)
    {
        if (productId == Guid.Empty) throw new ArgumentException("Domain_Favorite_ProductIdRequired", nameof(productId));
        if (userId == null && string.IsNullOrWhiteSpace(guestId))
            throw new ArgumentException("Domain_Favorite_UserOrGuestRequired");

        return new FavoriteItem(userId, guestId, productId);
    }

    public void AssignToUser(Guid userId)
    {
        if (userId == Guid.Empty) throw new ArgumentException("Domain_Favorite_UserIdRequired", nameof(userId));
        
        UserId = userId;
        GuestId = null; // Clear the guest id since it's now associated with a user
        UpdatedAt = DateTime.UtcNow;
    }
}
