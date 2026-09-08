using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using loxxking_backend_clean.Shared;
using loxxking_backend_clean.Shared.Resources;

namespace loxxking_backend_clean.Api.Common;

public static class ResultExtensions
{
    private static IHttpContextAccessor? _httpContextAccessor;

    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private static string LocalizeErrorMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return message;

        var localizer = _httpContextAccessor?.HttpContext?.RequestServices.GetService<IStringLocalizer<SharedResource>>();
        if (localizer != null)
        {
            return localizer.Get(message, message);
        }
        return message;
    }

    public static IActionResult ToApiResponse<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(ApiResponse<T>.Ok(result.Value));
        }

        var localizedMessage = LocalizeErrorMessage(result.Error.Message);

        return result.Error.Code switch
        {
            "Error.NotFound" or "NotFound" => new NotFoundObjectResult(ApiResponse<T>.Fail(localizedMessage)),
            "Error.Unauthorized" or "Unauthorized" => new UnauthorizedObjectResult(ApiResponse<T>.Fail(localizedMessage)),
            _ => new BadRequestObjectResult(ApiResponse<T>.Fail(localizedMessage))
        };
    }

    public static IActionResult ToApiResponse(this Result result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(ApiResponse<object>.Ok(null!));
        }

        var localizedMessage = LocalizeErrorMessage(result.Error.Message);

        return result.Error.Code switch
        {
            "Error.NotFound" or "NotFound" => new NotFoundObjectResult(ApiResponse<object>.Fail(localizedMessage)),
            "Error.Unauthorized" or "Unauthorized" => new UnauthorizedObjectResult(ApiResponse<object>.Fail(localizedMessage)),
            _ => new BadRequestObjectResult(ApiResponse<object>.Fail(localizedMessage))
        };
    }
}
