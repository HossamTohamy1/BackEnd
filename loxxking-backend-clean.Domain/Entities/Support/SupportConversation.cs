namespace loxxking_backend_clean.Domain.Entities.Support;
public class SupportConversation : BaseEntity {
    public string OrderNumber { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;
    public string? CustomerEmail { get; private set; }
    public Guid? AssignedTo { get; private set; }
    public SupportStatus Status { get; private set; } = SupportStatus.Open;
    
    private readonly List<SupportMessage> _messages = new();
    public IReadOnlyCollection<SupportMessage> Messages => _messages.AsReadOnly();

    protected SupportConversation() { }

    public static SupportConversation Create(string orderNumber, string customerName, string customerPhone, string? customerEmail)
    {
        return new SupportConversation
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            CustomerEmail = customerEmail,
            Status = SupportStatus.Open,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddMessage(
        Guid? senderId,
        Guid? recipientId,
        string message,
        string? attachmentUrl,
        Guid? relatedOrderId,
        Guid? relatedReviewId,
        string? guestName)
    {
        _messages.Add(new SupportMessage {
            SenderId = senderId,
            RecipientId = recipientId,
            Message = message,
            AttachmentUrl = attachmentUrl,
            RelatedOrderId = relatedOrderId,
            RelatedReviewId = relatedReviewId,
            GuestName = guestName
        });
    }

    public void ChangeStatus(SupportStatus newStatus)
    {
        Status = newStatus;
    }

    public void AssignTo(Guid staffId)
    {
        AssignedTo = staffId;
    }
}
