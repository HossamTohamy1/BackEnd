namespace loxxking_backend_clean.Application.Features.Favorites.Commands.MergeGuestFavorites;

public record MergeGuestFavoritesCommand(string GuestId) : IRequest<Result>;

public class MergeGuestFavoritesHandler : IRequestHandler<MergeGuestFavoritesCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MergeGuestFavoritesHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(MergeGuestFavoritesCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == Guid.Empty)
        {
            return Result.Failure(new Error("Error.Unauthorized", "Favorite_LoginRequiredToMerge"));
        }

        if (string.IsNullOrWhiteSpace(request.GuestId))
        {
            return Result.Failure(new Error("Error.Validation", "Favorite_GuestIdRequired"));
        }

        var guestFavorites = await _context.FavoriteItems
            .Where(f => f.GuestId == request.GuestId)
            .ToListAsync(cancellationToken);

        if (!guestFavorites.Any())
        {
            return Result.Success();
        }

        var userFavoriteProductIds = await _context.FavoriteItems
            .Where(f => f.UserId == currentUserId)
            .Select(f => f.ProductId)
            .ToListAsync(cancellationToken);

        foreach (var guestFavorite in guestFavorites)
        {
            if (userFavoriteProductIds.Contains(guestFavorite.ProductId))
            {
                _context.FavoriteItems.Remove(guestFavorite);
            }
            else
            {
                guestFavorite.AssignToUser(currentUserId);
                _context.FavoriteItems.Update(guestFavorite);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
