namespace loxxking_backend_clean.Application.Features.Reviews.Queries.GetReviewsByProduct;

public class GetReviewsByProductHandler : IRequestHandler<GetReviewsByProductQuery, Result<List<GetReviewsByProductResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetReviewsByProductHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<GetReviewsByProductResponse>>> Handle(GetReviewsByProductQuery request, CancellationToken cancellationToken)
    {
        Guid? guestConversationId = null;
        if (!string.IsNullOrWhiteSpace(request.GuestId) && Guid.TryParse(request.GuestId, out _))
        {
            guestConversationId = await _context.SupportConversations
                .Where(c => c.OrderNumber == $"guest:{request.GuestId}" || c.CustomerEmail == $"guest_{request.GuestId}@guest.local")
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var reviews = await _context.Reviews
            .Where(r => r.ProductId == request.ProductId && 
                        (r.Status == ReviewStatus.Approved || 
                        (request.CurrentUserId.HasValue && request.CurrentUserId.Value != Guid.Empty && r.UserId == request.CurrentUserId.Value && r.Status == ReviewStatus.Pending) ||
                        (guestConversationId.HasValue && r.UserId == null && r.Status == ReviewStatus.Pending && _context.SupportMessages.Any(m => m.ConversationId == guestConversationId.Value && m.RelatedReviewId == r.Id))))
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new GetReviewsByProductResponse(
                r.Id,
                r.Rating.Value,
                r.Comment,
                r.CreatedAt,
                r.GuestName,
                r.User != null ? new ReviewUserDto(r.User.Id, r.User.Name) : null,
                _context.SupportMessages.Any(m => m.RelatedReviewId == r.Id),
                r.Status == ReviewStatus.Pending
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(reviews);
    }
}
