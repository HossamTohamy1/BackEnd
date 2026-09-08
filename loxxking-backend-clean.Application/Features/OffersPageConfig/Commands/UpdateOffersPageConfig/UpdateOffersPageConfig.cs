using Microsoft.Extensions.Caching.Distributed;

namespace loxxking_backend_clean.Application.Features.OffersPageConfig.Commands.UpdateOffersPageConfig;

public record UpdateOffersPageConfigCommand(
    string HeroTitle,
    string HeroSubtitle,
    bool ShowHero,
    string CurrentOffersTitle,
    string BundlesTitle,
    string BundlesSubtitle
) : IRequest<Result>;

public class UpdateOffersPageConfigHandler : IRequestHandler<UpdateOffersPageConfigCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public UpdateOffersPageConfigHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result> Handle(UpdateOffersPageConfigCommand request, CancellationToken cancellationToken)
    {
        var config = await _context.OffersPageConfigs.FirstOrDefaultAsync(cancellationToken);

        if (config == null)
        {
            config = new Domain.Entities.Offers.OffersPageConfig();
            _context.OffersPageConfigs.Add(config);
        }

        config.HeroTitle = request.HeroTitle;
        config.HeroSubtitle = request.HeroSubtitle;
        config.ShowHero = request.ShowHero;
        config.CurrentOffersTitle = request.CurrentOffersTitle;
        config.BundlesTitle = request.BundlesTitle;
        config.BundlesSubtitle = request.BundlesSubtitle;

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync("PageConfig_Offers_ar", cancellationToken);
        await _cache.RemoveAsync("PageConfig_Offers_en", cancellationToken);

        return Result.Success();
    }
}
