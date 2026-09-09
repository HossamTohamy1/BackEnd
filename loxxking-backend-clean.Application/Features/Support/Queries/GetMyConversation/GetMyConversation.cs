using loxxking_backend_clean.Application.Features.Support.Queries.GetMessages;
using loxxking_backend_clean.Domain.Entities.Support;

namespace loxxking_backend_clean.Application.Features.Support.Queries.GetMyConversation;

public record GetMyConversationQuery(Guid UserId) : IRequest<Result<ConversationWithMessagesResponse?>>;

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
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return Result.Failure<ConversationWithMessagesResponse?>(new Error("Error.NotFound", "User not found"));

        var conversation = await _context.SupportConversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.CustomerEmail == user.Email || c.CustomerName == user.Name, cancellationToken);

        if (conversation == null)
            return Result.Success<ConversationWithMessagesResponse?>(null);

        var messages = conversation.Messages.OrderBy(m => m.CreatedAt).Select(m => new GetMessagesResponse(
            m.Id,
            conversation.Id,
            m.Message,
            m.CreatedAt,
            m.IsRead,
            m.SenderId == null || _context.Users.Any(u => u.Id == m.SenderId && u.Role != loxxking_backend_clean.Domain.Enums.UserRole.Customer) ? "Staff" : "Customer",
            m.SenderId == null || _context.Users.Any(u => u.Id == m.SenderId && u.Role != loxxking_backend_clean.Domain.Enums.UserRole.Customer) ? "Support" : "Customer"
        )).ToList();

        return Result.Success<ConversationWithMessagesResponse?>(new ConversationWithMessagesResponse(conversation.Id, messages));
    }
}
