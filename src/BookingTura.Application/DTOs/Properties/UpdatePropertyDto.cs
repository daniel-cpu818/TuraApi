namespace BookingTura.Application.DTOs.Properties;

public class UpdatePropertyDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public Guid? PropertyTypeId { get; set; }

    public string? Hood { get; set; }

    public string? Piso { get; set; }

    public string? Commune { get; set; }

    public string? Address { get; set; }
}
