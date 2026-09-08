namespace loxxking_backend_clean.Application.Features.Favorites.Commands.RemoveFavorite;

public record RemoveFavoriteCommand(Guid ProductId, Guid UserId, string? GuestId) : IRequest<Result>;
