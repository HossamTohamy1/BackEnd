using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using loxxking_backend_clean.Application.Common.Interfaces;

namespace loxxking_backend_clean.Application.Features.HomePageConfig.Queries.GetHomePageConfig;

public class GetHomePageConfigHandler : IRequestHandler<GetHomePageConfigQuery, Result<HomePageConfigResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "PageConfig_HomePage";

    public GetHomePageConfigHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<HomePageConfigResponse>> Handle(GetHomePageConfigQuery request, CancellationToken cancellationToken)
    {
        var cachedData = await _cache.GetStringAsync(CacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedConfig = JsonSerializer.Deserialize<HomePageConfigResponse>(cachedData);
            if (cachedConfig != null) return Result.Success(cachedConfig);
        }

        var config = await _context.HomePageConfigs
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        HomePageConfigResponse response;

        if (config == null)
        {
            response = new HomePageConfigResponse("[]", 1, null);
        }
        else
        {
            response = new HomePageConfigResponse(config.SectionsJson, config.Version, config.UpdatedAt);
        }

        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) };
        await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(response), options, cancellationToken);

        return Result.Success(response);
    }
}
