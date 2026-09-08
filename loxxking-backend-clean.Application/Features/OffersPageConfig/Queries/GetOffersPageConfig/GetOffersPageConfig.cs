namespace loxxking_backend_clean.Application.Features.OffersPageConfig.Queries.GetOffersPageConfig;

public record OffersPageConfigResponse(
    string HeroTitle,
    string HeroSubtitle,
    bool ShowHero,
    string CurrentOffersTitle,
    string BundlesTitle,
    string BundlesSubtitle
);

public record GetOffersPageConfigQuery() : IRequest<Result<OffersPageConfigResponse>>;
