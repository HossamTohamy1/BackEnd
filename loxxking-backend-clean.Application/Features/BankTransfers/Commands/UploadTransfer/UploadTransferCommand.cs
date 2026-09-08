namespace loxxking_backend_clean.Application.Features.BankTransfers.Commands.UploadTransfer;

public record UploadTransferCommand(
    Guid OrderId,
    Stream FileStream,
    string FileName,
    string ContentType,
    Guid UserId
) : IRequest<Result<UploadTransferResponse>>;

public record UploadTransferResponse(Guid Id, string Status);
