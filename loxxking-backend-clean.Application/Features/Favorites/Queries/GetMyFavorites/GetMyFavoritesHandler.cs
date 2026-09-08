using loxxking_backend_clean.Application.Features.Products.Queries.GetProducts;

namespace loxxking_backend_clean.Application.Features.Favorites.Queries.GetMyFavorites;

public class GetMyFavoritesHandler : IRequestHandler<GetMyFavoritesQuery, Result<List<GetMyFavoritesResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetMyFavoritesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<GetMyFavoritesResponse>>> Handle(GetMyFavoritesQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty && string.IsNullOrWhiteSpace(request.GuestId))
        {
            return Result.Failure<List<GetMyFavoritesResponse>>(new Error("Error.Unauthorized", "Favorite_LoginOrGuestRequired"));
        }

        var query = _context.FavoriteItems
            .Include(w => w.Product)
            .ThenInclude(p => p.Category)
            .AsQueryable();

        if (request.UserId != Guid.Empty)
            query = query.Where(w => w.UserId == request.UserId);
        else
            query = query.Where(w => w.GuestId == request.GuestId);

        var items = await query
            .OrderByDescending(w => w.AddedAt)
            .Select(w => new GetMyFavoritesResponse(
                w.Id,
                w.ProductId,
                w.AddedAt,
                new ProductListResponse(
                    w.Product.Id,
                    w.Product.Slug,
                    w.Product.NameEn,
                    w.Product.NameAr,
                    w.Product.Description,
                    w.Product.Description,
                    w.Product.BasePrice.Value,
                    w.Product.OriginalPrice != null ? w.Product.OriginalPrice.Value : null,
                    w.Product.Images,
                    w.Product.Category.NameEn,
                    w.Product.Sizes,
                    w.Product.SizeChartJson,
                    w.Product.Stock,
                    w.Product.Rating,
                    w.Product.ReviewCount,
                    w.Product.IsNew,
                    w.Product.IsBestSeller,
                    w.Product.Badge,
                    w.Product.Colors
                )
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
