namespace loxxking_backend_clean.Domain.Entities.Favorites;

public class FavoritesPageConfig : BaseEntity
{
    public bool ShowTitle { get; set; } = true;
    public string HeaderTitle { get; set; } = string.Empty;
    public string HeaderSubtitle { get; set; } = string.Empty;
    
    public bool ShowAddAllToCart { get; set; } = true;
    public string AddAllToCartText { get; set; } = string.Empty;
    
    public bool ShowToolbar { get; set; } = true;
    public bool ShowSort { get; set; } = true;
    public bool ShowCount { get; set; } = true;
    
    public bool ShowProductColor { get; set; } = true;
    public bool ShowProductSize { get; set; } = true;
    public bool ShowProductPrice { get; set; } = true;
    public bool ShowProductOldPrice { get; set; } = true;
    public bool ShowProductStock { get; set; } = true;
    public bool ShowRemoveAction { get; set; } = true;
    public bool ShowMoveToCartAction { get; set; } = true;
    
    public string EmptyStateTitle { get; set; } = string.Empty;
    public string EmptyStateSubtitle { get; set; } = string.Empty;
    public string EmptyStateButtonText { get; set; } = string.Empty;
    public bool ShowEmptyStateIllustration { get; set; } = true;
    
    public bool ShowTrustBadges { get; set; } = true;
    public string TrustBadgesJson { get; set; } = "[]";
}
