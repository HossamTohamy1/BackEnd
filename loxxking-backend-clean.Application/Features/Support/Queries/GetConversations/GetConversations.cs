namespace loxxking_backend_clean.Application.Features.Support.Queries.GetConversations;

public record GetConversationsQuery() : IRequest<Result<List<ConversationResponse>>>;
public record ConversationResponse(Guid Id, string CustomerName, DateTime UpdatedAt, int UnreadCount);

public class GetConversationsHandler : IRequestHandler<GetConversationsQuery, Result<List<ConversationResponse>>>
{
    private readonly IApplicationDbContext _context;
    public GetConversationsHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result<List<ConversationResponse>>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var convs = await _context.SupportConversations
            .Include(c => c.Messages)
            .Select(c => new
            {
                c.Id,
                c.UpdatedAt,
                UnreadCount = c.Messages.Count(m => !m.IsRead && (m.SenderId == null || _context.Users.Any(u => u.Id == m.SenderId && (u.Role == loxxking_backend_clean.Domain.Enums.UserRole.Customer)))),
                CustomerName = c.Messages.OrderByDescending(m => m.CreatedAt).Select(m => m.SenderId != null ? _context.Users.Where(u => u.Id == m.SenderId).Select(u => u.Name).FirstOrDefault() : m.GuestName).FirstOrDefault(n => n != null) ?? "Customer"
            })
            .ToListAsync(cancellationToken);

        var responses = convs.Select(c => new ConversationResponse(c.Id, c.CustomerName, c.UpdatedAt ?? DateTime.UtcNow, c.UnreadCount)).ToList();
        return Result.Success(responses);
    }
}
