namespace loxxking_backend_clean.Application.Features.Support.Queries.GetMessages;

public class GetMessagesHandler : IRequestHandler<GetMessagesQuery, Result<List<GetMessagesResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetMessagesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<GetMessagesResponse>>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var messages = await _context.SupportMessages
            .Where(sm => sm.ConversationId == request.ConversationId)
            .OrderBy(sm => sm.CreatedAt)
            .Select(sm => new GetMessagesResponse(
                sm.Id,
                sm.ConversationId,
                sm.Message,
                sm.CreatedAt,
                sm.IsRead,
                sm.SenderId == null || _context.Users.Any(u => u.Id == sm.SenderId && u.Role != loxxking_backend_clean.Domain.Enums.UserRole.Customer) ? "Staff" : (sm.GuestName != null ? "Guest" : "Customer"),
                sm.SenderId == null || _context.Users.Any(u => u.Id == sm.SenderId && u.Role != loxxking_backend_clean.Domain.Enums.UserRole.Customer) ? "Support" : (sm.Sender != null ? sm.Sender.Name : (sm.GuestName ?? "Guest"))
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(messages);
    }
}
