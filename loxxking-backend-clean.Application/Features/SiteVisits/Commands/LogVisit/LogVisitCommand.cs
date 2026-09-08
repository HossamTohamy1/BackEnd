namespace loxxking_backend_clean.Application.Features.SiteVisits.Commands.LogVisit;

public record LogVisitCommand(Guid CountryId, string Page) : IRequest<Result<LogVisitResponse>>;

public record LogVisitResponse(Guid Id, string Message);
