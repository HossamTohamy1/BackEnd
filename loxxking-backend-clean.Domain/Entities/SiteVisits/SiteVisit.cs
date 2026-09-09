using loxxking_backend_clean.Domain.Entities.Countries;
namespace loxxking_backend_clean.Domain.Entities.SiteVisits;

public class SiteVisit : BaseEntity {
    public Guid? CountryId { get; private set; }
    public Country? Country { get; private set; }
    public string Page { get; private set; } = string.Empty;
    public DateTime VisitedAt { get; private set; }

    public string? IpAddress { get; private set; }

    protected SiteVisit() { }

    private SiteVisit(Guid? countryId, string page, string? ipAddress)
    {
        Id = Guid.NewGuid();
        CountryId = countryId;
        Page = page;
        IpAddress = ipAddress;
        VisitedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public static SiteVisit Create(Guid? countryId, string page, string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(page)) throw new ArgumentException("Domain_SiteVisit_PageRequired", nameof(page));

        return new SiteVisit(countryId, page, ipAddress);
    }
}
