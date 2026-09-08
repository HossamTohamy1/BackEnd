using loxxking_backend_clean.Domain.ValueObjects;

namespace loxxking_backend_clean.Domain.Entities.Offers;

public class BundleOffer : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Subtitle { get; private set; } = string.Empty;
    
    public Money BundlePrice { get; private set; } = null!;
    
    public string ImageUrl { get; private set; } = string.Empty; 
    public DateRange ActivePeriod { get; private set; } = null!;
    
    private readonly List<BundleOfferItem> _items = new();
    public IReadOnlyCollection<BundleOfferItem> Items => _items.AsReadOnly();

    protected BundleOffer() { }

    private BundleOffer(string title, string subtitle, Money bundlePrice, string imageUrl, DateRange activePeriod)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Domain_BundleOffer_TitleRequired", nameof(title));
        
        Title = title;
        Subtitle = subtitle;
        BundlePrice = bundlePrice;
        ImageUrl = imageUrl;
        ActivePeriod = activePeriod;
    }

    public static BundleOffer Create(string title, string subtitle, Money bundlePrice, string imageUrl, DateRange activePeriod)
    {
        return new BundleOffer(title, subtitle, bundlePrice, imageUrl, activePeriod);
    }

    public void UpdateDetails(string title, string subtitle, Money bundlePrice, string imageUrl, DateRange activePeriod)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Domain_BundleOffer_TitleRequired", nameof(title));

        Title = title;
        Subtitle = subtitle;
        BundlePrice = bundlePrice;
        ImageUrl = imageUrl;
        ActivePeriod = activePeriod;
    }

    public void AddItem(Guid productId, int quantity)
    {
        _items.Add(BundleOfferItem.Create(Id, productId, quantity));
    }

    public void ClearItems()
    {
        _items.Clear();
    }
}
