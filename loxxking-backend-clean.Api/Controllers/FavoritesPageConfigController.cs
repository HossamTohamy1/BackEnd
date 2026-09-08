using loxxking_backend_clean.Application.Features.FavoritesPageConfig.Commands.UpdateFavoritesPageConfig;
using loxxking_backend_clean.Application.Features.FavoritesPageConfig.Queries.GetFavoritesPageConfig;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/favorites-config")]
public class FavoritesPageConfigController : ControllerBase
{
    private readonly ISender _sender;
    public FavoritesPageConfigController(ISender sender) { _sender = sender; }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) {
        return (await _sender.Send(new GetFavoritesPageConfigQuery(), ct)).ToApiResponse();
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateFavoritesPageConfigCommand cmd, CancellationToken ct) {
        return (await _sender.Send(cmd, ct)).ToApiResponse();
    }
}
