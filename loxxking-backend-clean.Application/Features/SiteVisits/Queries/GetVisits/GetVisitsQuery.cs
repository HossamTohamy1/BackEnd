namespace loxxking_backend_clean.Application.Features.SiteVisits.Queries.GetVisits;

public record GetVisitsQuery(
    Guid? CountryId,
    DateTime? DateFrom,
    DateTime? DateTo,
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<GetVisitsResponse>>;

public record GetVisitsResponse(
    object Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
