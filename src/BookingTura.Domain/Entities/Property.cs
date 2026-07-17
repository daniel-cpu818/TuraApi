using BookingTura.Domain.Common;

namespace BookingTura.Domain.Entities;

public class Property : AggregateRoot
{
    public new Guid Id { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? Address { get; set; }

    public string Hood { get; set; }

    public string? Commune { get; set; }
    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    // 🔹 OWNER
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }

    // 🔹 PROPERTY TYPE
    public Guid PropertyTypeId { get; set; }
    public PropertyType PropertyType { get; set; } = null!;

    // 🔥 LOCATION (LO QUE TE FALTABA)
    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public ICollection<Publication> Publications { get; set; } = new List<Publication>();
    public ICollection<ContactRequest> ContactRequests { get; set; } = new List<ContactRequest>();
}