namespace loxxking_backend_clean.Domain.Entities.Countries;

public class Country : BaseEntity
{
    public string Name { get; private set; }
    public string Currency { get; private set; }
    public string DefaultLanguage { get; private set; }
    public bool IsDefault { get; private set; }

    private Country() { } // EF Core

    public static Country Create(string name, string currency, string defaultLanguage = "en", bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Domain_Country_NameRequired", nameof(name));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Domain_Country_CurrencyRequired", nameof(currency));

        return new Country
        {
            Id = Guid.NewGuid(),
            Name = name,
            Currency = currency,
            DefaultLanguage = defaultLanguage,
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(string name, string currency, string defaultLanguage, bool isDefault)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Domain_Country_NameRequired", nameof(name));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Domain_Country_CurrencyRequired", nameof(currency));

        Name = name;
        Currency = currency;
        DefaultLanguage = defaultLanguage;
        IsDefault = isDefault;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
