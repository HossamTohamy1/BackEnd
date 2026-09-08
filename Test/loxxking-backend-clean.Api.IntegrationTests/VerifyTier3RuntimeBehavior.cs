using loxxking_backend_clean.Application.Features.Invoices.Commands.CreateInvoice;
using loxxking_backend_clean.Domain.Entities.Invoices;
using loxxking_backend_clean.Domain.ValueObjects;
using loxxking_backend_clean.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace loxxking_backend_clean.Api.IntegrationTests;

public class VerifyTier3RuntimeBehavior
{
    private ApplicationDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        context.Database.ExecuteSqlRaw("PRAGMA foreign_keys=OFF;");
        return context;
    }

    [Fact]
    public void Verify_Invoice_Create_And_Validation()
    {
        var orderId = Guid.NewGuid();
        var invoiceNumber = "INV-123456";
        var amount = Money.FromDecimal(100.50m);
        
        var invoice = Invoice.Create(orderId, invoiceNumber, amount);
        
        Assert.NotNull(invoice);
        Assert.Equal(orderId, invoice.OrderId);
        Assert.Equal(invoiceNumber, invoice.InvoiceNumber);
        Assert.Equal(100.50m, invoice.TotalAmount.Value);
        Assert.True((DateTime.UtcNow - invoice.IssuedAt).TotalSeconds < 5);
        
        Assert.Throws<ArgumentException>(() => Invoice.Create(Guid.Empty, invoiceNumber, amount));
        Assert.Throws<ArgumentException>(() => Invoice.Create(orderId, "", amount));
        Assert.Throws<ArgumentNullException>(() => Invoice.Create(orderId, invoiceNumber, null!));
    }

    [Fact]
    public async Task Verify_CreateInvoiceHandler_Creates_Valid_Invoice()
    {
        using var db = GetDbContext();
        var order = loxxking_backend_clean.Domain.Entities.Orders.Order.Create(
            null,
            Guid.NewGuid(),
            "John Doe",
            "john@example.com",
            "+1234567890",
            "123 Test St",
            loxxking_backend_clean.Domain.Enums.PaymentMethod.BankTransfer,
            null,
            null,
            null
        );
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var handler = new CreateInvoiceHandler(db);
        var command = new CreateInvoiceCommand(order.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var createdInvoice = await db.Invoices.FirstAsync();
        Assert.Equal(order.Id, createdInvoice.OrderId);
        Assert.Equal(0m, createdInvoice.TotalAmount.Value);
        Assert.StartsWith("INV-", createdInvoice.InvoiceNumber);
        Assert.True((DateTime.UtcNow - createdInvoice.IssuedAt).TotalSeconds < 5);
    }
}
