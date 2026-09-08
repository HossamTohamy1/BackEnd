namespace loxxking_backend_clean.Application.Features.Products.Queries.GetProduct;

public record GetProductQuery(Guid Id) : IRequest<Result<GetProductResponse>>;
