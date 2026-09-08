namespace loxxking_backend_clean.Application.Features.Favorites.Commands.RemoveFavorite;

public class RemoveFavoriteHandler : IRequestHandler<RemoveFavoriteCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveFavoriteHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveFavoriteCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId == Guid.Empty && string.IsNullOrWhiteSpace(request.GuestId))
        {
            return Result.Failure(new Error("Error.Unauthorized", "Favorite_LoginOrGuestRequired"));
        }

        var item = await _context.FavoriteItems
            .FirstOrDefaultAsync(w => w.ProductId == request.ProductId && (request.UserId != Guid.Empty ? w.UserId == request.UserId : w.GuestId == request.GuestId), cancellationToken);

        if (item == null) return Result.Failure(new Error("Error.NotFound", "Favorite_ItemNotFound"));

        _context.FavoriteItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
