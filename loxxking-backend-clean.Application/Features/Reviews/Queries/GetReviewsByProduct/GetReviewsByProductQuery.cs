namespace loxxking_backend_clean.Application.Features.Reviews.Queries.GetReviewsByProduct;

public record GetReviewsByProductQuery(Guid ProductId, Guid? CurrentUserId = null, string? GuestId = null) : IRequest<Result<List<GetReviewsByProductResponse>>>;

public record GetReviewsByProductResponse(
    Guid Id,
    int Rating,
    string Comment,
    DateTime CreatedAt,
    string? GuestName,
    ReviewUserDto? User,
    bool SupportContacted,
    bool IsPending = false
);

public record ReviewUserDto(Guid Id, string Name);
