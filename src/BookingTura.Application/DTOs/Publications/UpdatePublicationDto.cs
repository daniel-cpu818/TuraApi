using BookingTura.Domain.Enums;

namespace BookingTura.Application.DTOs.Publications;

public class UpdatePublicationDto
{
    // Datos de la propiedad
    public string? Title { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public Guid PropertyTypeId { get; set; }

    public string? Hood { get; set; }

    public string? Commune { get; set; }

    public string? Piso { get; set; }

    public string? Address { get; set; }

    // Coordenadas (opcional, si las usas)
    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    // Datos de la publicación
    public PublicationType Type { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }
}