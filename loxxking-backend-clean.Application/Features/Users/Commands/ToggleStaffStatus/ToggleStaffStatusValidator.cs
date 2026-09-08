namespace loxxking_backend_clean.Application.Features.Users.Commands.ToggleStaffStatus;

public class ToggleStaffStatusValidator : AbstractValidator<ToggleStaffStatusCommand>
{
    public ToggleStaffStatusValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(localizer["Validation_FieldRequired"]);
    }
}
