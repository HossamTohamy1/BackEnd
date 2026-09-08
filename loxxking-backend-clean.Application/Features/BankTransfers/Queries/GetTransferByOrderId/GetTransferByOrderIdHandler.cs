namespace loxxking_backend_clean.Application.Features.BankTransfers.Queries.GetTransferByOrderId;

public class GetTransferByOrderIdHandler : IRequestHandler<GetTransferByOrderIdQuery, Result<GetTransferByOrderIdResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetTransferByOrderIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetTransferByOrderIdResponse>> Handle(GetTransferByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var transfer = await _context.BankTransfers
            .Where(bt => bt.OrderId == request.OrderId)
            .Select(bt => new
            {
                bt.Id,
                bt.OrderId,
                bt.ProofImageUrl,
                Status = bt.Status.ToString(),
                bt.SubmittedAt,
                bt.ReviewedAt,
                bt.RejectionReason,
                CustomerId = bt.Order.CustomerId,
                CustomerName = bt.Order.Customer != null ? bt.Order.Customer.Name : "Guest"
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (transfer is null)
        {
            return Result.Failure<GetTransferByOrderIdResponse>(new Error("Error.NotFound", "BankTransfer_NotFound"));
        }

        if (!request.IsStaff && transfer.CustomerId != request.UserId)
        {
            return Result.Failure<GetTransferByOrderIdResponse>(new Error("Error.Unauthorized", "Auth_UnauthorizedAccess"));
        }

        return Result.Success(new GetTransferByOrderIdResponse(
            transfer.Id,
            transfer.OrderId,
            transfer.ProofImageUrl,
            transfer.Status,
            transfer.SubmittedAt,
            transfer.ReviewedAt,
            transfer.RejectionReason,
            transfer.CustomerName
        ));
    }
}
