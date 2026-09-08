namespace loxxking_backend_clean.Application.Features.Favorites.Queries.CheckFavorite;

public record CheckFavoriteQuery(Guid ProductId, Guid? UserId, string? GuestId) : IRequest<Result<bool>>;

public class CheckFavoriteHandler : IRequestHandler<CheckFavoriteQuery, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public CheckFavoriteHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result<bool>> Handle(CheckFavoriteQuery request, CancellationToken cancellationToken)
    {
        var exists = await _context.FavoriteItems.AnyAsync(w => w.ProductId == request.ProductId && (request.UserId.HasValue ? w.UserId == request.UserId : w.GuestId == request.GuestId), cancellationToken);
        return Result.Success(exists);
    }
}
