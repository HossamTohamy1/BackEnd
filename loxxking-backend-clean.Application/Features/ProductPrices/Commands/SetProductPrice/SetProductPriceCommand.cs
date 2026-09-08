namespace loxxking_backend_clean.Application.Features.ProductPrices.Commands.SetProductPrice;

public record SetProductPriceCommand(Guid ProductId, Guid CountryId, decimal Price) : IRequest<Result>;
