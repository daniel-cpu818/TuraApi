using BookingTura.Domain.Common;
using BookingTura.Domain.Enums;

namespace BookingTura.Domain.Entities;

public class Publication : BaseEntity
{
    public Guid PropertyId { get; set; }

    public PublicationType Type { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public Property Property { get; set; }
}
