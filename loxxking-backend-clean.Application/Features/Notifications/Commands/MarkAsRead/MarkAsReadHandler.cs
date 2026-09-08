namespace loxxking_backend_clean.Application.Features.Notifications.Commands.MarkAsRead;

public class MarkAsReadHandler : IRequestHandler<MarkAsReadCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public MarkAsReadHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken);

        if (notification is null || notification.UserId != request.UserId)
        {
            return Result.Failure(new Error("Error.NotFound", "Notification_NotFound"));
        }

        notification.MarkAsRead();
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
