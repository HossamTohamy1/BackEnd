namespace loxxking_backend_clean.Application.Common.Interfaces;

public interface ISupportNotificationService
{
    Task NotifyMessageReceivedAsync(string conversationId, Guid? userId, string? userName, string message, DateTime timestamp);
}
