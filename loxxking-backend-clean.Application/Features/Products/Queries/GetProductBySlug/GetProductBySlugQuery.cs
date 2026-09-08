using loxxking_backend_clean.Application.Features.Products.Queries.GetProduct;

namespace loxxking_backend_clean.Application.Features.Products.Queries.GetProductBySlug;

public record GetProductBySlugQuery(string Slug) : IRequest<Result<GetProductResponse>>;
