using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace loxxking_backend_clean.Application.Features.FavoritesPageConfig.Queries.GetFavoritesPageConfig;

public record GetFavoritesPageConfigQuery() : IRequest<Result<Domain.Entities.Favorites.FavoritesPageConfig>>;

public class GetFavoritesPageConfigHandler : IRequestHandler<GetFavoritesPageConfigQuery, Result<Domain.Entities.Favorites.FavoritesPageConfig>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource>? _localizer;
    private readonly IDistributedCache _cache;

    public GetFavoritesPageConfigHandler(IApplicationDbContext context, IDistributedCache cache, IStringLocalizer<SharedResource>? localizer = null)
    {
        _context = context;
        _cache = cache;
        _localizer = localizer;
    }

    public async Task<Result<Domain.Entities.Favorites.FavoritesPageConfig>> Handle(GetFavoritesPageConfigQuery request, CancellationToken cancellationToken)
    {
        var lang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var cacheKey = $"PageConfig_Favorites_{lang}";

        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedConfig = JsonSerializer.Deserialize<Domain.Entities.Favorites.FavoritesPageConfig>(cachedData);
            if (cachedConfig != null) return Result.Success(cachedConfig);
        }

        var config = await _context.FavoritesPageConfigs.FirstOrDefaultAsync(cancellationToken);
        
        Domain.Entities.Favorites.FavoritesPageConfig resultConfig;

        if (config == null)
        {
            resultConfig = new Domain.Entities.Favorites.FavoritesPageConfig
            {
                ShowTitle = true,
                HeaderTitle = GetText("FavoritesPage_DefaultHeaderTitle", "Favorites", "المفضلة"),
                HeaderSubtitle = GetText("FavoritesPage_DefaultHeaderSubtitle", "List of products you liked", "قائمة بالمنتجات التي تم الإعجاب بها"),
                ShowAddAllToCart = true,
                AddAllToCartText = GetText("FavoritesPage_DefaultAddAllToCartText", "Add all products to cart", "إضافة جميع المنتجات إلى السلة"),
                ShowToolbar = true,
                ShowSort = true,
                ShowCount = true,
                ShowProductColor = true,
                ShowProductSize = true,
                ShowProductPrice = true,
                ShowProductOldPrice = true,
                ShowProductStock = true,
                ShowRemoveAction = true,
                ShowMoveToCartAction = true,
                EmptyStateTitle = GetText("FavoritesPage_DefaultEmptyStateTitle", "Favorites list is empty", "قائمة المفضلة فارغة"),
                EmptyStateSubtitle = GetText("FavoritesPage_DefaultEmptyStateSubtitle", "You haven't added any products to your favorites list yet", "لم تقم بإضافة أي منتج إلى قائمة المفضلة بعد"),
                EmptyStateButtonText = GetText("FavoritesPage_DefaultEmptyStateButtonText", "Explore Shopping", "اكتشف التسوق"),
                ShowEmptyStateIllustration = true,
                ShowTrustBadges = true,
                TrustBadgesJson = "[]"
            };
        }
        else
        {
            resultConfig = new Domain.Entities.Favorites.FavoritesPageConfig
            {
                Id = config.Id,
                CreatedAt = config.CreatedAt,
                UpdatedAt = config.UpdatedAt,
                IsActive = config.IsActive,
                IsDeleted = config.IsDeleted,
                ShowTitle = config.ShowTitle,
                HeaderTitle = Resolve(config.HeaderTitle, "FavoritesPage_DefaultHeaderTitle", "المفضلة", "Favorites"),
                HeaderSubtitle = Resolve(config.HeaderSubtitle, "FavoritesPage_DefaultHeaderSubtitle", "قائمة بالمنتجات التي تم الإعجاب بها", "List of products you liked"),
                ShowAddAllToCart = config.ShowAddAllToCart,
                AddAllToCartText = Resolve(config.AddAllToCartText, "FavoritesPage_DefaultAddAllToCartText", "إضافة جميع المنتجات إلى السلة", "Add all products to cart"),
                ShowToolbar = config.ShowToolbar,
                ShowSort = config.ShowSort,
                ShowCount = config.ShowCount,
                ShowProductColor = config.ShowProductColor,
                ShowProductSize = config.ShowProductSize,
                ShowProductPrice = config.ShowProductPrice,
                ShowProductOldPrice = config.ShowProductOldPrice,
                ShowProductStock = config.ShowProductStock,
                ShowRemoveAction = config.ShowRemoveAction,
                ShowMoveToCartAction = config.ShowMoveToCartAction,
                EmptyStateTitle = Resolve(config.EmptyStateTitle, "FavoritesPage_DefaultEmptyStateTitle", "قائمة المفضلة فارغة", "Favorites list is empty"),
                EmptyStateSubtitle = Resolve(config.EmptyStateSubtitle, "FavoritesPage_DefaultEmptyStateSubtitle", "لم تقم بإضافة أي منتج إلى قائمة المفضلة بعد", "You haven't added any products to your favorites list yet"),
                EmptyStateButtonText = Resolve(config.EmptyStateButtonText, "FavoritesPage_DefaultEmptyStateButtonText", "اكتشف التسوق", "Explore Shopping"),
                ShowEmptyStateIllustration = config.ShowEmptyStateIllustration,
                ShowTrustBadges = config.ShowTrustBadges,
                TrustBadgesJson = config.TrustBadgesJson
            };
        }

        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(resultConfig), options, cancellationToken);

        return Result.Success(resultConfig);
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
