using loxxking_backend_clean.Application.Features.OffersPageConfig.Commands.UpdateOffersPageConfig;
using loxxking_backend_clean.Application.Features.OffersPageConfig.Queries.GetOffersPageConfig;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/offers-page-config")]
public class OffersPageConfigController : ControllerBase
{
    private readonly ISender _sender;
    public OffersPageConfigController(ISender sender) { _sender = sender; }

    [HttpGet]
    public async Task<IActionResult> GetConfig(CancellationToken ct) => 
        (await _sender.Send(new GetOffersPageConfigQuery(), ct)).ToApiResponse();

    [HttpPut]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> UpdateConfig([FromBody] UpdateOffersPageConfigCommand cmd, CancellationToken ct) => 
        (await _sender.Send(cmd, ct)).ToApiResponse();
}
