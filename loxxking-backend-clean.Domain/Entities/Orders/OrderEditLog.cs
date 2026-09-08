using loxxking_backend_clean.Domain.Entities.Users;
namespace loxxking_backend_clean.Domain.Entities.Orders;

public class OrderEditLog : BaseEntity {
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid EditedBy { get; set; }
    public User Editor { get; set; } = null!;
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime EditedAt { get; set; } = DateTime.UtcNow;
}
