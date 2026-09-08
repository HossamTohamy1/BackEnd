using loxxking_backend_clean.Application.Features.Invoices.Queries.GetInvoices;

namespace loxxking_backend_clean.Application.Features.Invoices.Queries.GetInvoiceById;

public class GetInvoiceByIdHandler : IRequestHandler<GetInvoiceByIdQuery, Result<GetInvoiceByIdResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetInvoiceByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetInvoiceByIdResponse>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Where(i => i.Id == request.Id)
            .Select(i => new
            {
                i.Id,
                i.InvoiceNumber,
                i.TotalAmount,
                i.IssuedAt,
                CustomerId = i.Order.CustomerId,
                Order = new InvoiceOrderDto(
                    i.Order.Id,
                    i.Order.OrderNumber,
                    i.Order.Customer != null ? i.Order.Customer.Name : "Guest",
                    i.Order.Country.Name
                )
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (invoice is null)
        {
            return Result.Failure<GetInvoiceByIdResponse>(new Error("Error.NotFound", "Invoice_NotFound"));
        }

        if (!request.IsStaff && invoice.CustomerId != request.UserId)
        {
            return Result.Failure<GetInvoiceByIdResponse>(new Error("Error.Unauthorized", "Auth_UnauthorizedAccess"));
        }

        return Result.Success(new GetInvoiceByIdResponse(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.TotalAmount.Value,
            invoice.IssuedAt,
            invoice.Order
        ));
    }
}
