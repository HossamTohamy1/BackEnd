using loxxking_backend_clean.Domain.Entities.Products;
namespace loxxking_backend_clean.Domain.Entities.Offers;
public class OfferProduct : BaseEntity {
    public Guid OfferId { get; set; }
    public Offer Offer { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; } = 1;
    public decimal DiscountPercentage { get; set; }
}
