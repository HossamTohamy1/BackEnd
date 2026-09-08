namespace loxxking_backend_clean.Application.Features.SiteVisits.Queries.GetVisits;

public class GetVisitsHandler : IRequestHandler<GetVisitsQuery, Result<GetVisitsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetVisitsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetVisitsResponse>> Handle(GetVisitsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        var query = _context.SiteVisits.AsQueryable();

        if (request.CountryId.HasValue)
            query = query.Where(sv => sv.CountryId == request.CountryId.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(sv => sv.VisitedAt >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(sv => sv.VisitedAt <= request.DateTo.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var data = await query
            .OrderByDescending(sv => sv.VisitedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(sv => new
            {
                sv.Id,
                CountryName = sv.Country.Name,
                sv.Page,
                sv.VisitedAt
            })
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Result.Success(new GetVisitsResponse(data, totalCount, page, pageSize, totalPages));
    }
}
