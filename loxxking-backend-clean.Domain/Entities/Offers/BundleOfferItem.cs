using loxxking_backend_clean.Domain.Entities.Products;

namespace loxxking_backend_clean.Domain.Entities.Offers;

public class BundleOfferItem : BaseEntity
{
    public Guid BundleOfferId { get; private set; }
    public BundleOffer BundleOffer { get; private set; } = null!;
    
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    
    public int Quantity { get; private set; }

    protected BundleOfferItem() { }

    internal static BundleOfferItem Create(Guid bundleOfferId, Guid productId, int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Domain_BundleOffer_QuantityGreaterThanZero", nameof(quantity));

        return new BundleOfferItem
        {
            BundleOfferId = bundleOfferId,
            ProductId = productId,
            Quantity = quantity
        };
    }
}
