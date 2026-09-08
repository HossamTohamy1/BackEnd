using loxxking_backend_clean.Domain.Entities.Notifications;
using loxxking_backend_clean.Application.Features.Reviews.Helpers;


namespace loxxking_backend_clean.Application.Features.Reviews.Commands.ApproveReview;

public record ApproveReviewCommand(Guid Id) : IRequest<Result>;

public class ApproveReviewHandler : IRequestHandler<ApproveReviewCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource>? _localizer;

    public ApproveReviewHandler(IApplicationDbContext context, IStringLocalizer<SharedResource>? localizer = null)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (review is null)
            return Result.Failure(new Error("Error.NotFound", "Review_NotFound"));

        review.Approve();
        _context.Reviews.Update(review);

        if (review.UserId.HasValue)
        {
            var notifMsg = _localizer.Get("Notification_ReviewApproved", "Your review has been approved and is now visible.");
            _context.Notifications.Add(Notification.Create(
                review.UserId.Value,
                NotificationType.ReviewResponse,
                notifMsg,
                review.Id
            ));
        }

        await ReviewSyncHelper.SyncProductRatingAsync(review.ProductId, _context, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
