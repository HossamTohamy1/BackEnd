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
        var firstDayOfMonth = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var todayCount = await _context.SiteVisits
            .Where(sv => sv.VisitedAt >= today && sv.VisitedAt < today.AddDays(1))
            .CountAsync(cancellationToken);

        var uniqueTodayCount = await _context.SiteVisits
            .Where(sv => sv.VisitedAt >= today && sv.VisitedAt < today.AddDays(1) && sv.IpAddress != null)
            .Select(sv => sv.IpAddress)
            .Distinct()
            .CountAsync(cancellationToken);

        var thisMonthCount = await _context.SiteVisits
            .Where(sv => sv.VisitedAt >= firstDayOfMonth)
            .CountAsync(cancellationToken);

        var totalVisits = await _context.SiteVisits.CountAsync(cancellationToken);

        var totalUniqueVisits = await _context.SiteVisits
            .Where(sv => sv.IpAddress != null)
            .Select(sv => sv.IpAddress)
            .Distinct()
            .CountAsync(cancellationToken);

        return Result.Success(new GetTodayCountResponse(
            todayCount, 
            uniqueTodayCount, 
            totalVisits, 
            totalUniqueVisits, 
            thisMonthCount));
    }
}
