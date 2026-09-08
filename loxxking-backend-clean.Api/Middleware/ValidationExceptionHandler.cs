using FluentValidation;
using loxxking_backend_clean.Domain.Common;
using loxxking_backend_clean.Shared;
using loxxking_backend_clean.Shared.Resources;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Localization;

namespace loxxking_backend_clean.Api.Middleware;

public class ValidationExceptionHandler : IExceptionHandler
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ValidationExceptionHandler(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var validationHeader = _localizer.Get("Validation_Failed", "Validation failed");

        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = ApiResponse<object>.Fail(validationHeader, errors);
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }

        if (exception is DomainValidationException domainValidationException)
        {
            var localizedMessage = _localizer.Get(domainValidationException.ResourceKey, domainValidationException.Message, domainValidationException.Args);
            var response = ApiResponse<object>.Fail(validationHeader, new List<string> { localizedMessage });
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }

        if (exception is ArgumentException argumentException)
        {
            var key = argumentException.Message;
            var paramIdx = key.IndexOf("(Parameter");
            if (paramIdx >= 0)
            {
                key = key.Substring(0, paramIdx).Trim();
            }
            var localizedMessage = _localizer.Get(key, key);
            var response = ApiResponse<object>.Fail(validationHeader, new List<string> { localizedMessage });
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }

        if (exception is InvalidOperationException invalidOperationException)
        {
            var localizedMessage = _localizer.Get(invalidOperationException.Message, invalidOperationException.Message);
            var response = ApiResponse<object>.Fail(validationHeader, new List<string> { localizedMessage });
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }

        return false;
    }
}
