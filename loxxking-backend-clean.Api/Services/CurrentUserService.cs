using System.Security.Claims;
using loxxking_backend_clean.Application.Common.Interfaces;

namespace loxxking_backend_clean.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(userId) ? Guid.Empty : Guid.Parse(userId);
        }
    }

    public string? GeoCountryName => _httpContextAccessor.HttpContext?.Request?.Headers["X-Geo-Country"].FirstOrDefault();
}
