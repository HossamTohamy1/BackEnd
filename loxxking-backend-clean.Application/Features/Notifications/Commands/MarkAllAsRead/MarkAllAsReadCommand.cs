namespace loxxking_backend_clean.Application.Features.Notifications.Commands.MarkAllAsRead;

public record MarkAllAsReadCommand(Guid UserId) : IRequest<Result<MarkAllAsReadResponse>>;

public record MarkAllAsReadResponse(int Updated);
