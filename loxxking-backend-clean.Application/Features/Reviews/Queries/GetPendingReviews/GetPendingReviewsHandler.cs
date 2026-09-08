namespace loxxking_backend_clean.Application.Features.Reviews.Queries.GetPendingReviews;

public class GetPendingReviewsHandler : IRequestHandler<GetPendingReviewsQuery, Result<List<GetPendingReviewsResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingReviewsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<GetPendingReviewsResponse>>> Handle(GetPendingReviewsQuery request, CancellationToken cancellationToken)
    {
        var pending = await _context.Reviews
            .Where(r => r.Status == ReviewStatus.Pending)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new GetPendingReviewsResponse(
                r.Id,
                r.Rating.Value,
                r.Comment,
                r.CreatedAt,
                r.UserId,
                r.User != null ? r.User.Name : null
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(pending);
    }
}
