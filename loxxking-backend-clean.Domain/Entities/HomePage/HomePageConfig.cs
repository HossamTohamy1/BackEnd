using loxxking_backend_clean.Domain.Common;

namespace loxxking_backend_clean.Domain.Entities.HomePage;

public class HomePageConfig : BaseEntity
{
    public string SectionsJson { get; set; } = "[]";
    public int Version { get; set; } = 1;
    public string? LastModifiedBy { get; set; }
}
