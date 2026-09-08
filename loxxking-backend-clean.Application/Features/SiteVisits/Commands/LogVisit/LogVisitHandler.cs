using loxxking_backend_clean.Domain.Entities.Notifications;
using loxxking_backend_clean.Domain.Entities.SiteVisits;


namespace loxxking_backend_clean.Application.Features.SiteVisits.Commands.LogVisit;

public class LogVisitHandler : IRequestHandler<LogVisitCommand, Result<LogVisitResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource>? _localizer;

    public LogVisitHandler(IApplicationDbContext context, IStringLocalizer<SharedResource>? localizer = null)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result<LogVisitResponse>> Handle(LogVisitCommand request, CancellationToken cancellationToken)
    {
        var country = await _context.Countries.FirstOrDefaultAsync(c => c.Id == request.CountryId, cancellationToken);
        if (country is null)
        {
            return Result.Failure<LogVisitResponse>(new Error("Error.NotFound", "Country_NotFound"));
        }

        var siteVisit = SiteVisit.Create(request.CountryId, request.Page);

        _context.SiteVisits.Add(siteVisit);

        if (request.Page.ToLower().Contains("checkout"))
        {
            var admins = await _context.Users
                .Where(u => u.Role == UserRole.Admin)
                .ToListAsync(cancellationToken);

            var notifMsg = _localizer.Get("Notification_SiteVisitAlert", $"New visit from {country.Name} on {request.Page}", country.Name, request.Page);
            foreach (var admin in admins)
            {
                _context.Notifications.Add(Notification.Create(
                    admin.Id,
                    NotificationType.SystemAlert,
                    notifMsg,
                    siteVisit.Id
                ));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var successMsg = _localizer.Get("SiteVisit_TrackedSuccessfully", "Site visit tracked successfully.");
        return Result.Success(new LogVisitResponse(siteVisit.Id, successMsg));
    }
}
