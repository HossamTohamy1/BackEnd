namespace loxxking_backend_clean.Application.Features.Reviews.Commands.CreateReview;

public record CreateReviewCommand(
    Guid ProductId,
    int Rating,
    string Comment,
    string? GuestName,
    Guid UserId
) : IRequest<Result<CreateReviewResponse>>;

public record CreateReviewResponse(Guid Id);
