using loxxking_backend_clean.Domain.Entities.Orders;
namespace loxxking_backend_clean.Domain.Entities.Invoices;

public class Invoice : BaseEntity {
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public string InvoiceNumber { get; private set; } = string.Empty;
    public string PdfUrl { get; private set; } = string.Empty;
    public loxxking_backend_clean.Domain.ValueObjects.Money TotalAmount { get; private set; } = null!;
    public DateTime IssuedAt { get; private set; }

    protected Invoice() { }

    private Invoice(Guid orderId, string invoiceNumber, loxxking_backend_clean.Domain.ValueObjects.Money totalAmount)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        InvoiceNumber = invoiceNumber;
        TotalAmount = totalAmount;
        IssuedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public static Invoice Create(Guid orderId, string invoiceNumber, loxxking_backend_clean.Domain.ValueObjects.Money totalAmount)
    {
        if (orderId == Guid.Empty) throw new ArgumentException("Domain_Invoice_OrderIdRequired", nameof(orderId));
        if (string.IsNullOrWhiteSpace(invoiceNumber)) throw new ArgumentException("Domain_Invoice_InvoiceNumberRequired", nameof(invoiceNumber));
        if (totalAmount == null) throw new ArgumentNullException(nameof(totalAmount), "Domain_Invoice_TotalAmountRequired");

        return new Invoice(orderId, invoiceNumber, totalAmount);
    }
}
