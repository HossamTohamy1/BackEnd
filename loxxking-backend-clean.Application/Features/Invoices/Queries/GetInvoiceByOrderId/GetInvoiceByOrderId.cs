namespace loxxking_backend_clean.Application.Features.Invoices.Queries.GetInvoiceByOrderId;

public record GetInvoiceByOrderIdQuery(Guid OrderId) : IRequest<Result<object>>;

public class GetInvoiceByOrderIdHandler : IRequestHandler<GetInvoiceByOrderIdQuery, Result<object>>
{
    private readonly IApplicationDbContext _context;
    public GetInvoiceByOrderIdHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result<object>> Handle(GetInvoiceByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices.Where(i => i.OrderId == request.OrderId).Select(i => new { i.Id, i.InvoiceNumber, i.TotalAmount, i.IssuedAt }).FirstOrDefaultAsync(cancellationToken);
        if (invoice == null) return Result.Failure<object>(new Error("Error.NotFound", "Invoice_NotFound"));
        return Result.Success<object>(invoice);
    }
}
