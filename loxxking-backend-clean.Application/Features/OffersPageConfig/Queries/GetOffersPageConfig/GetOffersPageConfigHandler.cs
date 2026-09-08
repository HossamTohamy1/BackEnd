using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace loxxking_backend_clean.Application.Features.OffersPageConfig.Queries.GetOffersPageConfig;

public class GetOffersPageConfigHandler : IRequestHandler<GetOffersPageConfigQuery, Result<OffersPageConfigResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource>? _localizer;
    private readonly IDistributedCache _cache;

    public GetOffersPageConfigHandler(IApplicationDbContext context, IDistributedCache cache, IStringLocalizer<SharedResource>? localizer = null)
    {
        _context = context;
        _cache = cache;
        _localizer = localizer;
    }

    public async Task<Result<OffersPageConfigResponse>> Handle(GetOffersPageConfigQuery request, CancellationToken cancellationToken)
    {
        var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var cacheKey = $"PageConfig_Offers_{lang}";
        
        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedConfig = JsonSerializer.Deserialize<OffersPageConfigResponse>(cachedData);
            if (cachedConfig != null) return Result.Success(cachedConfig);
        }

        var config = await _context.OffersPageConfigs.FirstOrDefaultAsync(cancellationToken);

        OffersPageConfigResponse response;

        if (config == null)
        {
            response = new OffersPageConfigResponse(
                GetText("OffersPage_DefaultHeroTitle", "Special Offers", "عروض خاصة"),
                GetText("OffersPage_DefaultHeroSubtitle", "Best prices for a limited time", "أفضل الأسعار لفترة محدودة"),
                true,
                GetText("OffersPage_DefaultCurrentOffersTitle", "Current Discounts", "التخفيضات الحالية"),
                GetText("OffersPage_DefaultBundlesTitle", "Save more with bundles", "وفر أكثر مع الباقات"),
                GetText("OffersPage_DefaultBundlesSubtitle", "Choose the bundle that best suits you at discounted prices", "اختار الباقة الأنسب لك بأسعار مخفضة")
            );
        }
        else
        {
            string heroTitle = Resolve(config.HeroTitle, "OffersPage_DefaultHeroTitle", "عروض خاصة", "Special Offers");
            string heroSubtitle = Resolve(config.HeroSubtitle, "OffersPage_DefaultHeroSubtitle", "أفضل الأسعار لفترة محدودة", "Best prices for a limited time");
            bool showHero = config.ShowHero;
            string currentOffersTitle = Resolve(config.CurrentOffersTitle, "OffersPage_DefaultCurrentOffersTitle", "التخفيضات الحالية", "Current Discounts");
            string bundlesTitle = Resolve(config.BundlesTitle, "OffersPage_DefaultBundlesTitle", "وفر أكثر مع الباقات", "Save more with bundles");
            string bundlesSubtitle = Resolve(config.BundlesSubtitle, "OffersPage_DefaultBundlesSubtitle", "اختار الباقة الأنسب لك بأسعار مخفضة", "Choose the bundle that best suits you at discounted prices");

            response = new OffersPageConfigResponse(
                heroTitle,
                heroSubtitle,
                showHero,
                currentOffersTitle,
                bundlesTitle,
                bundlesSubtitle
            );
        }

        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), options, cancellationToken);

        return Result.Success(response);
    }

    private string GetText(string key, string enDefault, string arDefault)
    {
        if (_localizer != null)
        {
            var val = _localizer[key];
            if (!val.ResourceNotFound && !string.IsNullOrWhiteSpace(val.Value))
                return val.Value;
        }
        var isArabic = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
        return isArabic ? arDefault : enDefault;
    }

    private string Resolve(string? val, string key, string legacyArabicDefault, string enDefault)
    {
        if (string.IsNullOrWhiteSpace(val) || val == legacyArabicDefault)
            return GetText(key, enDefault, legacyArabicDefault);
        return val;
    }
}

