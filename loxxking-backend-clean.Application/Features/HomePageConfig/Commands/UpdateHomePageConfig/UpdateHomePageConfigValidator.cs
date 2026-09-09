using FluentValidation;

namespace loxxking_backend_clean.Application.Features.HomePageConfig.Commands.UpdateHomePageConfig;

public class UpdateHomePageConfigValidator : AbstractValidator<UpdateHomePageConfigCommand>
{
    public UpdateHomePageConfigValidator()
    {
        RuleFor(x => x.SectionsJson)
            .NotEmpty().WithMessage("SectionsJson cannot be empty.")
            .Must(IsValidJson).WithMessage("SectionsJson must be a valid JSON array string.");
    }

    private static bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array;
        }
        catch
        {
            return false;
        }
    }
}
