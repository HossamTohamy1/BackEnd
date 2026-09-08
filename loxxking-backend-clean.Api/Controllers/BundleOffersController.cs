using loxxking_backend_clean.Application.Features.BundleOffers.Commands.CreateBundleOffer;
using loxxking_backend_clean.Application.Features.BundleOffers.Commands.DeleteBundleOffer;
using loxxking_backend_clean.Application.Features.BundleOffers.Commands.UpdateBundleOffer;
using loxxking_backend_clean.Application.Features.BundleOffers.Queries.GetBundleOfferById;
using loxxking_backend_clean.Application.Features.BundleOffers.Queries.GetBundleOffers;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/bundle-offers")]
public class BundleOffersController : ControllerBase
{
    private readonly ISender _sender;
    public BundleOffersController(ISender sender) { _sender = sender; }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly, CancellationToken ct) => 
        (await _sender.Send(new GetBundleOffersQuery(activeOnly), ct)).ToApiResponse();

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) => 
        (await _sender.Send(new GetBundleOfferByIdQuery(id), ct)).ToApiResponse();

    [HttpPost]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> Create([FromBody] CreateBundleOfferCommand cmd, CancellationToken ct) => 
        (await _sender.Send(cmd, ct)).ToApiResponse();

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBundleOfferCommand cmd, CancellationToken ct) => 
        (await _sender.Send(cmd with { Id = id }, ct)).ToApiResponse();

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) => 
        (await _sender.Send(new DeleteBundleOfferCommand(id), ct)).ToApiResponse();
}
