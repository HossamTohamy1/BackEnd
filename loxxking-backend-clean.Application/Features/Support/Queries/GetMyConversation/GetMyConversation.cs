using loxxking_backend_clean.Application.Features.Support.Queries.GetMessages;
using loxxking_backend_clean.Domain.Entities.Support;

namespace loxxking_backend_clean.Application.Features.Support.Queries.GetMyConversation;

public record GetMyConversationQuery(Guid? UserId, string? GuestId = null) : IRequest<Result<ConversationWithMessagesResponse?>>;

public record ConversationWithMessagesResponse(Guid Id, List<GetMessagesResponse> Messages);

public class GetMyConversationHandler : IRequestHandler<GetMyConversationQuery, Result<ConversationWithMessagesResponse?>>
{
    private readonly IApplicationDbContext _context;

    public GetMyConversationHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ConversationWithMessagesResponse?>> Handle(GetMyConversationQuery request, CancellationToken cancellationToken)
    {
        SupportConversation? conversation = null;

        if (request.UserId.HasValue && request.UserId.Value != Guid.Empty)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId.Value, cancellationToken);
            if (user == null) return Result.Failure<ConversationWithMessagesResponse?>(new Error("Error.NotFound", "User not found"));

            conversation = await _context.SupportConversations
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.CustomerEmail == user.Email || c.CustomerName == user.Name, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.GuestId) && Guid.TryParse(request.GuestId, out _))
        {
            conversation = await _context.SupportConversations
                .Include(c => c.Messages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.OrderNumber == $"guest:{request.GuestId}" || c.CustomerEmail == $"guest_{request.GuestId}@guest.local", cancellationToken);
        }

        if (conversation == null)
            return Result.Success<ConversationWithMessagesResponse?>(null);

        var messages = conversation.Messages.OrderBy(m => m.CreatedAt).Select(m => new GetMessagesResponse(
            m.Id,
            conversation.Id,
            m.Message,
            m.CreatedAt,
            m.IsRead,
            m.IsStaff ? "Staff" : (m.SenderId != null ? "Customer" : "Guest"),
            m.IsStaff ? (m.GuestName ?? "Support") : (m.Sender != null ? m.Sender.Name : (m.GuestName ?? "Customer")),
            m.AttachmentUrl
        )).ToList();

        return Result.Success<ConversationWithMessagesResponse?>(new ConversationWithMessagesResponse(conversation.Id, messages));
    }
}
