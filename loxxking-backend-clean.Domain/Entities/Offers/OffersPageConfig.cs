namespace loxxking_backend_clean.Domain.Entities.Offers;

public class OffersPageConfig : BaseEntity
{
    public string HeroTitle { get; set; } = string.Empty;
    public string HeroSubtitle { get; set; } = string.Empty;
    public bool ShowHero { get; set; } = true;
    public string CurrentOffersTitle { get; set; } = string.Empty;
    public string BundlesTitle { get; set; } = string.Empty;
    public string BundlesSubtitle { get; set; } = string.Empty;
}
