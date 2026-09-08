namespace loxxking_backend_clean.Application.Features.Products.Commands.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage(localizer["Product.Validation.NameRequired"])
            .MaximumLength(200).WithMessage(localizer["Product.Validation.NameMaxLength"]);

        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage(localizer["Product.Validation.NameRequired"])
            .MaximumLength(200).WithMessage(localizer["Product.Validation.NameMaxLength"]);

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage(localizer["Validation_FieldRequired"]);

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage(localizer["Validation_CannotBeNegative", "BasePrice"]);
    }
}
