namespace loxxking_backend_clean.Application.Features.ProductPrices.Queries.GetProductsWithPricesByCountry;

public record GetProductsWithPricesByCountryQuery(Guid? CountryId) : IRequest<Result<List<object>>>;

public class GetProductsWithPricesByCountryHandler : IRequestHandler<GetProductsWithPricesByCountryQuery, Result<List<object>>>
{
    private readonly IApplicationDbContext _context;
    public GetProductsWithPricesByCountryHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result<List<object>>> Handle(GetProductsWithPricesByCountryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsQueryable();
        var results = await query.Select(p => new {
            p.Id, p.NameEn, p.NameAr, p.BasePrice,
            CountryPrices = p.Prices.Select(pp => new { pp.CountryId, CountryName = pp.Country.Name, pp.Price, pp.Country.Currency })
        }).ToListAsync(cancellationToken);

        if (request.CountryId.HasValue) {
            results = results.Select(p => new {
                p.Id, p.NameEn, p.NameAr, p.BasePrice,
                CountryPrices = p.CountryPrices.Where(cp => cp.CountryId == request.CountryId.Value)
            }).ToList();
        }

        return Result.Success<List<object>>(results.Cast<object>().ToList());
    }
}
