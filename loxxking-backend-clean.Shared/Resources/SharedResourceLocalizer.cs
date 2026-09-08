using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Microsoft.Extensions.Localization;

namespace loxxking_backend_clean.Shared.Resources;

public class SharedResourceLocalizer : IStringLocalizer<SharedResource>, IStringLocalizer
{
    private readonly ResourceManager _resourceManager;

    public SharedResourceLocalizer()
    {
        var assembly = typeof(SharedResource).Assembly;
        _resourceManager = new ResourceManager("loxxking_backend_clean.Shared.Resources.SharedResource", assembly);
    }

    public LocalizedString this[string name]
    {
        get
        {
            var culture = CultureInfo.CurrentUICulture;
            var value = _resourceManager.GetString(name, culture);
            return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var culture = CultureInfo.CurrentUICulture;
            var format = _resourceManager.GetString(name, culture);
            var value = format != null ? string.Format(CultureInfo.CurrentCulture, format, arguments) : name;
            return new LocalizedString(name, value, resourceNotFound: format == null);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var resourceSet = _resourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
        if (resourceSet != null)
        {
            foreach (DictionaryEntry entry in resourceSet)
            {
                yield return new LocalizedString(entry.Key.ToString()!, entry.Value?.ToString() ?? string.Empty);
            }
        }
    }
}

public class SharedResourceLocalizerFactory : IStringLocalizerFactory
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public SharedResourceLocalizerFactory(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public IStringLocalizer Create(Type resourceSource) => _localizer;
    public IStringLocalizer Create(string baseName, string location) => _localizer;
}
