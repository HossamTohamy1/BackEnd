using loxxking_backend_clean.Application.Features.Reviews.Helpers;

namespace loxxking_backend_clean.Application.Features.Reviews.Commands.RejectReview;

public record RejectReviewCommand(Guid Id) : IRequest<Result>;

public class RejectReviewHandler : IRequestHandler<RejectReviewCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public RejectReviewHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result> Handle(RejectReviewCommand request, CancellationToken cancellationToken)
    {
        var r = await _context.Reviews.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (r == null) return Result.Failure(new Error("Error.NotFound", "Review_NotFound"));
        r.Reject();
        _context.Reviews.Update(r);
        await ReviewSyncHelper.SyncProductRatingAsync(r.ProductId, _context, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
