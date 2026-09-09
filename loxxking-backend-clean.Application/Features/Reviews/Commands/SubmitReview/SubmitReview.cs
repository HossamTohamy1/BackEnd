using loxxking_backend_clean.Domain.Entities.Reviews;
using loxxking_backend_clean.Domain.Entities.Support;
using loxxking_backend_clean.Domain.Enums;
using loxxking_backend_clean.Application.Common.Interfaces;

namespace loxxking_backend_clean.Application.Features.Reviews.Commands.SubmitReview;

public record SubmitReviewCommand(Guid ProductId, Guid UserId, int Rating, string Comment) : IRequest<Result>;

public class SubmitReviewHandler : IRequestHandler<SubmitReviewCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ISupportNotificationService _notificationService;

    public SubmitReviewHandler(IApplicationDbContext context, ISupportNotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(SubmitReviewCommand request, CancellationToken cancellationToken)
    {
        if (request.Rating < 1 || request.Rating > 5)
            return Result.Failure(new Error("Error.Validation", "Review_RatingRange"));

        if (request.UserId == Guid.Empty)
            return Result.Failure(new Error("Error.Unauthorized", "Review_AuthRequired"));

        var productExists = await _context.Products.AnyAsync(p => p.Id == request.ProductId, cancellationToken);
        if (!productExists) return Result.Failure(new Error("Error.NotFound", "Product_NotFound"));

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return Result.Failure(new Error("Error.NotFound", "User_NotFound"));

        var existing = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ProductId == request.ProductId && r.UserId == request.UserId, cancellationToken);

        bool isNewReview = false;
        Review review;

        if (existing != null)
        {
            existing.Update(loxxking_backend_clean.Domain.ValueObjects.RatingScore.FromInt(request.Rating), request.Comment);
            _context.Reviews.Update(existing);
            review = existing;
        }
        else
        {
            isNewReview = true;
            review = Review.Create(
                request.ProductId,
                request.UserId,
                null,
                loxxking_backend_clean.Domain.ValueObjects.RatingScore.FromInt(request.Rating),
                request.Comment
            );
            _context.Reviews.Add(review);
        }

        Guid? conversationIdToNotify = null;
        Guid? adminId = null;

        if (isNewReview)
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

            adminId = await _context.Users.Where(u => u.Role == UserRole.Admin).Select(u => u.Id).FirstOrDefaultAsync(cancellationToken);

            var supportMessage = new SupportMessage {
                SenderId = adminId,
                RecipientId = request.UserId,
                Message = "أهلاً بك في متجر LOXXKING، معك أستاذة فاطمة من الدعم، هنا لمساعدتك.",
                RelatedReviewId = review.Id,
                GuestName = "Support",
                ConversationId = conversation.Id
            };
            _context.SupportMessages.Add(supportMessage);

            conversationIdToNotify = conversation.Id;
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (isNewReview && conversationIdToNotify.HasValue)
        {
            try
            {
                await _notificationService.NotifyMessageReceivedAsync(
                    conversationIdToNotify.Value.ToString(),
                    adminId,
                    "Support",
                    "أهلاً بك في متجر LOXXKING، معك أستاذة فاطمة من الدعم، هنا لمساعدتك.",
                    DateTime.UtcNow
                );
            }
            catch
            {
                // Fire and forget; do not fail the review submission if SignalR broadcast fails.
            }
        }

        return Result.Success();
    }
}
