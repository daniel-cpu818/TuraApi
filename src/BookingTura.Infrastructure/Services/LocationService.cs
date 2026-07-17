using BookingTura.Application.DTOs;
using BookingTura.Application.Interfaces;
using BookingTura.Domain.Entities;
using BookingTura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingTura.Infrastructure.Services;

public class LocationService : ILocationService
{
    private readonly BookingTuraDbContext _context;
    private readonly IGeocodingService _geocodingService;

    public LocationService(
        BookingTuraDbContext context,
        IGeocodingService geocodingService)
    {
        _context = context;
        _geocodingService = geocodingService;
    }

    public async Task<List<CreateLocationDto>> GetAllAsync()
    {
        return await _context.Locations
            .Select(l => new CreateLocationDto
            {
                Id = l.Id,
                Hood = l.Hood,
                Piso = l.Piso,
                Commune = l.Commune,
                Address = l.Address,
                Latitude = l.Latitude,
                Longitude = l.Longitude
            })
            .ToListAsync();
    }

    public async Task<CreateLocationDto?> GetByIdAsync(Guid id)
    {
        var location = await _context.Locations.FindAsync(id);

        if (location == null) return null;

        return MapToDto(location);
    }

    public async Task<CreateLocationDto?> FindMatchAsync(CreateLocationDto dto)
    {
        var hood = NormalizeText(dto.Hood);
        var piso = NormalizeText(dto.Piso);
        var commune = NormalizeText(dto.Commune);

        var location = await _context.Locations.FirstOrDefaultAsync(l =>
            l.Hood == hood &&
            l.Piso == piso &&
            l.Commune == commune &&
            l.Address == dto.Address
        );

        return location == null ? null : MapToDto(location);
    }

    public async Task<CreateLocationDto> CreateAsync(CreateLocationDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Address))
            throw new Exception("La direccion es obligatoria");

        var hood = NormalizeText(dto.Hood);
        var piso = NormalizeText(dto.Piso);
        var commune = NormalizeText(dto.Commune);

        var existing = await _context.Locations.FirstOrDefaultAsync(l =>
            l.Hood == hood &&
            l.Piso == piso &&
            l.Commune == commune &&
            l.Address == dto.Address
        );

        if (existing != null)
        {
            if (!HasCoordinates(existing))
            {
                var coordinates = await ResolveCoordinatesAsync(dto);
                ApplyCoordinates(existing, coordinates);
                await _context.SaveChangesAsync();
            }

            return MapToDto(existing);
        }

        var coordinatesForNewLocation = await ResolveCoordinatesAsync(dto);

        var location = new Location
        {
            Id = Guid.NewGuid(),
            Hood = hood,
            Piso = piso,
            Commune = commune,
            Address = dto.Address,
            Latitude = coordinatesForNewLocation?.Latitude,
            Longitude = coordinatesForNewLocation?.Longitude
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return MapToDto(location);
    }

    private async Task<(double Latitude, double Longitude)?> ResolveCoordinatesAsync(CreateLocationDto dto)
    {
        try
        {
            var geocoded = await _geocodingService.GeocodeAsync(dto.Address!, dto.Commune, dto.Hood, dto.Piso);
            if (geocoded != null)
                return (geocoded.Latitude, geocoded.Longitude);
        }
        catch
        {
        }

        if (dto.Latitude.HasValue && dto.Longitude.HasValue)
            return (dto.Latitude.Value, dto.Longitude.Value);

        return null;
    }

    private static bool HasCoordinates(Location location)
    {
        return location.Latitude.HasValue && location.Longitude.HasValue;
    }

    private static void ApplyCoordinates(
        Location location,
        (double Latitude, double Longitude)? coordinates)
    {
        if (coordinates is null)
            return;

        location.Latitude = coordinates.Value.Latitude;
        location.Longitude = coordinates.Value.Longitude;
    }

    private static string NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static CreateLocationDto MapToDto(Location location)
    {
        return new CreateLocationDto
        {
            Id = location.Id,
            Hood = location.Hood,
            Piso = location.Piso,
            Commune = location.Commune,
            Address = location.Address,
            Latitude = location.Latitude,
            Longitude = location.Longitude
        };
    }
}
