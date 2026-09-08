using System.Net.Http.Json;
using loxxking_backend_clean.Application.Common.Interfaces;
using loxxking_backend_clean.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace loxxking_backend_clean.Infrastructure.Services;

public class LegacyCrmSyncService : ILegacyCrmSyncService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LegacyCrmSyncService> _logger;

    public LegacyCrmSyncService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<LegacyCrmSyncService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<CrmSyncResponse>> SyncOrderAsync(CrmOrderSyncDto dto, CancellationToken cancellationToken = default)
    {
        var enabled = _configuration.GetValue<bool>("LegacyCrm:Enabled", false);
        if (!enabled)
        {
            _logger.LogWarning("Legacy CRM sync is disabled. Skipping sync for LoxxkingOrderId {OrderId}", dto.LoxxkingOrderId);
            return Result<CrmSyncResponse>.Failure<CrmSyncResponse>(new Error("SyncDisabled", "Sync is disabled in configuration."));
        }

        var baseUrl = _configuration.GetValue<string>("LegacyCrm:BaseUrl");
        var apiKey = _configuration.GetValue<string>("LegacyCrm:ApiKey");

        if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("Legacy CRM configuration is missing BaseUrl or ApiKey.");
            return Result<CrmSyncResponse>.Failure<CrmSyncResponse>(new Error("ConfigMissing", "Configuration missing."));
        }

        try
        {
            var client = _httpClientFactory.CreateClient("LegacyCrmClient");
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Token {apiKey}");

            var response = await client.PostAsJsonAsync("Api/Order/SyncFromLoxxking", dto, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<CrmSyncResponse>(cancellationToken: cancellationToken);
                if (content != null && content.Success)
                {
                    _logger.LogInformation("Successfully synced order {OrderId} to Legacy CRM. CrmOrderId: {CrmOrderId}", dto.LoxxkingOrderId, content.LegacyCrmOrderId);
                    return Result<CrmSyncResponse>.Success(content);
                }
                
                var errorMsg = content?.ErrorMessage ?? "Unknown success response structure from CRM.";
                _logger.LogWarning("SyncOrder API returned Ok but Success=false for {OrderId}. Error: {Error}", dto.LoxxkingOrderId, errorMsg);
                return Result<CrmSyncResponse>.Failure<CrmSyncResponse>(new Error("ApiError", errorMsg));
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to sync order {OrderId} to Legacy CRM. Status: {StatusCode}, Body: {Body}", dto.LoxxkingOrderId, response.StatusCode, responseBody);
                return Result<CrmSyncResponse>.Failure<CrmSyncResponse>(new Error("HttpError", $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while syncing order {OrderId} to Legacy CRM.", dto.LoxxkingOrderId);
            return Result<CrmSyncResponse>.Failure<CrmSyncResponse>(new Error("Exception", ex.Message));
        }
    }
}
