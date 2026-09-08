namespace loxxking_backend_clean.Application.Features.Offers.Commands.DeleteOffer;

public record DeleteOfferCommand(Guid Id) : IRequest<Result>;

public class DeleteOfferHandler : IRequestHandler<DeleteOfferCommand, Result>
{
    private readonly IApplicationDbContext _context;
    public DeleteOfferHandler(IApplicationDbContext context) { _context = context; }

    public async Task<Result> Handle(DeleteOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _context.Offers.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (offer == null) return Result.Failure(new Error("Error.NotFound", "Offer_NotFound"));
        _context.Offers.Remove(offer);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
