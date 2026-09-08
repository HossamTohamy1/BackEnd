using loxxking_backend_clean.Application.Features.Products.Queries.GetProducts;

namespace loxxking_backend_clean.Application.Features.BundleOffers.Queries.GetBundleOffers;

public record GetBundleOffersQuery(bool ActiveOnly) : IRequest<Result<List<BundleOfferResponse>>>;

public record BundleOfferItemResponse(
    ProductListResponse Product,
    int Quantity
);

public record BundleOfferResponse(
    Guid Id,
    string Title,
    string Subtitle,
    decimal BundlePrice,
    decimal OriginalPrice,
    decimal Saving,
    decimal DiscountPercent,
    string ImageUrl,
    DateTime StartDate,
    DateTime EndDate,
    List<BundleOfferItemResponse> Items
);
