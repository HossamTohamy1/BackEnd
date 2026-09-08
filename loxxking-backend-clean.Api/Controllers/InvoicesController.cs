using loxxking_backend_clean.Application.Features.Invoices.Commands.CreateInvoice;
using loxxking_backend_clean.Application.Features.Invoices.Queries.GetInvoiceById;
using loxxking_backend_clean.Application.Features.Invoices.Queries.GetInvoiceByOrderId;
using loxxking_backend_clean.Application.Features.Invoices.Queries.GetInvoicePdf;
using loxxking_backend_clean.Application.Features.Invoices.Queries.GetInvoices;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly ISender _sender;
    public InvoicesController(ISender sender) { _sender = sender; }

    [HttpGet]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> GetAll(CancellationToken ct) => (await _sender.Send(new GetInvoicesQuery(), ct)).ToApiResponse();

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) => (await _sender.Send(new GetInvoiceByIdQuery(id, Guid.Empty, true), ct)).ToApiResponse();

    [HttpPost("{orderId}")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> Create(Guid orderId, CancellationToken ct) => (await _sender.Send(new CreateInvoiceCommand(orderId), ct)).ToApiResponse();

    [HttpGet("by-order/{orderId}")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> GetByOrderId(Guid orderId, CancellationToken ct) => (await _sender.Send(new GetInvoiceByOrderIdQuery(orderId), ct)).ToApiResponse();

    [HttpGet("{id}/pdf")]
[Authorize]
    public async Task<IActionResult> GetPdf(Guid id, CancellationToken ct) {
        var res = await _sender.Send(new GetInvoicePdfQuery(id), ct);
        if(!res.IsSuccess) return NotFound();
        return File(res.Value, "application/pdf", $"Invoice_{id}.pdf");
    }
}
