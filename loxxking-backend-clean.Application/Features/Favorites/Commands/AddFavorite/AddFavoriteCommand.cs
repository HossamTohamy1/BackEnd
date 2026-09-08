namespace loxxking_backend_clean.Application.Features.Favorites.Commands.AddFavorite;

public record AddFavoriteCommand(Guid ProductId, Guid UserId, string? GuestId) : IRequest<Result<AddFavoriteResponse>>;

public record AddFavoriteResponse(string Message, Guid FavoriteItemId);
