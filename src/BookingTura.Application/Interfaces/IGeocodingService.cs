using BookingTura.Application.DTOs.Geocoding;

namespace BookingTura.Application.Interfaces;

public interface IGeocodingService
{
    Task<GeocodingResultDto?> GeocodeAsync(
        string address,
        string? commune = null,
        string? hood = null,
        string? piso = null,
        CancellationToken cancellationToken = default);
}
