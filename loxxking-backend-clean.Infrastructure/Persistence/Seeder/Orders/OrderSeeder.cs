using loxxking_backend_clean.Domain.Entities.BankTransfers;
using loxxking_backend_clean.Domain.Entities.Invoices;
using loxxking_backend_clean.Domain.Entities.Orders;
using loxxking_backend_clean.Domain.Enums;
using loxxking_backend_clean.Infrastructure.Persistence.Seeder.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace loxxking_backend_clean.Infrastructure.Persistence.Seeder.Orders;

public class OrderSeeder : IDataSeeder
{
    public int Order => 8;

    public async Task SeedAsync(SeedContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = serviceProvider.GetRequiredService<ILogger<OrderSeeder>>();

        var customer = context.CustomerUser;
        var admin = context.AdminUser;
        var defaultCountry = context.DefaultCountry;

        var shirt = context.Products.FirstOrDefault(p => p.Slug == "oxford-cotton-shirt");
        var watch = context.Products.FirstOrDefault(p => p.Slug == "minimalist-chronograph-watch");

        if (shirt == null || watch == null)
        {
            logger.LogWarning("Products not found for order seeding. Skipping OrderSeeder.");
            return;
        }

        // Order 1: Completed CashOnDelivery with Invoice and Audit Log
        const string order1Number = "ORD-2026-0001";
        var order1 = await dbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == order1Number, cancellationToken);
        if (order1 == null)
        {
            order1 = Domain.Entities.Orders.Order.Create(
                customerId: customer.Id,
                countryId: defaultCountry.Id,
                orderNumber: order1Number,
                address: "15 Tahrir Square, Downtown, Cairo",
                phone: "+201000000004",
                notes: "الرجاء الاتصال قبل التوصيل بنصف ساعة",
                paymentMethod: PaymentMethod.CashOnDelivery);

            order1.AddItem(shirt.Id, 2, shirt.BasePrice);
            order1.ChangeStatus(OrderStatus.Delivered);
            order1.ChangePaymentStatus(PaymentStatus.Paid);
            order1.UpdateDetails("+201000000004", "15 Tahrir Square, Downtown, Cairo", "SHP-2026-0001");

            await dbContext.Orders.AddAsync(order1, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded completed order '{OrderNumber}'.", order1Number);
        }

        // Seed Invoice for Order 1
        const string invNumber = "INV-2026-0001";
        var invExists = await dbContext.Invoices.AnyAsync(i => i.InvoiceNumber == invNumber, cancellationToken);
        if (!invExists)
        {
            var invoice = Invoice.Create(order1.Id, invNumber, order1.TotalAmount);
            await dbContext.Invoices.AddAsync(invoice, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded Invoice '{InvoiceNumber}' for order '{OrderNumber}'.", invNumber, order1Number);
        }

        // Seed OrderEditLog for Order 1
        var editLogExists = await dbContext.OrderEditLogs.AnyAsync(l => l.OrderId == order1.Id, cancellationToken);
        if (!editLogExists)
        {
            var editLog = new OrderEditLog
            {
                OrderId = order1.Id,
                EditedBy = admin.Id,
                Editor = admin,
                FieldName = "Status",
                OldValue = "Processing",
                NewValue = "Delivered",
                EditedAt = DateTime.UtcNow
            };
            await dbContext.OrderEditLogs.AddAsync(editLog, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded OrderEditLog for order '{OrderNumber}'.", order1Number);
        }

        // Order 2: BankTransfer under review with BankTransfer receipt
        const string order2Number = "ORD-2026-0002";
        var order2 = await dbContext.Orders.FirstOrDefaultAsync(o => o.OrderNumber == order2Number, cancellationToken);
        if (order2 == null)
        {
            order2 = Domain.Entities.Orders.Order.Create(
                customerId: customer.Id,
                countryId: defaultCountry.Id,
                orderNumber: order2Number,
                address: "45 El-Nozha St, Heliopolis, Cairo",
                phone: "+201000000004",
                notes: "تم تحويل المبلغ عبر البنك الأهلي المصري",
                paymentMethod: PaymentMethod.BankTransfer);

            order2.AddItem(watch.Id, 1, watch.BasePrice);
            order2.UpdateDetails("+201000000004", "45 El-Nozha St, Heliopolis, Cairo", "SHP-2026-0002");

            await dbContext.Orders.AddAsync(order2, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded BankTransfer order '{OrderNumber}'.", order2Number);
        }

        // Seed BankTransfer for Order 2
        var transferExists = await dbContext.BankTransfers.AnyAsync(bt => bt.OrderId == order2.Id, cancellationToken);
        if (!transferExists)
        {
            var bankTransfer = BankTransfer.Create(order2.Id, "/assets/dashboard/sample-receipt.jpg");
            await dbContext.BankTransfers.AddAsync(bankTransfer, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded BankTransfer receipt for order '{OrderNumber}'.", order2Number);
        }
    }
}
