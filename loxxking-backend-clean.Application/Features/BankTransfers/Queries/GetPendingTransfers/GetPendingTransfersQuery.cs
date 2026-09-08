namespace loxxking_backend_clean.Application.Features.BankTransfers.Queries.GetPendingTransfers;

public record GetPendingTransfersQuery() : IRequest<Result<List<GetPendingTransfersResponse>>>;

public record GetPendingTransfersResponse(
    Guid Id,
    Guid OrderId,
    string CustomerName,
    string ProofImageUrl,
    DateTime SubmittedAt
);
