using loxxking_backend_clean.Application.Features.Countries.Queries.GetCountries;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CountriesController : ControllerBase
{
    private readonly ISender _sender;

    public CountriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetCountriesQuery();
        var result = await _sender.Send(query, cancellationToken);
        return result.ToApiResponse();
    }
}
