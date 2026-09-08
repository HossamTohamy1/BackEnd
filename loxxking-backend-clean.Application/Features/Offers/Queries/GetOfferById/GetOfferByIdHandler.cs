namespace loxxking_backend_clean.Application.Features.Offers.Queries.GetOfferById;

public class GetOfferByIdHandler : IRequestHandler<GetOfferByIdQuery, Result<GetOfferByIdResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetOfferByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<GetOfferByIdResponse>> Handle(GetOfferByIdQuery request, CancellationToken cancellationToken)
    {
        var offer = await _context.Offers
            .Where(o => o.Id == request.Id)
            .Select(o => new GetOfferByIdResponse(
                o.Id,
                o.ProductId,
                o.Product.NameEn,
                o.Discount.Value,
                o.ActivePeriod.StartDate,
                o.ActivePeriod.EndDate
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (offer is null)
        {
            return Result.Failure<GetOfferByIdResponse>(new Error("Error.NotFound", "Offer_NotFound"));
        }

        return Result.Success(offer);
    }
}
