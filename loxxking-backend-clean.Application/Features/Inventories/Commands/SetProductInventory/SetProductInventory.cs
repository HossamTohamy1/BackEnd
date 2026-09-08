using loxxking_backend_clean.Domain.Entities.Products;

namespace loxxking_backend_clean.Application.Features.Inventories.Commands.SetProductInventory;

public record SetProductInventoryCommand(Guid ProductId, Guid CountryId, int Quantity) : IRequest<Result>;

public class SetProductInventoryHandler : IRequestHandler<SetProductInventoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public SetProductInventoryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(SetProductInventoryCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product is null) return Result.Failure(new Error("Error.NotFound", "Product_NotFound"));

        var country = await _context.Countries.FirstOrDefaultAsync(c => c.Id == request.CountryId, cancellationToken);
        if (country is null) return Result.Failure(new Error("Error.NotFound", "Country_NotFound"));

        var inventory = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.CountryId == request.CountryId, cancellationToken);

        if (inventory is null)
        {
            inventory = loxxking_backend_clean.Domain.Entities.Inventory.InventoryItem.Create(request.ProductId, request.CountryId, request.Quantity);
            _context.InventoryItems.Add(inventory);
        }
        else
        {
            inventory.UpdateQuantity(request.Quantity);
            _context.InventoryItems.Update(inventory);
        }

        await loxxking_backend_clean.Application.Features.Inventories.Helpers.InventorySyncHelper.SyncProductStockAsync(request.ProductId, _context, cancellationToken);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
