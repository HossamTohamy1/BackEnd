namespace loxxking_backend_clean.Application.Features.Reviews.Queries.GetReviewsByProduct;

public record GetReviewsByProductQuery(Guid ProductId) : IRequest<Result<List<GetReviewsByProductResponse>>>;

public record GetReviewsByProductResponse(
    Guid Id,
    int Rating,
    string Comment,
    DateTime CreatedAt,
    string? GuestName,
    ReviewUserDto? User
);

public record ReviewUserDto(Guid Id, string Name);
