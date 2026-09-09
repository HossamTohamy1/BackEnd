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
    [Authorize]
    public async Task<IActionResult> GetMessages(Guid conversationId, CancellationToken ct) => (await _sender.Send(new GetMessagesQuery(conversationId), ct)).ToApiResponse();

    [HttpGet("conversations/{conversationId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetConversationById(Guid conversationId, CancellationToken ct) => (await _sender.Send(new GetMessagesQuery(conversationId), ct)).ToApiResponse();

    [HttpPost("send")]
    [Authorize]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageCommand cmd, CancellationToken ct) {
        return (await _sender.Send(cmd, ct)).ToApiResponse();
    }

    [HttpPost("conversations/{conversationId:guid}/messages")]
    [Authorize]
    public async Task<IActionResult> SendConversationMessage(Guid conversationId, [FromBody] ChatMessageInputDto input, CancellationToken ct)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        var userId = userIdStr is not null && Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
        var isStaff = User.IsInRole("Admin") || User.IsInRole("StoreManager") || User.IsInRole("SalesEmployee");
        var text = input.Text ?? input.Message ?? "";
        return (await _sender.Send(new SendMessageCommand(conversationId, text, userId, null, isStaff), ct)).ToApiResponse();
    }

    [HttpGet("conversations")]
    [Authorize(Roles = "Admin,StoreManager,SalesEmployee")]
    public async Task<IActionResult> GetConversations(CancellationToken ct) => (await _sender.Send(new GetConversationsQuery(), ct)).ToApiResponse();

    [HttpPatch("conversations/{conversationId}/read")]
    [HttpPost("conversations/{conversationId}/read")]
    [Authorize(Roles = "Admin,StoreManager,SalesEmployee")]
    public async Task<IActionResult> MarkRead(Guid conversationId, CancellationToken ct) => (await _sender.Send(new MarkConversationReadCommand(conversationId), ct)).ToApiResponse();

    [HttpGet("conversations/my")]
    [Authorize]
    public async Task<IActionResult> GetMyConversation(CancellationToken ct)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
        return (await _sender.Send(new loxxking_backend_clean.Application.Features.Support.Queries.GetMyConversation.GetMyConversationQuery(userId), ct)).ToApiResponse();
    }
}

public record ChatMessageInputDto(string? Text, string? Message, string? Sender);
