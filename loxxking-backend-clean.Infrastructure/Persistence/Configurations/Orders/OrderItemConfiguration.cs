using loxxking_backend_clean.Domain.Entities.Orders;

namespace loxxking_backend_clean.Infrastructure.Persistence.Configurations.Orders;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(o => o.PriceAtOrder);
    }
}
