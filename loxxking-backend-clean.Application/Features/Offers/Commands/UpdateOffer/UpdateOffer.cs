namespace loxxking_backend_clean.Application.Features.Offers.Commands.UpdateOffer;

public record UpdateOfferCommand(Guid Id, decimal DiscountPercent, DateTime StartDate, DateTime EndDate) : IRequest<Result>;

public class UpdateOfferHandler : IRequestHandler<UpdateOfferCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public UpdateOfferHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result> Handle(UpdateOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _context.Offers.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (offer == null) return Result.Failure(new Error("Error.NotFound", "Offer_NotFound"));
        offer.UpdateDiscount(loxxking_backend_clean.Domain.ValueObjects.Percentage.FromDecimal(request.DiscountPercent));
        offer.UpdatePeriod(loxxking_backend_clean.Domain.ValueObjects.DateRange.Create(request.StartDate, request.EndDate));
        _context.Offers.Update(offer);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
