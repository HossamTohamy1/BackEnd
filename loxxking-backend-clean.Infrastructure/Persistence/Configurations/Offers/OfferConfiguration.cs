using loxxking_backend_clean.Domain.Entities.Offers;

namespace loxxking_backend_clean.Infrastructure.Persistence.Configurations.Offers;

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.Property(o => o.Discount)
            .HasConversion(
                p => p.Value,
                v => loxxking_backend_clean.Domain.ValueObjects.Percentage.UnsafeFromDatabase(v))
            .HasColumnName("DiscountPercent");

        builder.OwnsOne(o => o.ActivePeriod, ap =>
        {
            ap.Property(p => p.StartDate).HasColumnName("StartDate");
            ap.Property(p => p.EndDate).HasColumnName("EndDate");
            ap.HasIndex(p => new { p.EndDate, p.StartDate });
        });
    }
}
