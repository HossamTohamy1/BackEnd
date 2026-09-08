using loxxking_backend_clean.Domain.Entities.Invoices;

namespace loxxking_backend_clean.Application.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceHandler : IRequestHandler<CreateInvoiceCommand, Result<CreateInvoiceResponse>>
{
    private readonly IApplicationDbContext _context;

    public CreateInvoiceHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateInvoiceResponse>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result.Failure<CreateInvoiceResponse>(new Error("Error.NotFound", "Order_NotFound"));

        var existing = await _context.Invoices
            .FirstOrDefaultAsync(i => i.OrderId == request.OrderId, cancellationToken);

        if (existing is not null)
        {
            return Result.Success(new CreateInvoiceResponse(existing.Id, existing.InvoiceNumber, existing.TotalAmount.Value, existing.IssuedAt, true));
        }

        var invoice = Invoice.Create(
            request.OrderId,
            $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            order.TotalAmount
        );

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateInvoiceResponse(invoice.Id, invoice.InvoiceNumber, invoice.TotalAmount.Value, invoice.IssuedAt, false));
    }
}
