namespace loxxking_backend_clean.Application.Features.BankTransfers.Queries.GetTransferByOrderId;

public record GetTransferByOrderIdQuery(Guid OrderId, Guid UserId, bool IsStaff) : IRequest<Result<GetTransferByOrderIdResponse>>;

public record GetTransferByOrderIdResponse(
    Guid Id,
    Guid OrderId,
    string ProofImageUrl,
    string Status,
    DateTime SubmittedAt,
    DateTime? ReviewedAt,
    string? RejectionReason,
    string CustomerName
);
