using BookingTura.Domain.Common;

namespace BookingTura.Domain.Entities;

public class PropertyType : BaseEntity
{
    public new Guid Id { get; set; }
    public string? Name { get; set; }

    public ICollection<Property> Properties { get; set; } = new List<Property>();
}