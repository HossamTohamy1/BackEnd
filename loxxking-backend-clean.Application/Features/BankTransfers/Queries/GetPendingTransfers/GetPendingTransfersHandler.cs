namespace loxxking_backend_clean.Application.Features.BankTransfers.Queries.GetPendingTransfers;

public class GetPendingTransfersHandler : IRequestHandler<GetPendingTransfersQuery, Result<List<GetPendingTransfersResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingTransfersHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<GetPendingTransfersResponse>>> Handle(GetPendingTransfersQuery request, CancellationToken cancellationToken)
    {
        var pending = await _context.BankTransfers
            .Where(bt => bt.Status == BankTransferStatus.PendingReview)
            .OrderBy(bt => bt.SubmittedAt)
            .Select(bt => new GetPendingTransfersResponse(
                bt.Id,
                bt.OrderId,
                bt.Order.Customer != null ? bt.Order.Customer.Name : "Guest",
                bt.ProofImageUrl,
                bt.SubmittedAt
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(pending);
    }
}
