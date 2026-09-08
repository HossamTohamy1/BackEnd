namespace loxxking_backend_clean.Application.Features.Reviews.Queries.GetPendingReviews;

public record GetPendingReviewsQuery() : IRequest<Result<List<GetPendingReviewsResponse>>>;

public record GetPendingReviewsResponse(
    Guid Id,
    int Rating,
    string Comment,
    DateTime CreatedAt,
    Guid? UserId,
    string? UserName
);
