using loxxking_backend_clean.Application.Features.BundleOffers.Queries.GetBundleOffers;
using loxxking_backend_clean.Application.Features.Products.Queries.GetProducts;

namespace loxxking_backend_clean.Application.Features.BundleOffers.Queries.GetBundleOfferById;

public record GetBundleOfferByIdQuery(Guid Id) : IRequest<Result<BundleOfferResponse>>;

public class GetBundleOfferByIdHandler : IRequestHandler<GetBundleOfferByIdQuery, Result<BundleOfferResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetBundleOfferByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<BundleOfferResponse>> Handle(GetBundleOfferByIdQuery request, CancellationToken cancellationToken)
    {
        var b = await _context.BundleOffers
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (b == null) return Result.Failure<BundleOfferResponse>(new Error("Error.NotFound", "BundleOffer_NotFound"));

        var items = b.Items.Select(i => new BundleOfferItemResponse(
            new ProductListResponse(
                i.Product.Id,
                i.Product.Slug,
                i.Product.NameEn,
                i.Product.NameAr,
                i.Product.Description,
                i.Product.Description,
                i.Product.BasePrice.Value, 
                i.Product.OriginalPrice != null ? i.Product.OriginalPrice.Value : null,
                i.Product.Images,
                i.Product.Category.NameEn,
                i.Product.Sizes,
                i.Product.SizeChartJson,
                i.Product.Stock,
                i.Product.Rating,
                i.Product.ReviewCount,
                i.Product.IsNew,
                i.Product.IsBestSeller,
                null, 
                i.Product.Colors
            ),
            i.Quantity
        )).ToList();

        decimal originalPrice = items.Sum(i => i.Product.Price * i.Quantity);
        decimal saving = originalPrice - b.BundlePrice.Value;
        decimal discountPercent = originalPrice > 0 ? (saving / originalPrice) * 100 : 0;

        var response = new BundleOfferResponse(
            b.Id,
            b.Title,
            b.Subtitle,
            b.BundlePrice.Value,
            originalPrice,
            saving,
            Math.Round(discountPercent, 2),
            b.ImageUrl,
            b.ActivePeriod.StartDate,
            b.ActivePeriod.EndDate,
            items
        );

        return Result.Success(response);
    }
}
