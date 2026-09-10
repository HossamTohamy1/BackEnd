using loxxking_backend_clean.Application.Common.Interfaces;
using loxxking_backend_clean.Domain.Entities.Notifications;
using loxxking_backend_clean.Domain.Entities.Reviews;
using loxxking_backend_clean.Domain.Entities.Support;
using loxxking_backend_clean.Domain.Enums;

namespace loxxking_backend_clean.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, Result<CreateReviewResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource>? _localizer;
    private readonly ISupportNotificationService _notificationService;

    public CreateReviewHandler(
        IApplicationDbContext context,
        ISupportNotificationService notificationService,
        IStringLocalizer<SharedResource>? localizer = null)
    {
        _context = context;
        _notificationService = notificationService;
        _localizer = localizer;
    }

    public async Task<Result<CreateReviewResponse>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId != Guid.Empty)
        {
            var existing = await _context.Reviews
                .AnyAsync(r => r.ProductId == request.ProductId && r.UserId == request.UserId, cancellationToken);

            if (existing)
                return Result.Failure<CreateReviewResponse>(new Error("Error.Validation", "Review_AlreadyReviewed"));
        }
        string? guestName = null;
        if (request.UserId == Guid.Empty)
        {
            guestName = string.IsNullOrWhiteSpace(request.GuestName)
                ? $"user_{Guid.NewGuid().ToString("N")[..6]}"
                : request.GuestName.Trim();
        }

        var review = Review.Create(
            request.ProductId,
            request.UserId != Guid.Empty ? request.UserId : null,
            guestName,
            loxxking_backend_clean.Domain.ValueObjects.RatingScore.FromInt(request.Rating),
            request.Comment
        );

        review.Approve();
        _context.Reviews.Add(review);
        
        var admins = await _context.Users
            .Where(u => u.Role == UserRole.Admin)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var notifMsg = _localizer?.Get("Notification_ReviewNeedsModeration", "A new review needs moderation.") ?? "A new review needs moderation.";
        foreach (var adminId in admins)
        {
            _context.Notifications.Add(Notification.Create(
                adminId,
                NotificationType.NewReview,
                notifMsg,
                review.Id
            ));
        }

        Guid? conversationIdToNotify = null;
        Guid? staffSenderId = admins.FirstOrDefault();
        if (staffSenderId == Guid.Empty) staffSenderId = null;

        var effectiveGuestId = !string.IsNullOrWhiteSpace(request.GuestId) && Guid.TryParse(request.GuestId, out _)
            ? request.GuestId
            : Guid.NewGuid().ToString();

        if (request.UserId == Guid.Empty)
        {
            var conversation = await _context.SupportConversations.Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.OrderNumber == $"guest:{effectiveGuestId}" || c.CustomerEmail == $"guest_{effectiveGuestId}@guest.local", cancellationToken);

            if (conversation == null)
            {
                conversation = SupportConversation.Create(
                    $"guest:{effectiveGuestId}",
                    request.GuestName ?? "Guest",
                    string.Empty,
                    $"guest_{effectiveGuestId}@guest.local"
                );
                _context.SupportConversations.Add(conversation);
            }

            var supportMessage = new SupportMessage {
                SenderId = staffSenderId,
                RecipientId = null,
                Message = "أهلاً بك في متجر LOXXKING، معك أستاذ سعيد من الدعم للمساعدة.",
                RelatedReviewId = review.Id,
                GuestName = "Support",
                ConversationId = conversation.Id,
                IsRead = false
            };
            _context.SupportMessages.Add(supportMessage);
            conversationIdToNotify = conversation.Id;
        }
        else if (request.UserId != Guid.Empty)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user != null)
            {
                var conversation = await _context.SupportConversations.Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.CustomerEmail == user.Email || c.CustomerName == user.Name, cancellationToken);

                if (conversation == null)
                {
                    conversation = SupportConversation.Create(
                        string.Empty,
                        user.Name ?? "Customer",
                        user.PhoneNumber ?? string.Empty,
                        user.Email
                    );
                    _context.SupportConversations.Add(conversation);
                }

                var supportMessage = new SupportMessage {
                    SenderId = staffSenderId,
                    RecipientId = request.UserId,
                    Message = "أهلاً بك في متجر LOXXKING، معك أستاذ سعيد من الدعم للمساعدة.",
                    RelatedReviewId = review.Id,
                    GuestName = "Support",
                    ConversationId = conversation.Id,
                    IsRead = false
                };
                _context.SupportMessages.Add(supportMessage);
                conversationIdToNotify = conversation.Id;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (conversationIdToNotify.HasValue)
        {
            try
            {
                await _notificationService.NotifyMessageReceivedAsync(
                    conversationIdToNotify.Value.ToString(),
                    staffSenderId,
                    "Support",
                    "أهلاً بك في متجر LOXXKING، معك أستاذة فاطمة من الدعم، هنا لمساعدتك.",
                    DateTime.UtcNow
                );
            }
            catch
            {
                // Fire and forget; do not fail review creation if notification fails
            }
        }

        return Result.Success(new CreateReviewResponse(review.Id));
    }
}
