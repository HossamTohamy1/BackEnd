namespace loxxking_backend_clean.Application.Features.Products.Queries.GetBestSellers;

public record GetBestSellersQuery(int Top) : IRequest<Result<List<Guid>>>;

public class GetBestSellersHandler : IRequestHandler<GetBestSellersQuery, Result<List<Guid>>>
{
    private readonly IApplicationDbContext _context;
    public GetBestSellersHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result<List<Guid>>> Handle(GetBestSellersQuery request, CancellationToken cancellationToken)
    {
        var best = await _context.OrderItems.GroupBy(oi => oi.ProductId).OrderByDescending(g => g.Sum(oi => oi.Quantity)).Take(request.Top).Select(g => g.Key).ToListAsync(cancellationToken);
        return Result.Success(best);
    }
}
