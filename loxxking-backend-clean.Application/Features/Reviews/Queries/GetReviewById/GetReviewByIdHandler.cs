namespace loxxking_backend_clean.Application.Features.Reviews.Queries.GetReviewById;

public class GetReviewByIdHandler : IRequestHandler<GetReviewByIdQuery, Result<GetReviewByIdResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetReviewByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetReviewByIdResponse>> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        var review = await _context.Reviews
            .Where(r => r.Id == request.Id)
            .Select(r => new GetReviewByIdResponse(
                r.Id,
                r.Rating.Value,
                r.Comment,
                r.CreatedAt,
                r.Status.ToString(),
                r.UserId,
                r.User != null ? r.User.Name : null
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (review is null)
        {
            return Result.Failure<GetReviewByIdResponse>(new Error("Error.NotFound", "Review_NotFound"));
        }

        return Result.Success(review);
    }
}
