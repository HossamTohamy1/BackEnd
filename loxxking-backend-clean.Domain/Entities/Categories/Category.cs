namespace loxxking_backend_clean.Domain.Entities.Categories;

public class Category : BaseEntity
{
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    
    public string Slug { get; private set; } = string.Empty;
    public string ImageUrl { get; private set; } = string.Empty;

    protected Category() { }

    private Category(string nameAr, string nameEn, string slug, string imageUrl)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        Slug = slug;
        ImageUrl = imageUrl;
    }

    public static Category Create(string nameAr, string nameEn, string slug, string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new ArgumentException("Domain_Category_NameArRequired", nameof(nameAr));
        if (string.IsNullOrWhiteSpace(nameEn)) throw new ArgumentException("Domain_Category_NameEnRequired", nameof(nameEn));
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Domain_Category_SlugRequired", nameof(slug));

        return new Category(nameAr, nameEn, slug, imageUrl);
    }

    public void UpdateDetails(string nameAr, string nameEn, string slug, string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new ArgumentException("Domain_Category_NameArRequired", nameof(nameAr));
        if (string.IsNullOrWhiteSpace(nameEn)) throw new ArgumentException("Domain_Category_NameEnRequired", nameof(nameEn));
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Domain_Category_SlugRequired", nameof(slug));

        NameAr = nameAr;
        NameEn = nameEn;
        Slug = slug;
        ImageUrl = imageUrl;
    }
}
