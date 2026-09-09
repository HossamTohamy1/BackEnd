using loxxking_backend_clean.Application.Features.HomePageConfig.Commands.UpdateHomePageConfig;
using loxxking_backend_clean.Application.Features.HomePageConfig.Queries.GetHomePageConfig;
using Microsoft.AspNetCore.Authorization;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/home-page-config")]
public class HomePageConfigController : ControllerBase
{
    private readonly ISender _sender;
    public HomePageConfigController(ISender sender) { _sender = sender; }

    [HttpGet]
    public async Task<IActionResult> GetConfig(CancellationToken ct) =>
        (await _sender.Send(new GetHomePageConfigQuery(), ct)).ToApiResponse();

    [HttpPut]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> UpdateConfig([FromBody] UpdateHomePageConfigCommand cmd, CancellationToken ct) =>
        (await _sender.Send(cmd, ct)).ToApiResponse();
}
