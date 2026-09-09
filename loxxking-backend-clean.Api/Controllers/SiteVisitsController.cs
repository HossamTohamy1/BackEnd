using loxxking_backend_clean.Application.Common.Interfaces;
using loxxking_backend_clean.Application.Features.SiteVisits.Commands.LogVisit;
using loxxking_backend_clean.Application.Features.SiteVisits.Queries.GetTodayCount;
using loxxking_backend_clean.Application.Features.SiteVisits.Queries.GetVisits;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/site-visits")]
public class SiteVisitsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IIpResolverService _ipResolver;

    public SiteVisitsController(ISender sender, IIpResolverService ipResolver)
    {
        _sender = sender;
        _ipResolver = ipResolver;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> LogVisit([FromBody] LogVisitRequest request, CancellationToken cancellationToken)
    {
        var ip = _ipResolver.GetClientIpAddress();
        var command = new LogVisitCommand(request.CountryId, request.Page, ip);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToApiResponse();
    }

    [HttpGet("today-count")]
    [Authorize(Roles = "Admin,StoreManager,SalesEmployee")]
    public async Task<IActionResult> GetTodayCount(CancellationToken cancellationToken)
    {
        var query = new GetTodayCountQuery();
        var result = await _sender.Send(query, cancellationToken);
        return result.ToApiResponse();
    }

    [HttpGet]
    [Authorize(Roles = "Admin,StoreManager,SalesEmployee")]
    public async Task<IActionResult> GetVisits(
        [FromQuery] Guid? countryId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVisitsQuery(countryId, dateFrom, dateTo, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);
        return result.ToApiResponse();
    }
}

public record LogVisitRequest(Guid? CountryId, string Page);
