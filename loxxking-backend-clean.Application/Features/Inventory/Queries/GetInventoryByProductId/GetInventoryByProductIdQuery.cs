namespace loxxking_backend_clean.Application.Features.Inventory.Queries.GetInventoryByProductId;

public record GetInventoryByProductIdQuery(Guid ProductId) : IRequest<Result<GetInventoryByProductIdResponse>>;

public record GetInventoryByProductIdResponse(Guid ProductId, int StockQuantity);
