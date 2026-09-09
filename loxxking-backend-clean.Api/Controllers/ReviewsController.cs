using loxxking_backend_clean.Application.Features.Reviews.Commands.CreateReview;
using loxxking_backend_clean.Application.Features.Reviews.Commands.DeleteReview;
using loxxking_backend_clean.Application.Features.Reviews.Commands.HideReview;
using loxxking_backend_clean.Application.Features.Reviews.Commands.RejectReview;
using loxxking_backend_clean.Application.Features.Reviews.Commands.ApproveReview;
using loxxking_backend_clean.Application.Features.Reviews.Queries.GetPendingReviews;
using loxxking_backend_clean.Application.Features.Reviews.Queries.GetReviewById;
using loxxking_backend_clean.Application.Features.Reviews.Queries.GetReviewsByProduct;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly ISender _sender;
    public ReviewsController(ISender sender) { _sender = sender; }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? productId,
        [FromHeader(Name = "X-Guest-Id")] string? guestId,
        CancellationToken ct)
    {
        if (productId.HasValue && productId.Value != Guid.Empty)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            var userId = userIdStr is not null && Guid.TryParse(userIdStr, out var id) ? (Guid?)id : null;
            return (await _sender.Send(new GetReviewsByProductQuery(productId.Value, userId, guestId), ct)).ToApiResponse();
        }
        return (await _sender.Send(new GetPendingReviewsQuery(), ct)).ToApiResponse();
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProduct(
        Guid productId,
        [FromHeader(Name = "X-Guest-Id")] string? guestId,
        CancellationToken ct)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        var userId = userIdStr is not null && Guid.TryParse(userIdStr, out var id) ? (Guid?)id : null;
        return (await _sender.Send(new GetReviewsByProductQuery(productId, userId, guestId), ct)).ToApiResponse();
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateReviewCommand cmd,
        [FromHeader(Name = "X-Guest-Id")] string? guestId,
        CancellationToken ct)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        var userId = userIdStr is not null && Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
        return (await _sender.Send(cmd with { UserId = userId, GuestId = guestId }, ct)).ToApiResponse();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) => (await _sender.Send(new GetReviewByIdQuery(id), ct)).ToApiResponse();

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending(CancellationToken ct) => (await _sender.Send(new GetPendingReviewsQuery(), ct)).ToApiResponse();

    [HttpPatch("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct) => (await _sender.Send(new ApproveReviewCommand(id), ct)).ToApiResponse();

    [HttpPatch("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(Guid id, CancellationToken ct) => (await _sender.Send(new RejectReviewCommand(id), ct)).ToApiResponse();

    [HttpPatch("{id}/hide")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Hide(Guid id, CancellationToken ct) => (await _sender.Send(new HideReviewCommand(id), ct)).ToApiResponse();

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => (await _sender.Send(new DeleteReviewCommand(id), ct)).ToApiResponse();
}
