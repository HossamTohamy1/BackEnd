namespace loxxking_backend_clean.Application.Features.Notifications.Queries.GetUnreadCount;

public record GetUnreadCountQuery(Guid UserId) : IRequest<Result<GetUnreadCountResponse>>;

public record GetUnreadCountResponse(int UnreadCount);
