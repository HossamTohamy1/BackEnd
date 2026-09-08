using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using loxxking_backend_clean.Application.Features.Categories.Queries.GetCategories;

namespace loxxking_backend_clean.Application.Features.Categories.Queries.GetCategoryBySlug;

public class GetCategoryBySlugHandler : IRequestHandler<GetCategoryBySlugQuery, Result<CategoryListResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public GetCategoryBySlugHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<CategoryListResponse>> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"Category_Slug_{request.Slug}";

        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cached = JsonSerializer.Deserialize<CategoryListResponse>(cachedData);
            if (cached != null) return Result.Success(cached);
        }

        var category = await _context.Categories
            .Where(c => c.Slug.ToLower() == request.Slug.ToLower())
            .Select(c => new CategoryListResponse(
                c.Id,
                c.Slug,
                c.NameEn,
                c.NameAr,
                c.ImageUrl,
                _context.Products.Count(p => p.CategoryId == c.Id)
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
        {
            return Result.Failure<CategoryListResponse>(new Error("Error.NotFound", "Category_NotFound"));
        }

        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(category), options, cancellationToken);

        return Result.Success(category);
    }
}
