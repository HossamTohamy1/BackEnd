using Microsoft.Extensions.Caching.Distributed;

namespace loxxking_backend_clean.Application.Features.FavoritesPageConfig.Commands.UpdateFavoritesPageConfig;

public record UpdateFavoritesPageConfigCommand(
    bool ShowTitle,
    string HeaderTitle,
    string HeaderSubtitle,
    bool ShowAddAllToCart,
    string AddAllToCartText,
    bool ShowToolbar,
    bool ShowSort,
    bool ShowCount,
    bool ShowProductColor,
    bool ShowProductSize,
    bool ShowProductPrice,
    bool ShowProductOldPrice,
    bool ShowProductStock,
    bool ShowRemoveAction,
    bool ShowMoveToCartAction,
    string EmptyStateTitle,
    string EmptyStateSubtitle,
    string EmptyStateButtonText,
    bool ShowEmptyStateIllustration,
    bool ShowTrustBadges,
    string TrustBadgesJson
) : IRequest<Result<Domain.Entities.Favorites.FavoritesPageConfig>>;

public class UpdateFavoritesPageConfigHandler : IRequestHandler<UpdateFavoritesPageConfigCommand, Result<Domain.Entities.Favorites.FavoritesPageConfig>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public UpdateFavoritesPageConfigHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<Domain.Entities.Favorites.FavoritesPageConfig>> Handle(UpdateFavoritesPageConfigCommand request, CancellationToken cancellationToken)
    {
        var config = await _context.FavoritesPageConfigs.FirstOrDefaultAsync(cancellationToken);
        if (config == null)
        {
            config = new Domain.Entities.Favorites.FavoritesPageConfig();
            _context.FavoritesPageConfigs.Add(config);
        }

        config.ShowTitle = request.ShowTitle;
        config.HeaderTitle = request.HeaderTitle;
        config.HeaderSubtitle = request.HeaderSubtitle;
        config.ShowAddAllToCart = request.ShowAddAllToCart;
        config.AddAllToCartText = request.AddAllToCartText;
        config.ShowToolbar = request.ShowToolbar;
        config.ShowSort = request.ShowSort;
        config.ShowCount = request.ShowCount;
        config.ShowProductColor = request.ShowProductColor;
        config.ShowProductSize = request.ShowProductSize;
        config.ShowProductPrice = request.ShowProductPrice;
        config.ShowProductOldPrice = request.ShowProductOldPrice;
        config.ShowProductStock = request.ShowProductStock;
        config.ShowRemoveAction = request.ShowRemoveAction;
        config.ShowMoveToCartAction = request.ShowMoveToCartAction;
        config.EmptyStateTitle = request.EmptyStateTitle;
        config.EmptyStateSubtitle = request.EmptyStateSubtitle;
        config.EmptyStateButtonText = request.EmptyStateButtonText;
        config.ShowEmptyStateIllustration = request.ShowEmptyStateIllustration;
        config.ShowTrustBadges = request.ShowTrustBadges;
        config.TrustBadgesJson = request.TrustBadgesJson;

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync("PageConfig_Favorites_ar", cancellationToken);
        await _cache.RemoveAsync("PageConfig_Favorites_en", cancellationToken);

        return Result.Success(config);
    }
}
