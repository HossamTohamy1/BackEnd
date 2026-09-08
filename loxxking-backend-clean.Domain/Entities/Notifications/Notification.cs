namespace loxxking_backend_clean.Domain.Entities.Notifications;

public class Notification : BaseEntity {
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public Guid? RelatedEntityId { get; private set; }
    public bool IsRead { get; private set; } = false;

    protected Notification() { }

    private Notification(Guid userId, NotificationType type, string message, Guid? relatedEntityId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Type = type;
        Message = message;
        RelatedEntityId = relatedEntityId;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public static Notification Create(Guid userId, NotificationType type, string message, Guid? relatedEntityId = null)
    {
        if (userId == Guid.Empty) throw new ArgumentException("Domain_Notification_UserIdRequired", nameof(userId));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Domain_Notification_MessageRequired", nameof(message));

        return new Notification(userId, type, message, relatedEntityId);
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
