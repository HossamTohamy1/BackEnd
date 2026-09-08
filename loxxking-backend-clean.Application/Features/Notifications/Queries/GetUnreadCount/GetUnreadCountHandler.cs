namespace loxxking_backend_clean.Application.Features.Notifications.Queries.GetUnreadCount;

public class GetUnreadCountHandler : IRequestHandler<GetUnreadCountQuery, Result<GetUnreadCountResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetUnreadCountHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetUnreadCountResponse>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _context.Notifications
            .CountAsync(n => n.UserId == request.UserId && !n.IsRead, cancellationToken);

        return Result.Success(new GetUnreadCountResponse(count));
    }
}
