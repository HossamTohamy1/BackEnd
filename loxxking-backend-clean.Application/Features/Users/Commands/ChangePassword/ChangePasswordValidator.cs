namespace loxxking_backend_clean.Application.Features.Users.Commands.ChangePassword;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage(localizer["Validation_FieldRequired"]);
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(localizer["Validation_FieldRequired"])
            .MinimumLength(6).WithMessage(localizer["Validation_StringMinLength", "NewPassword", 6]);
    }
}
