using System.Net.Http.Json;
using loxxking_backend_clean.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace loxxking_backend_clean.Infrastructure.Services;

public class GeolocationService : IGeolocationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GeolocationService> _logger;
    private readonly string _providerUrlTemplate;
    private readonly TimeSpan _cacheDuration;

    public GeolocationService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<GeolocationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;

        // Default to a free provider like ipapi.co or ipinfo if configured. 
        // Example template: "https://ipapi.co/{0}/country/"
        // Another option: "http://ip-api.com/json/{0}?fields=countryCode"
        _providerUrlTemplate = configuration["Geolocation:ProviderUrlTemplate"] ?? "http://ip-api.com/json/{0}?fields=country,countryCode,currency";
        
        var cacheHours = configuration.GetValue<int?>("Geolocation:CacheDurationHours") ?? 24;
        _cacheDuration = TimeSpan.FromHours(cacheHours);
    }

    public async Task<string?> GetCountryCodeAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        var geo = await GetGeoLocationAsync(ipAddress, cancellationToken);
        return geo?.CountryCode;
    }

    public async Task<GeoLocationResult?> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) return null;

        var cacheKey = $"GeoIP_Full_{ipAddress}";

        if (_cache.TryGetValue<GeoLocationResult>(cacheKey, out var cachedResult) && cachedResult != null)
        {
            return cachedResult;
        }

        try
        {
            var url = string.Format(_providerUrlTemplate, ipAddress);
            using var client = _httpClientFactory.CreateClient("GeolocationClient");
            client.Timeout = TimeSpan.FromSeconds(5); // Fail fast

            var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Geolocation provider returned status code {StatusCode} for IP {IpAddress}", response.StatusCode, ipAddress);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var geo = ExtractGeoLocation(content);

            if (geo != null && !string.IsNullOrWhiteSpace(geo.CountryCode))
            {
                _cache.Set(cacheKey, geo, _cacheDuration);
                return geo;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve geolocation for IP {IpAddress}", ipAddress);
        }

        return null;
    }

    private GeoLocationResult? ExtractGeoLocation(string responseContent)
    {
        string? countryName = null;
        string? countryCode = null;
        string? currency = null;

        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(responseContent);
            var root = document.RootElement;
            
            if (root.TryGetProperty("countryCode", out var ccElement)) countryCode = ccElement.GetString();
            else if (root.TryGetProperty("country_code", out var ccElement2)) countryCode = ccElement2.GetString();

            if (root.TryGetProperty("country", out var cElement)) countryName = cElement.GetString();
            else if (root.TryGetProperty("country_name", out var cElement2)) countryName = cElement2.GetString();

            if (root.TryGetProperty("currency", out var currElement)) currency = currElement.GetString();
        }
        catch
        {
            if (responseContent.Length == 2 || responseContent.Length == 3)
            {
                countryCode = responseContent.Trim();
            }
        }

        if (string.IsNullOrWhiteSpace(countryCode)) return null;

        countryCode = countryCode.Trim().ToUpperInvariant();

        // Dynamically supplement using RegionInfo for ANY country worldwide
        try
        {
            var region = new System.Globalization.RegionInfo(countryCode);
            if (string.IsNullOrWhiteSpace(countryName))
            {
                countryName = region.EnglishName;
            }
            if (string.IsNullOrWhiteSpace(currency))
            {
                currency = region.ISOCurrencySymbol;
            }
        }
        catch
        {
            // RegionInfo fallback if non-standard code
            if (string.IsNullOrWhiteSpace(countryName)) countryName = countryCode;
            if (string.IsNullOrWhiteSpace(currency)) currency = "USD";
        }

        return new GeoLocationResult(countryName, countryCode, currency);
    }
}
