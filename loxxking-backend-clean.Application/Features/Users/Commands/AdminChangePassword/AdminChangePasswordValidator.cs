namespace loxxking_backend_clean.Application.Features.Users.Commands.AdminChangePassword;

public class AdminChangePasswordValidator : AbstractValidator<AdminChangePasswordCommand>
{
    public AdminChangePasswordValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(localizer["Validation_FieldRequired"]);
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(localizer["Validation_FieldRequired"])
            .MinimumLength(6).WithMessage(localizer["Validation_StringMinLength", "NewPassword", 6]);
    }
}
