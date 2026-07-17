using BookingTura.Domain.Common;
using BookingTura.Domain.Enums;

namespace BookingTura.Domain.Entities;

public class ContactRequest : BaseEntity
{
    public Guid PropertyId { get; set; }

    public Guid UserId { get; set; }

    public string Message { get; set; }

    public DateTime? CheckInDate { get; set; }

    public DateTime? CheckOutDate { get; set; }

    public ContactRequestStatus Status { get; set; } = ContactRequestStatus.Pending;

    public Property Property { get; set; }

    public User User { get; set; }
}