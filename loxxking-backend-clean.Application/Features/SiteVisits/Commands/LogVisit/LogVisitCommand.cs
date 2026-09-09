namespace loxxking_backend_clean.Application.Features.SiteVisits.Commands.LogVisit;

public record LogVisitCommand(Guid? CountryId, string Page, string? IpAddress = null) : IRequest<Result<LogVisitResponse>>;

public record LogVisitResponse(Guid Id, string Message);
