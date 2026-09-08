namespace loxxking_backend_clean.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage(localizer["Review_RatingRange"]);
        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage(localizer["Review_CommentRequired"]);
    }
}
