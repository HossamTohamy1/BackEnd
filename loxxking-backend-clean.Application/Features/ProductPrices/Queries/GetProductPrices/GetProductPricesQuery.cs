namespace loxxking_backend_clean.Application.Features.ProductPrices.Queries.GetProductPrices;

public record GetProductPricesQuery(Guid ProductId) : IRequest<Result<List<GetProductPricesResponse>>>;

public record GetProductPricesResponse(Guid CountryId, string CountryName, decimal Price);
