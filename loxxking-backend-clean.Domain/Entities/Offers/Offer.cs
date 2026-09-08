using loxxking_backend_clean.Domain.Entities.Products;
using loxxking_backend_clean.Domain.ValueObjects;

namespace loxxking_backend_clean.Domain.Entities.Offers;

public class Offer : BaseEntity 
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    
    public Percentage Discount { get; private set; } = null!;
    public DateRange ActivePeriod { get; private set; } = null!;

    protected Offer() { }

    private Offer(Guid productId, Percentage discount, DateRange activePeriod)
    {
        ProductId = productId;
        Discount = discount;
        ActivePeriod = activePeriod;
    }

    public static Offer Create(Guid productId, Percentage discount, DateRange activePeriod)
    {
        if (productId == Guid.Empty) throw new ArgumentException("Domain_Offer_ProductIdRequired", nameof(productId));

        return new Offer(productId, discount, activePeriod);
    }

    public void UpdateDiscount(Percentage discount)
    {
        Discount = discount;
    }

    public void UpdatePeriod(DateRange period)
    {
        ActivePeriod = period;
    }
}
