using loxxking_backend_clean.Domain.Entities.Support;

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

        var conversationId = request.ConversationId == Guid.Empty ? Guid.NewGuid() : request.ConversationId;

        var conversation = await _context.SupportConversations.Include(c => c.Messages).FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
        if (conversation == null)
        {
            conversation = SupportConversation.Create(
                string.Empty, // orderNumber
                request.GuestName ?? "Unknown",
                string.Empty, // customerPhone
                null // customerEmail
            );
            
            typeof(loxxking_backend_clean.Domain.Common.BaseEntity).GetProperty("Id")?.SetValue(conversation, conversationId);
            
            _context.SupportConversations.Add(conversation);
        }

        string senderType = request.IsStaff ? "Staff" : (request.UserId != Guid.Empty ? "Customer" : "Guest");
        string senderName = request.GuestName ?? "Unknown";

        if (request.UserId != Guid.Empty)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            if (user != null) senderName = user.Name;
        }

        conversation.AddMessage(
            request.UserId != Guid.Empty ? request.UserId : null,
            null,
            request.Message,
            null,
            null,
            null,
            request.UserId == Guid.Empty ? request.GuestName : null
        );

        var message = conversation.Messages.Last();

        await _context.SaveChangesAsync(cancellationToken);
        
        await _notificationService.NotifyMessageReceivedAsync(
            conversationId.ToString(),
            request.UserId,
            senderName,
            request.Message,
            message.CreatedAt
        );

        return Result.Success(new SendMessageResponse(
            message.Id,
            conversation.Id,
            senderType,
            senderName,
            message.Message,
            message.CreatedAt
        ));
    }
}
