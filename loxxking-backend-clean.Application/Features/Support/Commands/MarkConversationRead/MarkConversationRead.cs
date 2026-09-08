namespace loxxking_backend_clean.Application.Features.Support.Commands.MarkConversationRead;

public record MarkConversationReadCommand(Guid ConversationId) : IRequest<Result>;

public class MarkConversationReadHandler : IRequestHandler<MarkConversationReadCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public MarkConversationReadHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result> Handle(MarkConversationReadCommand request, CancellationToken cancellationToken)
    {
        var unread = await _context.SupportMessages.Where(m => m.ConversationId == request.ConversationId && !m.IsRead).ToListAsync(cancellationToken);
        foreach(var msg in unread) { msg.IsRead = true; }
        _context.SupportMessages.UpdateRange(unread);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
