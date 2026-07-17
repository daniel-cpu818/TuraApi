using BookingTura.Domain.Common;
using BookingTura.Domain.Enums;

namespace BookingTura.Domain.Entities;

public class User : AggregateRoot
{
    public string? Name { get; set; }
    public string Auth0Id { get; set; }
    
    public Guid Id { get; set; } 

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public UserRole Role { get; set; }

    public ICollection<Property> Properties { get; set; } = new List<Property>();

    public ICollection<ContactRequest> ContactRequests { get; set; } = new List<ContactRequest>();
}