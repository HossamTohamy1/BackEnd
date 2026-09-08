namespace loxxking_backend_clean.Application.Features.Notifications.Queries.GetMyNotifications;

public class GetMyNotificationsHandler : IRequestHandler<GetMyNotificationsQuery, Result<List<GetMyNotificationsResponse>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource>? _localizer;

    public GetMyNotificationsHandler(IApplicationDbContext context, IStringLocalizer<SharedResource>? localizer = null)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<List<GetMyNotificationsResponse>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var rawNotifications = await _context.Notifications
            .Where(n => n.UserId == request.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

        var notifications = rawNotifications
            .Select(n => new GetMyNotificationsResponse(
                n.Id,
                n.Type.ToString(),
                _localizer.Get(n.Message, n.Message),
                n.RelatedEntityId,
                n.IsRead,
                n.CreatedAt
            ))
            .ToList();

        return Result.Success(notifications);
    }
}
