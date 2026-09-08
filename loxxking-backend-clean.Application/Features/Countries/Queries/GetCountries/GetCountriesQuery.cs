namespace loxxking_backend_clean.Application.Features.Countries.Queries.GetCountries;

public record GetCountriesQuery() : IRequest<Result<List<GetCountriesResponse>>>;

public record GetCountriesResponse(Guid Id, string Name, string Currency, string DefaultLanguage);
