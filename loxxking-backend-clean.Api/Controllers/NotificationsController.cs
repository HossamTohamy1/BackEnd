using loxxking_backend_clean.Application.Features.Notifications.Commands.DeleteNotification;
using loxxking_backend_clean.Application.Features.Notifications.Commands.MarkAllAsRead;
using loxxking_backend_clean.Application.Features.Notifications.Commands.MarkAsRead;
using loxxking_backend_clean.Application.Features.Notifications.Queries.GetMyNotifications;
using loxxking_backend_clean.Application.Features.Notifications.Queries.GetUnreadCount;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ISender _sender;
    public NotificationsController(ISender sender) { _sender = sender; }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly, CancellationToken ct) {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value ?? Guid.Empty.ToString());
        return (await _sender.Send(new GetMyNotificationsQuery(userId), ct)).ToApiResponse();
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct) {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value ?? Guid.Empty.ToString());
        return (await _sender.Send(new GetUnreadCountQuery(userId), ct)).ToApiResponse();
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct) {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value ?? Guid.Empty.ToString());
        return (await _sender.Send(new MarkAsReadCommand(id, userId), ct)).ToApiResponse();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct) {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value ?? Guid.Empty.ToString());
        return (await _sender.Send(new MarkAllAsReadCommand(userId), ct)).ToApiResponse();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value ?? Guid.Empty.ToString());
        return (await _sender.Send(new DeleteNotificationCommand(id, userId), ct)).ToApiResponse();
    }
}
