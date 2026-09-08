using loxxking_backend_clean.Domain.Entities.Offers;

namespace loxxking_backend_clean.Infrastructure.Persistence.Configurations.Offers;

public class BundleOfferConfiguration : IEntityTypeConfiguration<BundleOffer>
{
    public void Configure(EntityTypeBuilder<BundleOffer> builder)
    {
        builder.Property(o => o.BundlePrice)
            .HasConversion(
                m => m.Value,
                v => loxxking_backend_clean.Domain.ValueObjects.Money.UnsafeFromDatabase(v))
            .HasColumnName("BundlePrice");

        builder.OwnsOne(o => o.ActivePeriod, ap =>
        {
            ap.Property(p => p.StartDate).HasColumnName("StartDate");
            ap.Property(p => p.EndDate).HasColumnName("EndDate");
        });
    }
}
