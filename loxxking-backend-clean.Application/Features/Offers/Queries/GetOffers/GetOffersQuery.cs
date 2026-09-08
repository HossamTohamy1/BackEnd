using loxxking_backend_clean.Application.Features.Products.Queries.GetProducts;

namespace loxxking_backend_clean.Application.Features.Offers.Queries.GetOffers;

public record GetOffersQuery(bool ActiveOnly) : IRequest<Result<List<ProductListResponse>>>;
