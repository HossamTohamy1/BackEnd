using loxxking_backend_clean.Domain.Entities.Reviews;

namespace loxxking_backend_clean.Infrastructure.Persistence.Configurations.Reviews;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(r => r.Rating)
            .HasConversion(
                v => v.Value,
                v => loxxking_backend_clean.Domain.ValueObjects.RatingScore.FromInt(v))
            .IsRequired();

        builder.HasIndex(r => new { r.ProductId, r.Status });
    }
}
