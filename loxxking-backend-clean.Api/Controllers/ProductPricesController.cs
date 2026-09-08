using loxxking_backend_clean.Application.Features.ProductPrices.Commands.SetProductPrice;
using loxxking_backend_clean.Application.Features.ProductPrices.Commands.UpsertProductPrice;
using loxxking_backend_clean.Application.Features.ProductPrices.Queries.GetProductPriceByCountry;
using loxxking_backend_clean.Application.Features.ProductPrices.Queries.GetProductPrices;
using loxxking_backend_clean.Application.Features.ProductPrices.Queries.GetProductsWithPricesByCountry;

namespace loxxking_backend_clean.Api.Controllers;

[ApiController]
[Route("api/product-prices")]
public class ProductPricesController : ControllerBase
{
    private readonly ISender _sender;
    public ProductPricesController(ISender sender) { _sender = sender; }

    [HttpGet("{productId}/prices")]
    public async Task<IActionResult> GetPrices(Guid productId, CancellationToken ct) => (await _sender.Send(new GetProductPricesQuery(productId), ct)).ToApiResponse();

    [HttpPut("{productId}/prices")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> UpsertPrice(Guid productId, [FromBody] UpsertProductPriceCommand cmd, CancellationToken ct) => (await _sender.Send(cmd with { ProductId = productId }, ct)).ToApiResponse();

    [HttpGet("price/{countryId}/{productId}")]
    public async Task<IActionResult> GetProductPriceByCountry(Guid productId, Guid countryId, CancellationToken ct) => (await _sender.Send(new GetProductPriceByCountryQuery(productId, countryId), ct)).ToApiResponse();

    [HttpPut("price/{countryId}/{productId}")]
    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> SetPrice(Guid productId, Guid countryId, [FromBody] SetProductPriceCommand cmd, CancellationToken ct) => (await _sender.Send(cmd with { ProductId = productId, CountryId = countryId }, ct)).ToApiResponse();

    [HttpGet("prices-by-country")]
    public async Task<IActionResult> GetProductsWithPricesByCountry([FromQuery] Guid? countryId, CancellationToken ct) => (await _sender.Send(new GetProductsWithPricesByCountryQuery(countryId), ct)).ToApiResponse();
}
