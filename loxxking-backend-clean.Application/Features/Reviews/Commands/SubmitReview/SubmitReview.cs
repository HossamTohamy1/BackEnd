using loxxking_backend_clean.Domain.Entities.Reviews;

namespace loxxking_backend_clean.Application.Features.Reviews.Commands.SubmitReview;

public record SubmitReviewCommand(Guid ProductId, Guid UserId, int Rating, string Comment) : IRequest<Result>;

public class SubmitReviewHandler : IRequestHandler<SubmitReviewCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SubmitReviewHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(SubmitReviewCommand request, CancellationToken cancellationToken)
    {
        if (request.Rating < 1 || request.Rating > 5)
            return Result.Failure(new Error("Error.Validation", "Review_RatingRange"));

        var productExists = await _context.Products.AnyAsync(p => p.Id == request.ProductId, cancellationToken);
        if (!productExists) return Result.Failure(new Error("Error.NotFound", "Product_NotFound"));

        var existing = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ProductId == request.ProductId && r.UserId == request.UserId, cancellationToken);

        if (existing != null)
        {
            existing.Update(loxxking_backend_clean.Domain.ValueObjects.RatingScore.FromInt(request.Rating), request.Comment);
            _context.Reviews.Update(existing);
        }
        else
        {
            var review = Review.Create(
                request.ProductId,
                request.UserId,
                null,
                loxxking_backend_clean.Domain.ValueObjects.RatingScore.FromInt(request.Rating),
                request.Comment
            );
            _context.Reviews.Add(review);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
