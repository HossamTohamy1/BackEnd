using loxxking_backend_clean.Application.Features.Products.Queries.GetProducts;

namespace loxxking_backend_clean.Application.Features.Favorites.Queries.GetMyFavorites;

public record GetMyFavoritesQuery(Guid UserId, string? GuestId) : IRequest<Result<List<GetMyFavoritesResponse>>>;

public record GetMyFavoritesResponse(
    Guid Id,
    Guid ProductId,
    DateTime AddedAt,
    ProductListResponse Product
);
