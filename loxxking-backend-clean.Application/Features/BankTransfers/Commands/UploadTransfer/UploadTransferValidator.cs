namespace loxxking_backend_clean.Application.Features.BankTransfers.Commands.UploadTransfer;

public class UploadTransferValidator : AbstractValidator<UploadTransferCommand>
{
    public UploadTransferValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage(localizer["Validation_FieldRequired"]);

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage(localizer["BankTransfer_ProofRequired"]);

        RuleFor(x => x.FileStream.Length)
            .GreaterThan(0).WithMessage(localizer["BankTransfer_ProofRequired"]);
    }
}
