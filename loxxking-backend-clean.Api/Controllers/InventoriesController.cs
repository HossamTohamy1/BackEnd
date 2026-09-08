using loxxking_backend_clean.Application.Features.Inventories.Commands.SetProductInventory;
using loxxking_backend_clean.Application.Features.Inventories.Queries.GetProductInventory;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,StoreManager")]
public class InventoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{productId:guid}")]
    public async Task<IActionResult> GetInventory(Guid productId)
    {
        var result = await _mediator.Send(new GetProductInventoryQuery(productId));
        return result.ToApiResponse();
    }

    [HttpPut("{productId:guid}/country/{countryId:guid}")]
    public async Task<IActionResult> SetInventory(Guid productId, Guid countryId, [FromBody] int quantity)
    {
        var result = await _mediator.Send(new SetProductInventoryCommand(productId, countryId, quantity));
        return result.ToApiResponse();
    }
}
