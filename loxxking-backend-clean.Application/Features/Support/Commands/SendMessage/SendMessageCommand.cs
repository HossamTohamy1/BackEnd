namespace loxxking_backend_clean.Application.Features.Support.Commands.SendMessage;

public record SendMessageCommand(
    Guid ConversationId,
    string Message,
    Guid UserId,
    string? GuestName,
    bool IsStaff,
    string? GuestId = null
) : IRequest<Result<SendMessageResponse>>;

public record SendMessageResponse(Guid Id, Guid ConversationId, string SenderType, string SenderName, string Message, DateTime CreatedAt);
