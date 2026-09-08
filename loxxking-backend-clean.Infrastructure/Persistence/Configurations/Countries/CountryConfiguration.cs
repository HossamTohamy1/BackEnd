using loxxking_backend_clean.Domain.Entities.Countries;

namespace loxxking_backend_clean.Infrastructure.Persistence.Configurations.Countries;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Currency).IsRequired().HasMaxLength(10);
    }
}
