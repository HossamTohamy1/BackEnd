using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace loxxking_backend_clean.Application.Features.Categories.Queries.GetCategories;

public record CategoryListResponse(
    Guid Id,
    string Slug,
    string NameEn, 
    string NameAr,
    string Image,
    int ProductCount
);
public record GetCategoriesQuery() : IRequest<Result<List<CategoryListResponse>>>;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryListResponse>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    
    public GetCategoriesHandler(IApplicationDbContext context, IDistributedCache cache) 
    { 
        _context = context; 
        _cache = cache;
    }

    public async Task<Result<List<CategoryListResponse>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = "CategoriesList";
        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
        
        List<CategoryListResponse>? cats = null;
        if (!string.IsNullOrEmpty(cachedData))
        {
            cats = JsonSerializer.Deserialize<List<CategoryListResponse>>(cachedData);
        }

        if (cats == null)
        {
            cats = await _context.Categories
                .Select(c => new CategoryListResponse(
                    c.Id,
                    c.Slug,
                    c.NameEn,
                    c.NameAr,
                    c.ImageUrl,
                    _context.Products.Count(p => p.CategoryId == c.Id)
                ))
                .ToListAsync(cancellationToken);
            
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(cats), options, cancellationToken);
        }
        
        return Result.Success(cats);
    }
}
