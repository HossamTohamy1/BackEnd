using loxxking_backend_clean.Domain.Entities.Products;
using loxxking_backend_clean.Domain.Entities.Users;
using loxxking_backend_clean.Domain.ValueObjects;

namespace loxxking_backend_clean.Domain.Entities.Reviews;

public class Review : BaseEntity {
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }
    public string? GuestName { get; private set; }
    public RatingScore Rating { get; private set; } = null!;
    public string Comment { get; private set; } = string.Empty;
    public ReviewStatus Status { get; private set; }

    protected Review() { }

    public static Review Create(Guid productId, Guid? userId, string? guestName, RatingScore rating, string comment)
    {
        if (!userId.HasValue && string.IsNullOrWhiteSpace(guestName))
        {
            throw new ArgumentException("Domain_Review_GuestNameRequired");
        }

        return new Review
        {
            ProductId = productId,
            UserId = userId,
            GuestName = !userId.HasValue ? guestName : null,
            Rating = rating,
            Comment = comment ?? string.Empty,
            Status = ReviewStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Approve()
    {
        Status = ReviewStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(RatingScore rating, string comment)
    {
        Rating = rating;
        Comment = comment ?? string.Empty;
        Status = ReviewStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = ReviewStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Hide()
    {
        Status = ReviewStatus.Hidden;
        UpdatedAt = DateTime.UtcNow;
    }
}
