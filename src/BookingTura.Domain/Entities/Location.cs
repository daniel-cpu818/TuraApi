using BookingTura.Domain.Common;

namespace BookingTura.Domain.Entities;

public class Location : BaseEntity
{

    public string? Hood { get; set; }
    public string? Piso { get; set; }
    public string? Commune { get; set; }
    public string? Address { get; set; }
     // NUEVO
    public double? Latitude { get; set; }

    // NUEVO
    public double? Longitude { get; set; }

    public ICollection<Property> Properties { get; set; } = new List<Property>();
}