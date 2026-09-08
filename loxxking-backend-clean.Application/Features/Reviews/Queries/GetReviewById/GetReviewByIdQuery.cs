namespace loxxking_backend_clean.Application.Features.Reviews.Queries.GetReviewById;

public record GetReviewByIdQuery(Guid Id) : IRequest<Result<GetReviewByIdResponse>>;

public record GetReviewByIdResponse(
    Guid Id,
    int Rating,
    string Comment,
    DateTime CreatedAt,
    string Status,
    Guid? UserId,
    string? UserName
);
