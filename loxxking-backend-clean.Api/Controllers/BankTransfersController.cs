using loxxking_backend_clean.Application.Features.BankTransfers.Commands.ReviewTransfer;
using loxxking_backend_clean.Application.Features.BankTransfers.Commands.UploadTransfer;
using loxxking_backend_clean.Application.Features.BankTransfers.Queries.GetPendingTransfers;
using loxxking_backend_clean.Application.Features.BankTransfers.Queries.GetTransferByOrderId;

using loxxking_backend_clean.Shared;
using loxxking_backend_clean.Shared.Resources;
using Microsoft.Extensions.Localization;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/bank-transfers")]
[Authorize]
public class BankTransfersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public BankTransfersController(ISender sender, IStringLocalizer<SharedResource> localizer)
    {
        _sender = sender;
        _localizer = localizer;
    }

    [HttpPost]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] Guid orderId, [FromForm] IFormFile proofImage, CancellationToken cancellationToken)
    {
        if (proofImage is null || proofImage.Length == 0)
            return BadRequest(ApiResponse<object>.Fail(_localizer.Get("BankTransfer_ProofRequired", "Proof image is required.")));

        var userId = GetCurrentUserId();
        using var stream = proofImage.OpenReadStream();

        var command = new UploadTransferCommand(orderId, stream, proofImage.FileName, proofImage.ContentType, userId);
        var result = await _sender.Send(command, cancellationToken);
        
        return result.ToApiResponse();
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isStaff = User.IsInRole("Admin") || User.IsInRole("StoreManager");

        var query = new GetTransferByOrderIdQuery(orderId, userId, isStaff);
        var result = await _sender.Send(query, cancellationToken);
        
        return result.ToApiResponse();
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var query = new GetPendingTransfersQuery();
        var result = await _sender.Send(query, cancellationToken);
        return result.ToApiResponse();
    }

    [HttpPatch("{id}/review")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> Review(Guid id, [FromBody] ReviewTransferRequest request, CancellationToken cancellationToken)
    {
        var command = new ReviewTransferCommand(id, request.Approved, request.RejectionReason);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToApiResponse();
    }

    private Guid GetCurrentUserId()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        return userIdStr is not null && Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
    }
}

public record ReviewTransferRequest(bool Approved, string? RejectionReason);
