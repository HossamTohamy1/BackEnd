namespace loxxking_backend_clean.Application.Features.Users.Commands.CreateStoreManager;

public class CreateStoreManagerValidator : AbstractValidator<CreateStoreManagerCommand>
{
    public CreateStoreManagerValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(localizer["Validation_FieldRequired"]);
        RuleFor(x => x.Email).NotEmpty().WithMessage(localizer["Validation_FieldRequired"]).EmailAddress().WithMessage(localizer["Validation_InvalidEmail"]);
        RuleFor(x => x.Phone).NotEmpty().WithMessage(localizer["Validation_FieldRequired"]);
        RuleFor(x => x.Password).NotEmpty().WithMessage(localizer["Validation_FieldRequired"]).MinimumLength(6).WithMessage(localizer["Validation_StringMinLength", "Password", 6]);
    }
}
