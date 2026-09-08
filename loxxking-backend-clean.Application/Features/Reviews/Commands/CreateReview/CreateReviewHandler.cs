using loxxking_backend_clean.Domain.Entities.Notifications;
using loxxking_backend_clean.Domain.Entities.Reviews;


namespace loxxking_backend_clean.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, Result<CreateReviewResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource>? _localizer;

    public CreateReviewHandler(IApplicationDbContext context, IStringLocalizer<SharedResource>? localizer = null)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<CreateReviewResponse>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId != Guid.Empty)
        {
            var hasPurchased = await _context.OrderItems
                .AnyAsync(oi => oi.ProductId == request.ProductId && oi.Order.UserId == request.UserId && oi.Order.Status == OrderStatus.Delivered, cancellationToken);

            if (!hasPurchased)
                return Result.Failure<CreateReviewResponse>(new Error("Error.Validation", "Review_OnlyPurchasedProducts"));

            var existing = await _context.Reviews
                .AnyAsync(r => r.ProductId == request.ProductId && r.UserId == request.UserId, cancellationToken);

            if (existing)
                return Result.Failure<CreateReviewResponse>(new Error("Error.Validation", "Review_AlreadyReviewed"));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.GuestName))
                return Result.Failure<CreateReviewResponse>(new Error("Error.Validation", "Review_GuestNameRequired"));
        }

        var review = Review.Create(
            request.ProductId,
            request.UserId != Guid.Empty ? request.UserId : null,
            request.UserId == Guid.Empty ? request.GuestName : null,
            loxxking_backend_clean.Domain.ValueObjects.RatingScore.FromInt(request.Rating),
            request.Comment
        );

        _context.Reviews.Add(review);
        
        var admins = await _context.Users
            .Where(u => u.Role == UserRole.Admin)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var notifMsg = _localizer.Get("Notification_ReviewNeedsModeration", "A new review needs moderation.");
        foreach (var adminId in admins)
        {
            _context.Notifications.Add(Notification.Create(
                adminId,
                NotificationType.NewReview,
                notifMsg,
                review.Id
            ));
        }
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateReviewResponse(review.Id));
    }
}
