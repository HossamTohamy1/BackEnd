namespace loxxking_backend_clean.Application.Features.Inventory.Queries.GetInventoryByProductId;

public class GetInventoryByProductIdHandler : IRequestHandler<GetInventoryByProductIdQuery, Result<GetInventoryByProductIdResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetInventoryByProductIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetInventoryByProductIdResponse>> Handle(GetInventoryByProductIdQuery request, CancellationToken cancellationToken)
    {
        var inventory = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId, cancellationToken);

        if (inventory is null)
        {
            return Result.Success(new GetInventoryByProductIdResponse(request.ProductId, 0));
        }

        return Result.Success(new GetInventoryByProductIdResponse(inventory.ProductId, inventory.Quantity));
    }
}
