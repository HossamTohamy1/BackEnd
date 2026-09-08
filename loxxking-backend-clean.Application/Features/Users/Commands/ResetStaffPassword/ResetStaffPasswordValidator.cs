namespace loxxking_backend_clean.Application.Features.Users.Commands.ResetStaffPassword;

public class ResetStaffPasswordValidator : AbstractValidator<ResetStaffPasswordCommand>
{
    public ResetStaffPasswordValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.StaffId).NotEmpty().WithMessage(localizer["Validation_FieldRequired"]);
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage(localizer["Validation_FieldRequired"]).MinimumLength(6).WithMessage(localizer["Validation_StringMinLength", "NewPassword", 6]);
    }
}
