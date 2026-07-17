using BookingTura.Application.DTOs.Properties;
using BookingTura.Domain.Enums;

namespace BookingTura.Application.DTOs.Publications;

public class PublicationResponseDto
{
    public Guid Id { get; set; }

    public Guid PropertyId { get; set; }

    public PublicationType Type { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public PropertyResponseDto Property { get; set; } = new();
}
