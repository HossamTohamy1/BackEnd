namespace loxxking_backend_clean.Application.Features.Support.Queries.GetMessages;

public record GetMessagesQuery(Guid ConversationId) : IRequest<Result<List<GetMessagesResponse>>>;

public record GetMessagesResponse(
    Guid Id,
    Guid ConversationId,
    string Message,
    DateTime CreatedAt,
    bool IsRead,
    string SenderType,
    string SenderName
);
