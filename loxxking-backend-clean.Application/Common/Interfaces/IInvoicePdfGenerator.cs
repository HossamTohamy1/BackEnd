using loxxking_backend_clean.Domain.Entities.Invoices;

namespace loxxking_backend_clean.Application.Common.Interfaces;

public interface IInvoicePdfGenerator
{
    Task<byte[]> GeneratePdfAsync(Invoice invoice, CancellationToken cancellationToken);
}
