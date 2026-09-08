namespace loxxking_backend_clean.Application.Features.Users.Commands.UpdateStaff;

public class UpdateStaffValidator : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(localizer["Validation_FieldRequired"]);
        RuleFor(x => x.Name).NotEmpty().WithMessage(localizer["Validation_FieldRequired"]);
        RuleFor(x => x.Phone).NotEmpty().WithMessage(localizer["Validation_FieldRequired"]);
    }
}
