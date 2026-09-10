using loxxking_backend_clean.Application.Features.Support.Commands.MarkConversationRead;
using loxxking_backend_clean.Application.Features.Support.Commands.SendMessage;
using loxxking_backend_clean.Application.Features.Support.Queries.GetConversations;
using loxxking_backend_clean.Application.Features.Support.Queries.GetMessages;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/support-chat")]
[Route("api/chat")]
public class SupportChatController : ControllerBase
{
    private readonly ISender _sender;
    public SupportChatController(ISender sender) { _sender = sender; }

    [HttpGet("messages/{conversationId}")]
    [HttpGet("conversations/{conversationId:guid}/messages")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMessages(Guid conversationId, CancellationToken ct) => (await _sender.Send(new GetMessagesQuery(conversationId), ct)).ToApiResponse();

    [HttpGet("conversations/{conversationId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetConversationById(Guid conversationId, CancellationToken ct) => (await _sender.Send(new GetMessagesQuery(conversationId), ct)).ToApiResponse();

    [HttpPost("send")]
    [AllowAnonymous]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("VisitorChatLimiter")]
    public async Task<IActionResult> SendMessage(
        [FromBody] SendMessageCommand cmd,
        [FromHeader(Name = "X-Guest-Id")] string? guestId,
        CancellationToken ct)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        var userId = userIdStr is not null && Guid.TryParse(userIdStr, out var id) ? id : cmd.UserId;
        var isStaff = User.IsInRole("Admin") || User.IsInRole("StoreManager") || User.IsInRole("SalesEmployee");
        return (await _sender.Send(cmd with { UserId = userId, IsStaff = isStaff, GuestId = guestId }, ct)).ToApiResponse();
    }

    [HttpPost("conversations/{conversationId:guid}/messages")]
    [AllowAnonymous]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("VisitorChatLimiter")]
    public async Task<IActionResult> SendConversationMessage(
        Guid conversationId,
        [FromBody] ChatMessageInputDto input,
        [FromHeader(Name = "X-Guest-Id")] string? guestId,
        CancellationToken ct)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        var userId = userIdStr is not null && Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
        var isStaff = User.IsInRole("Admin") || User.IsInRole("StoreManager") || User.IsInRole("SalesEmployee");
        var text = input.Text ?? input.Message ?? "";
        return (await _sender.Send(new SendMessageCommand(conversationId, text, userId, null, isStaff, guestId, input.ClientMessageId), ct)).ToApiResponse();
    }

    [HttpGet("conversations")]
    [Authorize(Roles = "Admin,StoreManager,SalesEmployee")]
    public async Task<IActionResult> GetConversations(CancellationToken ct) => (await _sender.Send(new GetConversationsQuery(), ct)).ToApiResponse();

    [HttpPatch("conversations/{conversationId}/read")]
    [HttpPost("conversations/{conversationId}/read")]
    [AllowAnonymous]
    public async Task<IActionResult> MarkRead(
        Guid conversationId,
        [FromHeader(Name = "X-Guest-Id")] string? guestId,
        CancellationToken ct)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        var userId = userIdStr is not null && Guid.TryParse(userIdStr, out var id) ? (Guid?)id : null;
        var isStaff = User.IsInRole("Admin") || User.IsInRole("StoreManager") || User.IsInRole("SalesEmployee");
        return (await _sender.Send(new MarkConversationReadCommand(conversationId, userId, guestId, isStaff), ct)).ToApiResponse();
    }

    [HttpGet("conversations/my")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMyConversation(
        [FromHeader(Name = "X-Guest-Id")] string? guestId,
        CancellationToken ct)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        var userId = userIdStr is not null && Guid.TryParse(userIdStr, out var id) ? (Guid?)id : null;

        if (!userId.HasValue && string.IsNullOrWhiteSpace(guestId))
        {
            return Ok(new loxxking_backend_clean.Shared.ApiResponse<loxxking_backend_clean.Application.Features.Support.Queries.GetMyConversation.ConversationWithMessagesResponse?>
            {
                Success = true,
                Data = null
            });
        }

        return (await _sender.Send(new loxxking_backend_clean.Application.Features.Support.Queries.GetMyConversation.GetMyConversationQuery(userId, guestId), ct)).ToApiResponse();
    }

    [HttpPost("incoming-from-crm")]
    [AllowAnonymous]
    public async Task<IActionResult> IncomingFromCrm([FromBody] IncomingCrmMessageDto dto, [FromHeader(Name = "X-CRM-Key")] string crmKey, [FromServices] Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        var expectedKey = configuration["LegacyCrm:IncomingKey"];
        if (string.IsNullOrEmpty(expectedKey) || crmKey != expectedKey)
        {
            return Unauthorized();
        }

        if (!Guid.TryParse(dto.VisitorSessionId, out var conversationId))
            return BadRequest("Invalid VisitorSessionId format.");

        var text = dto.Message ?? "";
        // Sending as Staff so it shows properly on Frontend (isStaff = true). We leave UserId empty since CRM employees don't map to Loxxking Users.
        // We set GuestName to EmployeeName so frontend can display "EmployeeName" for the reply.
        var cmd = new SendMessageCommand(conversationId, text, Guid.Empty, dto.EmployeeName, true, null, dto.ClientMessageId);
        
        return (await _sender.Send(cmd)).ToApiResponse();
    }
}

public record ChatMessageInputDto(string? Text, string? Message, string? Sender, string? ClientMessageId);
public record IncomingCrmMessageDto(string VisitorSessionId, string ClientMessageId, string StoreName, string? Message, string? AttachmentUrl, string EmployeeName);
