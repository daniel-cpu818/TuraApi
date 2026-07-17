namespace BookingTura.Application.DTOs.Properties;

public class PropertyResponseDto
{
    public Guid Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public Guid PropertyTypeId { get; set; }

    public string? PropertyTypeName { get; set; }

    public string? OwnerName { get; set; }

    public string? OwnerEmail { get; set; }

    public string? OwnerPhone { get; set; }

    public string? Address { get; set; }

    public string? Commune { get; set; }

    public string? Hood { get; set; }

    public string? Piso { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public IReadOnlyCollection<PropertyImageResponseDto> Images { get; set; } = [];
}
