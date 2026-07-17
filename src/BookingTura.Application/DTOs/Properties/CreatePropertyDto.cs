namespace BookingTura.Application.DTOs.Properties;

public class CreatePropertyDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }

    public Guid PropertyTypeId { get; set; }

    // 🔥 NUEVO MODELO (REEMPLAZA LocationId)
    public string? Hood { get; set; }
    public string? Piso { get; set; }
    public string? Commune { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}