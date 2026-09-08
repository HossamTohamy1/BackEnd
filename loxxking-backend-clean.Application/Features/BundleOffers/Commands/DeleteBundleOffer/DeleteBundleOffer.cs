using Microsoft.Extensions.Caching.Distributed;

namespace loxxking_backend_clean.Application.Features.BundleOffers.Commands.DeleteBundleOffer;

public record DeleteBundleOfferCommand(Guid Id) : IRequest<Result>;

public class DeleteBundleOfferHandler : IRequestHandler<DeleteBundleOfferCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public DeleteBundleOfferHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result> Handle(DeleteBundleOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _context.BundleOffers
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (offer == null) return Result.Failure(new Error("Error.NotFound", "BundleOffer_NotFound"));

        _context.BundleOffers.Remove(offer);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync("BundleOffers_ar_True", cancellationToken);
        await _cache.RemoveAsync("BundleOffers_ar_False", cancellationToken);
        await _cache.RemoveAsync("BundleOffers_en_True", cancellationToken);
        await _cache.RemoveAsync("BundleOffers_en_False", cancellationToken);

        return Result.Success();
    }
}
