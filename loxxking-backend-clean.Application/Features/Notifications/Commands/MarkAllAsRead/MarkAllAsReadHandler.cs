namespace loxxking_backend_clean.Application.Features.Notifications.Commands.MarkAllAsRead;

public class MarkAllAsReadHandler : IRequestHandler<MarkAllAsReadCommand, Result<MarkAllAsReadResponse>>
{
    private readonly IApplicationDbContext _context;

    public MarkAllAsReadHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<MarkAllAsReadResponse>> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == request.UserId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var n in unread)
        {
            n.MarkAsRead();
            _context.Notifications.Update(n);
        }
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new MarkAllAsReadResponse(unread.Count));
    }
}
