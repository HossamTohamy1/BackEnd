namespace loxxking_backend_clean.Application.Common.Interfaces;

public record OrderNotificationData(
    string OrderNumber,
    string CustomerName,
    string CustomerPhone,
    string Address,
    string Country,
    string PaymentMethod,
    decimal TotalAmount,
    List<OrderNotificationItem> Items,
    DateTime CreatedAt,
    byte[]? PdfAttachment = null,
    string Language = "ar"
);

public record OrderNotificationItem(string ProductName, int Quantity, decimal UnitPrice);

public interface IOrderNotificationService
{
    Task NotifyNewOrderAsync(OrderNotificationData order, CancellationToken cancellationToken = default);
}
