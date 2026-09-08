using Microsoft.Extensions.Localization;
using loxxking_backend_clean.Shared.Resources;

namespace loxxking_backend_clean.Shared;

public static class LocalizerExtensions
{
    public static string Get(this IStringLocalizer<SharedResource>? localizer, string key, string fallback, params object[] args)
    {
        if (localizer == null)
        {
            return args.Length > 0 ? string.Format(fallback, args) : fallback;
        }

        var localized = args.Length > 0 ? localizer[key, args] : localizer[key];
        if (localized.ResourceNotFound)
        {
            return args.Length > 0 ? string.Format(fallback, args) : fallback;
        }

        return localized.Value;
    }

    public static string Get(this IStringLocalizer<SharedResource>? localizer, string key, params object[] args)
    {
        return Get(localizer, key, key, args);
    }
}
