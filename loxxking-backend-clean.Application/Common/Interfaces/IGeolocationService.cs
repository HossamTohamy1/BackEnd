namespace loxxking_backend_clean.Application.Common.Interfaces;

public interface IGeolocationService
{
    Task<string?> GetCountryCodeAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<GeoLocationResult?> GetGeoLocationAsync(string ipAddress, CancellationToken cancellationToken = default);
}

public record GeoLocationResult(string CountryName, string CountryCode, string Currency);
