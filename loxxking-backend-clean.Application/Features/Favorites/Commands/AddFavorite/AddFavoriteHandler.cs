using loxxking_backend_clean.Domain.Entities.Favorites;

namespace loxxking_backend_clean.Application.Features.Favorites.Commands.AddFavorite;

public class AddFavoriteHandler : IRequestHandler<AddFavoriteCommand, Result<AddFavoriteResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource>? _localizer;

    public AddFavoriteHandler(IApplicationDbContext context, IStringLocalizer<SharedResource>? localizer = null)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<AddFavoriteResponse>> Handle(AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty && string.IsNullOrWhiteSpace(request.GuestId))
        {
            return Result.Failure<AddFavoriteResponse>(new Error("Error.Unauthorized", "Favorite_LoginOrGuestRequired"));
        }

        var productExists = await _context.Products.AnyAsync(p => p.Id == request.ProductId, cancellationToken);
        if (!productExists) return Result.Failure<AddFavoriteResponse>(new Error("Error.NotFound", "Product_NotFound"));

        var existing = await _context.FavoriteItems
            .FirstOrDefaultAsync(w => w.ProductId == request.ProductId && (request.UserId != Guid.Empty ? w.UserId == request.UserId : w.GuestId == request.GuestId), cancellationToken);

        if (existing != null)
        {
            var msg = _localizer?["Favorite_AlreadyInFavorites"].Value ?? "Already in Favorite.";
            return Result.Success(new AddFavoriteResponse(msg, existing.Id));
        }

        var userId = request.UserId != Guid.Empty ? request.UserId : (Guid?)null;
        var item = FavoriteItem.Create(request.ProductId, userId, request.GuestId);

        _context.FavoriteItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        var addedMsg = _localizer?["Favorite_AddedSuccessfully"].Value ?? "Added to Favorite.";
        return Result.Success(new AddFavoriteResponse(addedMsg, item.Id));
    }
}
