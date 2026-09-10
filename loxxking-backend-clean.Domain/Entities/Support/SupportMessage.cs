using loxxking_backend_clean.Domain.Common;
using loxxking_backend_clean.Domain.Entities.Users;
namespace loxxking_backend_clean.Domain.Entities.Support;

public class SupportMessage : BaseEntity {
    public Guid ConversationId { get; set; }
    public Guid? SenderId { get; set; }
    public User? Sender { get; set; }
    public Guid? RecipientId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public Guid? RelatedOrderId { get; set; }
    public Guid? RelatedReviewId { get; set; }
    public string? GuestName { get; set; }
    public bool IsRead { get; set; }

    // CRM Integration Fields
    public string? ClientMessageId { get; set; } // For idempotency from frontend
    public bool IsSyncedToCrm { get; set; }
    public DateTime? SyncedToCrmAt { get; set; }
    public int SyncAttempts { get; set; }
    public string? SyncError { get; set; }
}
