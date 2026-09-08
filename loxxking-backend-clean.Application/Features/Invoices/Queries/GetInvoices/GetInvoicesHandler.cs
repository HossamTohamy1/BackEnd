namespace loxxking_backend_clean.Application.Features.Invoices.Queries.GetInvoices;

public class GetInvoicesHandler : IRequestHandler<GetInvoicesQuery, Result<List<GetInvoicesResponse>>>
{
    private readonly IApplicationDbContext _context;

    public GetInvoicesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<GetInvoicesResponse>>> Handle(GetInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await _context.Invoices
            .OrderByDescending(i => i.IssuedAt)
            .Select(i => new GetInvoicesResponse(
                i.Id,
                i.InvoiceNumber,
                i.TotalAmount.Value,
                i.IssuedAt,
                new InvoiceOrderDto(
                    i.Order.Id,
                    i.Order.OrderNumber,
                    i.Order.Customer != null ? i.Order.Customer.Name : "Guest",
                    i.Order.Country.Name
                )
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(invoices);
    }
}
