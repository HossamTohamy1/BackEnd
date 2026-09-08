using loxxking_backend_clean.Application.Features.Orders.Commands.CreateOrder;
using loxxking_backend_clean.Application.Features.Orders.Commands.UpdateOrder;
using loxxking_backend_clean.Application.Features.Orders.Commands.UpdateOrderStatus;
using loxxking_backend_clean.Application.Features.Orders.Queries.GetOrderById;
using loxxking_backend_clean.Application.Features.Orders.Queries.GetOrderEditLogs;
using loxxking_backend_clean.Application.Features.Orders.Queries.GetOrders;
using loxxking_backend_clean.Application.Features.Orders.Queries.TrackOrder;
using loxxking_backend_clean.Domain.Enums;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;
    public OrdersController(ISender sender) { _sender = sender; }

    [HttpGet]
    [Authorize(Roles = "Admin,StoreManager,SalesEmployee")]
    public async Task<IActionResult> GetOrders([FromQuery] OrderStatus? status, CancellationToken ct) => (await _sender.Send(new GetOrdersQuery(status, null, null, null, null, null, 1, 10), ct)).ToApiResponse();

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,StoreManager,SalesEmployee")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) => (await _sender.Send(new GetOrderByIdQuery(id), ct)).ToApiResponse();

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand cmd, CancellationToken ct) => (await _sender.Send(cmd, ct)).ToApiResponse();

    [HttpPost("guest")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateGuestOrder([FromBody] CreateOrderCommand cmd, CancellationToken ct) => (await _sender.Send(cmd, ct)).ToApiResponse();

    [HttpPatch("{id}/status")]
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,StoreManager,SalesEmployee")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusCommand cmd, CancellationToken ct) {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value ?? Guid.Empty.ToString());
        return (await _sender.Send(cmd, ct)).ToApiResponse();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderCommand cmd, CancellationToken ct) {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value ?? Guid.Empty.ToString());
        return (await _sender.Send(cmd, ct)).ToApiResponse();
    }

    [HttpGet("{id}/edit-logs")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetEditLogs(Guid id, CancellationToken ct) => (await _sender.Send(new GetOrderEditLogsQuery(id), ct)).ToApiResponse();

    [HttpPost("track")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackOrder([FromBody] TrackOrderQuery query, CancellationToken ct) => (await _sender.Send(query, ct)).ToApiResponse();

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> DeleteOrder(Guid id, CancellationToken ct) => (await _sender.Send(new loxxking_backend_clean.Application.Features.Orders.Commands.DeleteOrder.DeleteOrderCommand(id), ct)).ToApiResponse();

    [HttpPost("bulk-status")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> BulkUpdateStatus([FromBody] loxxking_backend_clean.Application.Features.Orders.Commands.BulkUpdateOrderStatus.BulkUpdateOrderStatusCommand cmd, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value ?? Guid.Empty.ToString());
        return (await _sender.Send(cmd with { UserId = userId }, ct)).ToApiResponse();
    }
}
