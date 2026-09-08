namespace loxxking_backend_clean.Application.Features.Reviews.Commands.SubmitReview;

public class SubmitReviewValidator : AbstractValidator<SubmitReviewCommand>
{
    public SubmitReviewValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage(localizer["Review_RatingRange"]);
    }
}
