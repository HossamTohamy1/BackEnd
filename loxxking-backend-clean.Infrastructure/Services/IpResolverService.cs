using System.Net;
using System.Net.Http;
using loxxking_backend_clean.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace loxxking_backend_clean.Infrastructure.Services;

public class IpResolverService : IIpResolverService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly IMemoryCache? _cache;

    public IpResolverService(
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory? httpClientFactory = null,
        IMemoryCache? cache = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
    }

    public string? GetClientIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return null;

        string? ipString = null;

        // 1. Check common reverse proxy headers
        if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp) && !string.IsNullOrWhiteSpace(cfIp))
        {
            ipString = cfIp.ToString().Split(',')[0].Trim();
        }
        else if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var xff) && !string.IsNullOrWhiteSpace(xff))
        {
            ipString = xff.ToString().Split(',')[0].Trim();
        }
        else if (context.Request.Headers.TryGetValue("X-Real-IP", out var xRealIp) && !string.IsNullOrWhiteSpace(xRealIp))
        {
            ipString = xRealIp.ToString().Trim();
        }
        else if (context.Connection.RemoteIpAddress != null)
        {
            var ip = context.Connection.RemoteIpAddress;
            if (ip.IsIPv4MappedToIPv6)
            {
                ip = ip.MapToIPv4();
            }
            ipString = ip.ToString();
        }

        if (ipString == "::1")
        {
            ipString = "127.0.0.1";
        }

        // If it's already a valid public IP, use it directly!
        if (IsValidPublicIp(ipString))
        {
            return ipString;
        }

        // If local/loopback/private (e.g. testing locally on localhost),
        // resolve the machine's actual external Public IP on the Internet:
        var publicIp = GetOutboundPublicIp();
        if (!string.IsNullOrWhiteSpace(publicIp))
        {
            return publicIp;
        }

        return ipString;
    }

    private string? GetOutboundPublicIp()
    {
        if (_cache != null && _cache.TryGetValue<string>("Machine_Outbound_Public_IP", out var cachedIp) && !string.IsNullOrWhiteSpace(cachedIp))
        {
            return cachedIp;
        }

        try
        {
            if (_httpClientFactory != null)
            {
                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                var response = client.GetStringAsync("http://ip-api.com/json/?fields=query").GetAwaiter().GetResult();
                using var doc = System.Text.Json.JsonDocument.Parse(response);
                if (doc.RootElement.TryGetProperty("query", out var queryElement))
                {
                    var resolvedIp = queryElement.GetString()?.Trim();
                    if (IsValidPublicIp(resolvedIp))
                    {
                        _cache?.Set("Machine_Outbound_Public_IP", resolvedIp, TimeSpan.FromHours(1));
                        return resolvedIp;
                    }
                }
            }
        }
        catch
        {
            // Suppress fallback errors
        }

        return null;
    }

    public bool IsValidPublicIp(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) return false;
        
        if (!IPAddress.TryParse(ipAddress, out var ip)) return false;

        var bytes = ip.GetAddressBytes();

        switch (ip.AddressFamily)
        {
            case System.Net.Sockets.AddressFamily.InterNetwork:
                // IPv4
                if (bytes[0] == 10) return false; // 10.0.0.0/8
                if (bytes[0] == 127) return false; // 127.0.0.0/8 (Loopback)
                if (bytes[0] == 169 && bytes[1] == 254) return false; // 169.254.0.0/16 (Link-local)
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return false; // 172.16.0.0/12
                if (bytes[0] == 192 && bytes[1] == 168) return false; // 192.168.0.0/16
                break;
            case System.Net.Sockets.AddressFamily.InterNetworkV6:
                // IPv6
                if (IPAddress.IsLoopback(ip)) return false; // ::1
                if (ip.IsIPv6LinkLocal) return false; // fe80::/10
                if (ip.IsIPv6SiteLocal) return false; // fec0::/10
                // UniqueLocal is not a direct property, we can check prefix if necessary, but skipping for now or check bytes.
                break;
            default:
                return false;
        }

        return true;
    }
}
