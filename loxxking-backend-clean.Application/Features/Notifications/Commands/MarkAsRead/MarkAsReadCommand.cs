namespace loxxking_backend_clean.Application.Features.Notifications.Commands.MarkAsRead;

public record MarkAsReadCommand(Guid Id, Guid UserId) : IRequest<Result>;
