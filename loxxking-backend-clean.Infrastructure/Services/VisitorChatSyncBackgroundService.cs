using System.Net.Http.Json;
using loxxking_backend_clean.Application.Common.Interfaces;
using loxxking_backend_clean.Domain.Entities.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace loxxking_backend_clean.Infrastructure.Services;

public class VisitorChatSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VisitorChatSyncBackgroundService> _logger;
    private readonly TimeSpan _delay = TimeSpan.FromSeconds(2);
    private readonly int _maxAttempts = 5;

    public VisitorChatSyncBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<VisitorChatSyncBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("VisitorChatSyncBackgroundService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing VisitorChatSyncBackgroundService.");
            }

            await Task.Delay(_delay, stoppingToken);
        }
    }

    private async Task ProcessPendingMessagesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var enabled = configuration.GetValue<bool>("LegacyCrm:Enabled", false);
        if (!enabled) return;

        var baseUrl = configuration.GetValue<string>("LegacyCrm:BaseUrl");
        var apiKey = configuration.GetValue<string>("LegacyCrm:ChatApiKey") ?? configuration.GetValue<string>("LegacyCrm:ApiKey");

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(apiKey)) return;

        // Note: we only sync messages sent by Guest (SenderId == null, GuestName != null)
        // OR by a user if that is the case. Usually visitors don't have SenderId.
        var pendingMessages = await dbContext.SupportMessages
            .Where(m => !m.IsSyncedToCrm && m.SyncAttempts < _maxAttempts && m.SenderId == null)
            .OrderBy(m => m.CreatedAt)
            .Take(50)
            .ToListAsync(stoppingToken);

        if (!pendingMessages.Any()) return;

        var client = httpClientFactory.CreateClient("LegacyCrmClient");
        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Token {apiKey}");

        foreach (var message in pendingMessages)
        {
            message.SyncAttempts++;
            try
            {
                var payload = new
                {
                    VisitorSessionId = message.ConversationId.ToString(),
                    ClientMessageId = message.ClientMessageId ?? message.Id.ToString(),
                    StoreName = "Loxxking",
                    Message = message.Message,
                    AttachmentUrl = message.AttachmentUrl,
                    SenderName = message.GuestName ?? "Guest"
                };

                var response = await client.PostAsJsonAsync("api/VisitorChat/IncomingMessage", payload, stoppingToken);

                if (response.IsSuccessStatusCode)
                {
                    message.IsSyncedToCrm = true;
                    message.SyncedToCrmAt = DateTime.UtcNow;
                    message.SyncError = null;
                    _logger.LogInformation("Successfully synced visitor message {MessageId} to CRM.", message.Id);
                }
                else
                {
                    var responseStr = await response.Content.ReadAsStringAsync(stoppingToken);
                    message.SyncError = $"HTTP {(int)response.StatusCode}: {responseStr}";
                    _logger.LogWarning("Failed to sync visitor message {MessageId}. Status: {StatusCode}", message.Id, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                message.SyncError = ex.Message;
                _logger.LogError(ex, "Exception while syncing visitor message {MessageId}.", message.Id);
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }
}
