namespace loxxking_backend_clean.Application.Features.Products.Commands.UploadProductImage;

public class UploadProductImageValidator : AbstractValidator<UploadProductImageCommand>
{
    public UploadProductImageValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage(localizer["Validation_FieldRequired"]);

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage(localizer["Product_ImageRequired"]);


    }
}
