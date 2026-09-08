using loxxking_backend_clean.Application.Features.Reviews.Helpers;

namespace loxxking_backend_clean.Application.Features.Reviews.Commands.DeleteReview;

public record DeleteReviewCommand(Guid Id) : IRequest<Result>;

public class DeleteReviewHandler : IRequestHandler<DeleteReviewCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeleteReviewHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var r = await _context.Reviews.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (r == null) return Result.Failure(new Error("Error.NotFound", "Review_NotFound"));
        _context.Reviews.Remove(r);
        await ReviewSyncHelper.SyncProductRatingAsync(r.ProductId, _context, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
