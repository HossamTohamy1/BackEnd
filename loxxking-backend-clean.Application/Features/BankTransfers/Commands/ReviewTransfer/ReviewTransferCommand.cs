namespace loxxking_backend_clean.Application.Features.BankTransfers.Commands.ReviewTransfer;

public record ReviewTransferCommand(
    Guid Id,
    bool Approved,
    string? RejectionReason
) : IRequest<Result<ReviewTransferResponse>>;

public record ReviewTransferResponse(Guid Id, string Status);
