using loxxking_backend_clean.Application.Features.Favorites.Commands.AddFavorite;
using loxxking_backend_clean.Application.Features.Favorites.Commands.RemoveFavorite;
using loxxking_backend_clean.Application.Features.Favorites.Commands.MergeGuestFavorites;
using loxxking_backend_clean.Application.Features.Favorites.Queries.CheckFavorite;
using loxxking_backend_clean.Application.Features.Favorites.Queries.GetMyFavorites;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/favorites")]
public class FavoritesController : ControllerBase
{
    private readonly ISender _sender;
    public FavoritesController(ISender sender) { _sender = sender; }

    [HttpGet]
    public async Task<IActionResult> GetMyFavorites([FromHeader(Name = "X-Guest-Id")] string? guestId, CancellationToken ct) {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        var userId = userIdStr is not null && Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
        return (await _sender.Send(new GetMyFavoritesQuery(userId, guestId), ct)).ToApiResponse();
    }

    [HttpPost("{productId}")]
    public async Task<IActionResult> AddFavorite(Guid productId, [FromHeader(Name = "X-Guest-Id")] string? guestId, CancellationToken ct) {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        var userId = userIdStr is not null && Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
        return (await _sender.Send(new AddFavoriteCommand(productId, userId, guestId), ct)).ToApiResponse();
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveFavorite(Guid productId, [FromHeader(Name = "X-Guest-Id")] string? guestId, CancellationToken ct) {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        var userId = userIdStr is not null && Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
        return (await _sender.Send(new RemoveFavoriteCommand(productId, userId, guestId), ct)).ToApiResponse();
    }

    [HttpGet("check/{productId}")]
    public async Task<IActionResult> CheckFavorite(Guid productId, [FromHeader(Name = "X-Guest-Id")] string? guestId, CancellationToken ct) {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        var userId = userIdStr is not null && Guid.TryParse(userIdStr, out var id) ? id : (Guid?)null;
        return (await _sender.Send(new CheckFavoriteQuery(productId, userId, guestId), ct)).ToApiResponse();
    }

    [HttpPost("merge-guest")]
[Authorize]
    public async Task<IActionResult> MergeGuestFavorites([FromBody] MergeGuestFavoritesCommand cmd, CancellationToken ct) {
        return (await _sender.Send(cmd, ct)).ToApiResponse();
    }
}
