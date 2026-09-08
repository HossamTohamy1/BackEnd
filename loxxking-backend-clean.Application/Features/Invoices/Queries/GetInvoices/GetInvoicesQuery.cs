namespace loxxking_backend_clean.Application.Features.Invoices.Queries.GetInvoices;

public record GetInvoicesQuery() : IRequest<Result<List<GetInvoicesResponse>>>;

public record GetInvoicesResponse(
    Guid Id,
    string InvoiceNumber,
    decimal TotalAmount,
    DateTime IssuedAt,
    InvoiceOrderDto Order
);

public record InvoiceOrderDto(Guid Id, string OrderNumber, string CustomerName, string Country);
