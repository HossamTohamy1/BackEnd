namespace loxxking_backend_clean.Application.Features.HomePageConfig.Queries.GetHomePageConfig;

public record HomePageConfigResponse(
    string SectionsJson,
    int Version,
    DateTime? UpdatedAt
);

public record GetHomePageConfigQuery() : IRequest<Result<HomePageConfigResponse>>;
