namespace loxxking_backend_clean.Application.Features.Inventories.Helpers;

public static class InventorySyncHelper
{
    /// <summary>
    /// Recalculates the Product.Stock as the FULL SUM of all InventoryItem.Quantity for that product.
    /// This runs in-memory combining DB and local unsaved changes, so it can be called BEFORE SaveChangesAsync().
    /// </summary>
    public static async Task SyncProductStockAsync(Guid productId, IApplicationDbContext context, CancellationToken cancellationToken)
    {
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null) return;

        var dbItems = await context.InventoryItems
            .Where(i => i.ProductId == productId)
            .ToListAsync(cancellationToken);

        var addedItems = context.InventoryItems.Local
            .Where(i => i.ProductId == productId && context.Entry(i).State == EntityState.Added)
            .ToList();

        var allItems = dbItems.Concat(addedItems).Distinct().ToList();

        product.UpdateStock(allItems.Sum(i => i.Quantity));
        context.Products.Update(product);
    }
}
