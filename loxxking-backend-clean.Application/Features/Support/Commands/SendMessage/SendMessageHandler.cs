using loxxking_backend_clean.Domain.Entities.Support;
using loxxking_backend_clean.Domain.Entities.Users;

namespace loxxking_backend_clean.Application.Features.Support.Commands.SendMessage;

public class SendMessageHandler : IRequestHandler<SendMessageCommand, Result<SendMessageResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISupportNotificationService _notificationService;

    public SendMessageHandler(IApplicationDbContext context, ISupportNotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<Result<SendMessageResponse>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return Result.Failure<SendMessageResponse>(new Error("Error.Validation", "Support_MessageEmpty"));

        User? user = null;
        if (request.UserId != Guid.Empty)
        {
            user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        }

        SupportConversation? conversation = null;

        if (request.ConversationId != Guid.Empty)
        {
            conversation = await _context.SupportConversations.Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

            if (conversation == null)
                return Result.Failure<SendMessageResponse>(new Error("Error.NotFound", "Conversation not found"));

            // Ownership validation
            if (!request.IsStaff)
            {
                if (user != null)
                {
                    if (conversation.CustomerEmail != user.Email && conversation.CustomerName != user.Name)
                        return Result.Failure<SendMessageResponse>(new Error("Error.Unauthorized", "Unauthorized"));
                }
                else if (!string.IsNullOrWhiteSpace(request.GuestId) && Guid.TryParse(request.GuestId, out _))
                {
                    if (conversation.OrderNumber != $"guest:{request.GuestId}" && conversation.CustomerEmail != $"guest_{request.GuestId}@guest.local")
                        return Result.Failure<SendMessageResponse>(new Error("Error.Unauthorized", "Unauthorized"));
                }
                else
                {
                    return Result.Failure<SendMessageResponse>(new Error("Error.Unauthorized", "Unauthorized"));
                }
            }
        }
        else
        {
            // ConversationId is empty: find or create for user or guest
            if (user != null)
            {
                conversation = await _context.SupportConversations.Include(c => c.Messages)
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
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            else if (!string.IsNullOrWhiteSpace(request.GuestId) && Guid.TryParse(request.GuestId, out _))
            {
                conversation = await _context.SupportConversations.Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.OrderNumber == $"guest:{request.GuestId}" || c.CustomerEmail == $"guest_{request.GuestId}@guest.local", cancellationToken);

                if (conversation == null)
                {
                    conversation = SupportConversation.Create(
                        $"guest:{request.GuestId}",
                        request.GuestName ?? "Guest",
                        string.Empty,
                        $"guest_{request.GuestId}@guest.local"
                    );
                    _context.SupportConversations.Add(conversation);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
            else
            {
                return Result.Failure<SendMessageResponse>(new Error("Error.Unauthorized", "Unauthorized"));
            }
        }

        bool isStaff = request.IsStaff || request.IsFromCrm;
        string senderType = isStaff ? "Staff" : (request.UserId != Guid.Empty ? "Customer" : "Guest");
        string senderName = isStaff ? (request.GuestName ?? "Support") : (user != null ? (user.Name ?? "Customer") : (request.GuestName ?? "Guest"));

        if (!string.IsNullOrWhiteSpace(request.ClientMessageId))
        {
            var existing = await _context.SupportMessages.FirstOrDefaultAsync(m => m.ClientMessageId == request.ClientMessageId, cancellationToken);
            if (existing != null)
            {
                return Result.Success(new SendMessageResponse(
                    existing.Id,
                    existing.ConversationId,
                    senderType,
                    senderName,
                    existing.Message,
                    existing.CreatedAt,
                    existing.ClientMessageId,
                    existing.AttachmentUrl
                ));
            }
        }

        var message = new SupportMessage {
            SenderId = request.UserId != Guid.Empty ? request.UserId : null,
            RecipientId = null,
            Message = request.Message,
            GuestName = request.UserId == Guid.Empty ? (request.GuestName ?? (isStaff ? "Support" : "Guest")) : null,
            ConversationId = conversation.Id,
            IsRead = false,
            ClientMessageId = request.ClientMessageId,
            AttachmentUrl = request.AttachmentUrl,
            IsSyncedToCrm = request.IsFromCrm,
            IsStaff = isStaff
        };
        _context.SupportMessages.Add(message);

        await _context.SaveChangesAsync(cancellationToken);
        
        await _notificationService.NotifyMessageReceivedAsync(
            conversation.Id.ToString(),
            request.UserId != Guid.Empty ? request.UserId : null,
            senderName,
            request.Message,
            message.CreatedAt,
            message.AttachmentUrl,
            isStaff,
            senderType
        );

        return Result.Success(new SendMessageResponse(
            message.Id,
            conversation.Id,
            senderType,
            senderName,
            message.Message,
            message.CreatedAt,
            message.ClientMessageId,
            message.AttachmentUrl
        ));
    }
}
