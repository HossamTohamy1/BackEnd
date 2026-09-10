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
                sm.GuestName == "Support" 
                    || (!string.IsNullOrEmpty(sm.Message) && (sm.Message.Contains("أستاذ سعيد") || sm.Message.Contains("LOXXKING") || sm.Message.Contains("الدعم للمساعدة")))
                    || (sm.SenderId != null && _context.Users.Any(u => u.Id == sm.SenderId && u.Role != loxxking_backend_clean.Domain.Enums.UserRole.Customer)) ? "Staff" : "Customer",
                sm.GuestName == "Support" 
                    || (!string.IsNullOrEmpty(sm.Message) && (sm.Message.Contains("أستاذ سعيد") || sm.Message.Contains("LOXXKING") || sm.Message.Contains("الدعم للمساعدة")))
                    || (sm.SenderId != null && _context.Users.Any(u => u.Id == sm.SenderId && u.Role != loxxking_backend_clean.Domain.Enums.UserRole.Customer)) ? "أستاذ سعيد (الدعم الفني)" : (sm.Sender != null ? sm.Sender.Name : (!string.IsNullOrWhiteSpace(sm.GuestName) && sm.GuestName != "Support" ? sm.GuestName : "User")),
                sm.AttachmentUrl
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(messages);
    }
}
