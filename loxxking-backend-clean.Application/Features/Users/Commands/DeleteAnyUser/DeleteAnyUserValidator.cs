namespace loxxking_backend_clean.Application.Features.Users.Commands.DeleteAnyUser;

public class DeleteAnyUserValidator : AbstractValidator<DeleteAnyUserCommand>
{
    public DeleteAnyUserValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.TargetId).NotEmpty().WithMessage(localizer["Validation_FieldRequired"]);
    }
}
