using loxxking_backend_clean.Domain.Common;
using loxxking_backend_clean.Domain.Entities.Countries;
namespace loxxking_backend_clean.Domain.Entities.Products;

public class ProductPrice : BaseEntity {
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid CountryId { get; set; }
    public Country Country { get; set; } = null!;
    public decimal Price { get; set; }
}
