using BookingTura.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace BookingTura.API.Models.Publications;

public class CreatePublicationFormRequest
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public Guid PropertyTypeId { get; set; }

    public string? Hood { get; set; }

    public string? Piso { get; set; }

    public string? Commune { get; set; }

    public string? Address { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public PublicationType Type { get; set; } = PublicationType.Normal;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public List<IFormFile>? Images { get; set; }
}
