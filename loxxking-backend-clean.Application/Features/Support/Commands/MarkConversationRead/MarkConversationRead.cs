namespace loxxking_backend_clean.Application.Features.Support.Commands.MarkConversationRead;

public record MarkConversationReadCommand(
    Guid ConversationId,
    Guid? UserId = null,
    string? GuestId = null,
    bool IsStaff = false
) : IRequest<Result>;

public class MarkConversationReadHandler : IRequestHandler<MarkConversationReadCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public MarkConversationReadHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result> Handle(MarkConversationReadCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _context.SupportConversations.FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);
        if (conversation == null)
            return Result.Failure(new Error("Error.NotFound", "Conversation not found"));

        if (!request.IsStaff)
        {
            if (request.UserId.HasValue && request.UserId.Value != Guid.Empty)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId.Value, cancellationToken);
                if (user == null || (conversation.CustomerEmail != user.Email && conversation.CustomerName != user.Name))
                    return Result.Failure(new Error("Error.Unauthorized", "Unauthorized"));
            }
            else if (!string.IsNullOrWhiteSpace(request.GuestId) && Guid.TryParse(request.GuestId, out _))
            {
                if (conversation.OrderNumber != $"guest:{request.GuestId}" && conversation.CustomerEmail != $"guest_{request.GuestId}@guest.local")
                    return Result.Failure(new Error("Error.Unauthorized", "Unauthorized"));
            }
            else
            {
                return Result.Failure(new Error("Error.Unauthorized", "Unauthorized"));
            }
        }

        var unread = await _context.SupportMessages
            .Where(m => m.ConversationId == request.ConversationId && !m.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var msg in unread)
        {
            msg.IsRead = true;
        }

        _context.SupportMessages.UpdateRange(unread);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

