using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace loxxking_backend_clean.Application.Features.Countries.Queries.GetCountries;

public class GetCountriesHandler : IRequestHandler<GetCountriesQuery, Result<List<GetCountriesResponse>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public GetCountriesHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<List<GetCountriesResponse>>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = "Countries_All";
        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedCountries = JsonSerializer.Deserialize<List<GetCountriesResponse>>(cachedData);
            if (cachedCountries != null) return Result.Success(cachedCountries);
        }

        var countries = await _context.Countries
            .OrderBy(c => c.Name)
            .Select(c => new GetCountriesResponse(
                c.Id,
                c.Name,
                c.Currency,
                c.DefaultLanguage
            ))
            .ToListAsync(cancellationToken);

        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7) };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(countries), options, cancellationToken);

        return Result.Success(countries);
    }
}
