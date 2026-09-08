namespace loxxking_backend_clean.Application.Features.Notifications.Commands.DeleteNotification;

public record DeleteNotificationCommand(Guid Id, Guid UserId) : IRequest<Result>;

public class DeleteNotificationHandler : IRequestHandler<DeleteNotificationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeleteNotificationHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var n = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId, cancellationToken);
        if (n == null) return Result.Failure(new Error("Error.NotFound", "Notification_NotFound"));
        _context.Notifications.Remove(n);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
