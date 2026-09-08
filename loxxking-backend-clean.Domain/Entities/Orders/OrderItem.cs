using loxxking_backend_clean.Domain.Common;
using loxxking_backend_clean.Domain.Entities.Products;
namespace loxxking_backend_clean.Domain.Entities.Orders;

public class OrderItem : BaseEntity {
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal PriceAtOrder { get; set; }
}
