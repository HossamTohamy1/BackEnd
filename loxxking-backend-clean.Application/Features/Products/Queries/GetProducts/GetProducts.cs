using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace loxxking_backend_clean.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(Guid? CategoryId) : IRequest<Result<List<ProductListResponse>>>;
public record ProductListResponse(
    Guid Id, 
    string Slug,
    string NameEn, 
    string NameAr, 
    string DescEn,
    string DescAr,
    decimal Price,
    decimal? OriginalPrice,
    List<string> Images,
    string Category,
    List<string> Sizes,
    string? SizeChart,
    int Stock,
    double Rating,
    int ReviewCount,
    bool IsNew,
    bool IsBestSeller,
    string? Badge,
    List<string> Colors
);

public class GetProductsHandler : IRequestHandler<GetProductsQuery, Result<List<ProductListResponse>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    
    public GetProductsHandler(IApplicationDbContext context, IDistributedCache cache) 
    { 
        _context = context; 
        _cache = cache;
    }

    public async Task<Result<List<ProductListResponse>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"ProductsList_{request.CategoryId}";
        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
        
        List<ProductListResponse>? products = null;
        if (!string.IsNullOrEmpty(cachedData))
        {
            products = JsonSerializer.Deserialize<List<ProductListResponse>>(cachedData);
        }

        if (products == null)
        {
            var query = _context.Products.AsQueryable();
            if (request.CategoryId.HasValue) query = query.Where(p => p.CategoryId == request.CategoryId);
            products = await query.Select(p => new ProductListResponse(
                p.Id, 
                p.Slug,
                p.NameEn, 
                p.NameAr, 
                p.Description,
                p.Description, // Using Description for both until we have DescAr/DescEn in db
                p.BasePrice.Value,
                p.OriginalPrice != null ? p.OriginalPrice.Value : null,
                p.Images,
                p.Category.NameEn,
                p.Sizes,
                p.SizeChartJson,
                p.Stock,
                p.Rating,
                p.ReviewCount,
                p.IsNew,
                p.IsBestSeller,
                p.Badge,
                p.Colors
            )).ToListAsync(cancellationToken);
            
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(products), options, cancellationToken);
        }
        
        return Result.Success(products);
    }
}
