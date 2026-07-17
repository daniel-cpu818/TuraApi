using BookingTura.Domain.Entities;

namespace BookingTura.Infrastructure.Data;

public static class SeedData
{
    public static void SeedPropertyTypes(BookingTuraDbContext context)
    {
        if (context.PropertyTypes.Any())
            return;

        var types = new List<PropertyType>
        {
            new PropertyType { Id = Guid.NewGuid(), Name = "Apartment" },
            new PropertyType { Id = Guid.NewGuid(), Name = "House" },
            new PropertyType { Id = Guid.NewGuid(), Name = "Room" },
            new PropertyType { Id = Guid.NewGuid(), Name = "Hotel" }
        };

        context.PropertyTypes.AddRange(types);
        context.SaveChanges();
    }
}