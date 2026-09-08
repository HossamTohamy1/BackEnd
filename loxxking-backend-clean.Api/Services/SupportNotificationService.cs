using loxxking_backend_clean.Api.Hubs;
using loxxking_backend_clean.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace loxxking_backend_clean.Api.Services;

public class SupportNotificationService : ISupportNotificationService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public SupportNotificationService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyMessageReceivedAsync(string conversationId, Guid? userId, string? userName, string message, DateTime timestamp)
    {
        await _hubContext.Clients.Group($"conversation_{conversationId}").SendAsync("ReceiveMessage", new
        {
            userId,
            userName,
            message,
            timestamp
        });
    }
}
