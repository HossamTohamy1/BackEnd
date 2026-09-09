using loxxking_backend_clean.Application.Common.Interfaces;
using loxxking_backend_clean.Application.Features.HomePageConfig.Commands.UpdateHomePageConfig;
using loxxking_backend_clean.Application.Features.HomePageConfig.Queries.GetHomePageConfig;
using loxxking_backend_clean.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("upload-image")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> UploadImage([FromServices] IFileStorageService fileStorage, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("File is required"));

        using var stream = file.OpenReadStream();
        var url = await fileStorage.UploadAsync(stream, file.FileName, file.ContentType, "customizer", ct);
        return Ok(ApiResponse<string>.Ok(url));
    }
}

