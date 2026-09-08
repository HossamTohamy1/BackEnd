namespace loxxking_backend_clean.Application.Features.Inventories.Queries.GetProductInventory;

public record GetProductInventoryQuery(Guid ProductId) : IRequest<Result<List<GetProductInventoryResponse>>>;
public record GetProductInventoryResponse(Guid CountryId, string CountryName, int Quantity);

public class GetProductInventoryHandler : IRequestHandler<GetProductInventoryQuery, Result<List<GetProductInventoryResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetProductInventoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<GetProductInventoryResponse>>> Handle(GetProductInventoryQuery request, CancellationToken cancellationToken)
    {
        var inventories = await _context.InventoryItems
            .Include(i => i.Country)
            .Where(i => i.ProductId == request.ProductId)
            .Select(i => new GetProductInventoryResponse(i.CountryId, i.Country.Name, i.Quantity))
            .ToListAsync(cancellationToken);

        return Result.Success(inventories);
    }
}
