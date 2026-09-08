using loxxking_backend_clean.Domain.Entities.Products;

namespace loxxking_backend_clean.Infrastructure.Persistence.Configurations.Products;

public class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Price);
    }
}
