namespace loxxking_backend_clean.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.TargetId).NotEmpty().WithMessage(localizer["Validation_FieldRequired"]);
    }
}
