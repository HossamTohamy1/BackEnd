namespace loxxking_backend_clean.Application.Features.SiteVisits.Queries.GetTodayCount;

public record GetTodayCountQuery() : IRequest<Result<GetTodayCountResponse>>;

public record GetTodayCountResponse(
    int TodayCount, 
    int UniqueTodayCount, 
    int TotalVisits, 
    int TotalUniqueVisits, 
    int ThisMonthCount);
