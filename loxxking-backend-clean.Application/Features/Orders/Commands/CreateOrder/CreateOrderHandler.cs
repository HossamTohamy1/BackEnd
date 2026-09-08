using loxxking_backend_clean.Domain.Entities.Orders;
using loxxking_backend_clean.Domain.Entities.Invoices;
using loxxking_backend_clean.Domain.Entities.Inventory;

namespace loxxking_backend_clean.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    private static readonly SemaphoreSlim _orderNumberLock = new(1, 1);
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IInvoicePdfGenerator _pdfGenerator;
    private readonly IOrderNotificationService _notificationService;
    private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _cache;

    public CreateOrderHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IInvoicePdfGenerator pdfGenerator,
        IOrderNotificationService notificationService,
        Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _pdfGenerator = pdfGenerator;
        _notificationService = notificationService;
        _cache = cache;
    }

    public async Task<Result<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
            return Result.Failure<CreateOrderResponse>(new Error("Error.Validation", "Order_EmptyItems"));

        Guid? finalCountryId = null;

        var currentUserId = _currentUserService.UserId;

        if (request.CountryId.HasValue && request.CountryId.Value != Guid.Empty)
        {
            finalCountryId = request.CountryId.Value;
        }
        else if (currentUserId != Guid.Empty)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId, cancellationToken);
            finalCountryId = user?.CountryId;
        }

        if (finalCountryId is null)
        {
            var geoCountryName = _currentUserService.GeoCountryName ?? request.GuestCountryName;
            if (!string.IsNullOrWhiteSpace(geoCountryName))
            {
                var geoCountry = await _context.Countries
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == geoCountryName.ToLower(), cancellationToken);
                finalCountryId = geoCountry?.Id;
            }
        }

        if (finalCountryId is null)
        {
            var fallback = await _context.Countries
                .OrderByDescending(c => c.IsDefault)
                .ThenBy(c => c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
            finalCountryId = fallback?.Id;
        }

        if (finalCountryId is null)
            return Result.Failure<CreateOrderResponse>(new Error("Error.Validation", "Order_CountryRequired"));

        var countryEntity = await _context.Countries.FindAsync(new object?[] { finalCountryId.Value }, cancellationToken: cancellationToken);
        var resolvedCountryName = countryEntity?.Name ?? "—";

        Order order;
        Invoice invoice;
        var notificationItems = new List<OrderNotificationItem>();

        await _orderNumberLock.WaitAsync(cancellationToken);
        try
        {
            var year = DateTime.UtcNow.Year;
            var orderNumber = await GenerateOrderNumberAsync(year, cancellationToken);

            order = Order.Create(
                currentUserId != Guid.Empty ? currentUserId : null,
                finalCountryId.Value,
                orderNumber,
                request.Address,
                request.Phone,
                request.Notes,
                request.PaymentMethod,
                request.GuestName,
                null, // guestPhone
                null  // guestAddress
            );

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            foreach (var itemDto in request.Items)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == itemDto.ProductId, cancellationToken);
                if (product == null) continue;

                var price = await ApplyDynamicPricingAsync(product.Id, finalCountryId.Value, product.BasePrice.Value, cancellationToken);
                
                var inventoryResult = await DecrementInventoryAsync(product.Id, finalCountryId.Value, itemDto.Quantity, product.NameEn, cancellationToken);
                if (inventoryResult.IsFailure) return Result.Failure<CreateOrderResponse>(inventoryResult.Error);

                order.AddItem(product.Id, itemDto.Quantity, loxxking_backend_clean.Domain.ValueObjects.Money.FromDecimal(price));

                notificationItems.Add(new OrderNotificationItem(product.NameEn, itemDto.Quantity, price));
            }

            invoice = Invoice.Create(
                order.Id,
                $"INV-{order.OrderNumber}",
                order.TotalAmount
            );

            _context.Orders.Add(order);
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            _orderNumberLock.Release();
        }

        foreach (var itemDto in request.Items)
        {
            await _cache.RemoveAsync($"ProductDetail_{itemDto.ProductId}_ar", cancellationToken);
            await _cache.RemoveAsync($"ProductDetail_{itemDto.ProductId}_en", cancellationToken);
        }

        await ProcessInvoicesAndNotificationsAsync(order, invoice, request, currentUserId, resolvedCountryName, notificationItems, cancellationToken);

        return Result.Success(new CreateOrderResponse(order.Id, order.OrderNumber, order.TotalAmount.Value, finalCountryId.Value));
    }

    private async Task<decimal> ApplyDynamicPricingAsync(Guid productId, Guid countryId, decimal basePrice, CancellationToken cancellationToken)
    {
        var productPrice = await _context.ProductPrices
            .Where(p => p.ProductId == productId && p.CountryId == countryId)
            .Select(p => (decimal?)p.Price)
            .FirstOrDefaultAsync(cancellationToken);
        
        productPrice ??= await _context.ProductPrices
            .Where(p => p.ProductId == productId)
            .Select(p => (decimal?)p.Price)
            .FirstOrDefaultAsync(cancellationToken);

        return productPrice ?? basePrice;
    }

    private async Task<Result> DecrementInventoryAsync(Guid productId, Guid countryId, int quantity, string productName, CancellationToken cancellationToken)
    {
        for (int i = 0; i < 3; i++)
        {
            InventoryItem? inventory = null;
            var savepointName = $"inv_{i}_{productId:N}"[..30];
            try
            {
                var currentTransaction = _context.Database.CurrentTransaction;
                if (currentTransaction != null)
                {
                    await currentTransaction.CreateSavepointAsync(savepointName, cancellationToken);
                }

                inventory = await _context.InventoryItems
                    .FirstOrDefaultAsync(inv => inv.ProductId == productId && inv.CountryId == countryId, cancellationToken);
                
                inventory ??= await _context.InventoryItems
                    .FirstOrDefaultAsync(inv => inv.ProductId == productId, cancellationToken);

                if (inventory == null || inventory.Quantity < quantity)
                    return Result.Failure(new Error("Error.Validation", $"Insufficient inventory for product {productName}"));

                inventory.RemoveStock(quantity);
                _context.InventoryItems.Update(inventory);
                
                await loxxking_backend_clean.Application.Features.Inventories.Helpers.InventorySyncHelper.SyncProductStockAsync(productId, _context, cancellationToken);
                
                await _context.SaveChangesAsync(cancellationToken);
                
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                var currentTransaction = _context.Database.CurrentTransaction;
                
                if (currentTransaction != null)
                {
                    await currentTransaction.RollbackToSavepointAsync(savepointName, cancellationToken);
                }

                if (i == 2)
                {
                    return Result.Failure(new Error("Error.Concurrency", "Order_ConcurrencyRetry"));
                }
                
                if (inventory != null)
                {
                    _context.Entry(inventory).State = EntityState.Detached;
                }
            }
        }
        
        return Result.Failure(new Error("Error.Concurrency", "Order_ConcurrencyRetry"));
    }

    private async Task ProcessInvoicesAndNotificationsAsync(Order order, Invoice invoice, CreateOrderCommand request, Guid currentUserId, string? resolvedCountryName, List<OrderNotificationItem> notificationItems, CancellationToken cancellationToken)
    {
        byte[]? pdfAttachment = null;
        try
        {
            pdfAttachment = await _pdfGenerator.GeneratePdfAsync(invoice, cancellationToken);
        }
        catch (Exception)
        {
        }

        var customerUser = currentUserId != Guid.Empty 
            ? await _context.Users.FindAsync(new object?[] { currentUserId }, cancellationToken: cancellationToken)
            : null;
        var customerName = customerUser?.Name ?? request.GuestName ?? "Customer";
        var customerLang = customerUser?.PreferredLanguage ?? (System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName.StartsWith("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en");

        var notifData = new OrderNotificationData(
            order.OrderNumber,
            customerName,
            order.Phone ?? "",
            order.Address ?? "",
            resolvedCountryName ?? "",
            order.PaymentMethod.ToString(),
            order.TotalAmount.Value,
            notificationItems,
            order.CreatedAt,
            pdfAttachment,
            customerLang
        );

        _ = _notificationService.NotifyNewOrderAsync(notifData, CancellationToken.None)
            .ContinueWith(t => 
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    Console.WriteLine($"[CRITICAL] Order notification background task failed for order {order.OrderNumber}: {t.Exception.Flatten().Message}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private async Task<string> GenerateOrderNumberAsync(int year, CancellationToken cancellationToken)
    {
        var prefix = $"ORD-{year}-";

        var candidates = await _context.Orders
            .IgnoreQueryFilters()
            .Where(o => o.OrderNumber != null && o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber.Length)
            .ThenByDescending(o => o.OrderNumber)
            .Select(o => o.OrderNumber)
            .Take(50)
            .ToListAsync(cancellationToken);

        int maxSequence = 0;
        foreach (var candidate in candidates)
        {
            if (candidate.Length > prefix.Length)
            {
                var suffix = candidate.Substring(prefix.Length);
                if (int.TryParse(suffix, out var seq) && seq > maxSequence)
                {
                    maxSequence = seq;
                }
            }
        }

        var nextSequence = maxSequence + 1;
        var orderNumber = $"{prefix}{nextSequence:D4}";

        while (await _context.Orders.IgnoreQueryFilters().AnyAsync(o => o.OrderNumber == orderNumber, cancellationToken))
        {
            nextSequence++;
            orderNumber = $"{prefix}{nextSequence:D4}";
        }

        return orderNumber;
    }
}
