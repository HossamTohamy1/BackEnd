using loxxking_backend_clean.Domain.Entities.Products;
namespace loxxking_backend_clean.Domain.Entities.Inventory;

public class InventoryItem : BaseEntity {
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid CountryId { get; private set; }
    public loxxking_backend_clean.Domain.Entities.Countries.Country Country { get; private set; } = null!;
    public int Quantity { get; private set; }
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] RowVersion { get; private set; } = null!;

    protected InventoryItem() { }

    private InventoryItem(Guid productId, Guid countryId, int initialQuantity)
    {
        ProductId = productId;
        CountryId = countryId;
        Quantity = initialQuantity;
    }

    public static InventoryItem Create(Guid productId, Guid countryId, int initialQuantity)
    {
        if (productId == Guid.Empty) throw new ArgumentException("Domain_Inventory_ProductIdRequired", nameof(productId));
        if (countryId == Guid.Empty) throw new ArgumentException("Domain_Inventory_CountryIdRequired", nameof(countryId));
        if (initialQuantity < 0) throw new ArgumentException("Domain_Inventory_InitialQuantityNegative", nameof(initialQuantity));

        return new InventoryItem(productId, countryId, initialQuantity);
    }

    public void AddStock(int amount)
    {
        if (amount < 0) throw new ArgumentException("Domain_Inventory_AmountToAddNegative", nameof(amount));
        Quantity += amount;
    }

    public void RemoveStock(int amount)
    {
        if (amount < 0) throw new ArgumentException("Domain_Inventory_AmountToRemoveNegative", nameof(amount));
        if (Quantity - amount < 0) throw new InvalidOperationException("Domain_Inventory_CannotRemoveStock");
        
        Quantity -= amount;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity < 0) throw new ArgumentException("Domain_Inventory_QuantityNegative", nameof(newQuantity));
        Quantity = newQuantity;
    }
}
