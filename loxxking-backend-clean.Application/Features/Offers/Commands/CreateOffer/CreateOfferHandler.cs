using loxxking_backend_clean.Domain.Entities.Offers;
using Microsoft.Extensions.Caching.Distributed;

namespace loxxking_backend_clean.Application.Features.Offers.Commands.CreateOffer;

public class CreateOfferHandler : IRequestHandler<CreateOfferCommand, Result<CreateOfferResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    private const string ActiveOffersCacheKey = "offers:active";

    public CreateOfferHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<CreateOfferResponse>> Handle(CreateOfferCommand request, CancellationToken cancellationToken)
    {
        var productExists = await _context.Products.AnyAsync(p => p.Id == request.ProductId, cancellationToken);
        if (!productExists)
        {
            return Result.Failure<CreateOfferResponse>(new Error("Error.Validation", "Offer_InvalidProduct"));
        }

        var offer = Offer.Create(
            request.ProductId,
            loxxking_backend_clean.Domain.ValueObjects.Percentage.FromDecimal(request.DiscountPercent),
            loxxking_backend_clean.Domain.ValueObjects.DateRange.Create(request.StartDate, request.EndDate)
        );

        _context.Offers.Add(offer);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(ActiveOffersCacheKey, cancellationToken);

        return Result.Success(new CreateOfferResponse(offer.Id));
    }
}
