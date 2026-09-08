using Microsoft.Extensions.Caching.Distributed;

namespace loxxking_backend_clean.Application.Features.Countries.Commands.SetDefaultCountry;

public record SetDefaultCountryCommand(Guid CountryId) : IRequest<Result>;

public class SetDefaultCountryHandler : IRequestHandler<SetDefaultCountryCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    
    public SetDefaultCountryHandler(IApplicationDbContext context, IDistributedCache cache) 
    { 
        _context = context; 
        _cache = cache;
    }

    public async Task<Result> Handle(SetDefaultCountryCommand request, CancellationToken cancellationToken)
    {
        var newDefault = await _context.Countries.FirstOrDefaultAsync(c => c.Id == request.CountryId, cancellationToken);
        if (newDefault == null) 
            return Result.Failure(new Error("Error.NotFound", "Country_NotFound"));

        if (newDefault.IsDefault)
            return Result.Success();

        var currentDefaults = await _context.Countries.Where(c => c.IsDefault).ToListAsync(cancellationToken);
        
        foreach (var currentDefault in currentDefaults)
        {
            currentDefault.RemoveDefault();
            _context.Countries.Update(currentDefault);
        }

        newDefault.SetAsDefault();
        _context.Countries.Update(newDefault);

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync("Countries_All", cancellationToken);

        return Result.Success();
    }
}
