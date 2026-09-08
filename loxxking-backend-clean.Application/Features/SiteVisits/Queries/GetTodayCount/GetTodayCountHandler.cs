namespace loxxking_backend_clean.Application.Features.SiteVisits.Queries.GetTodayCount;

public class GetTodayCountHandler : IRequestHandler<GetTodayCountQuery, Result<GetTodayCountResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetTodayCountHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetTodayCountResponse>> Handle(GetTodayCountQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var count = await _context.SiteVisits
            .Where(sv => sv.VisitedAt >= today && sv.VisitedAt < today.AddDays(1))
            .CountAsync(cancellationToken);

        return Result.Success(new GetTodayCountResponse(count));
    }
}
