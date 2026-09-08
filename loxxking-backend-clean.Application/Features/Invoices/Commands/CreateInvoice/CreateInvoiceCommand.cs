namespace loxxking_backend_clean.Application.Features.Invoices.Commands.CreateInvoice;

public record CreateInvoiceCommand(Guid OrderId) : IRequest<Result<CreateInvoiceResponse>>;

public record CreateInvoiceResponse(
    Guid Id,
    string InvoiceNumber,
    decimal TotalAmount,
    DateTime IssuedAt,
    bool AlreadyExisted
);
