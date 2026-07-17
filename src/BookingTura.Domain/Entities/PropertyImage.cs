using BookingTura.Domain.Common;

namespace BookingTura.Domain.Entities;

public class PropertyImage : BaseEntity
{
    public Guid PropertyId { get; set; }

    public string? Url { get; set; }

    public bool IsMain { get; set; }

    public required Property Property { get; set; }
}