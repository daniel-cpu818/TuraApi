namespace BookingTura.Application.DTOs;

public class CreateLocationDto
{
    public Guid Id { get; set; }
    public string? Hood { get; set; }
    public string? Piso { get; set; }
    public string? Commune { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}