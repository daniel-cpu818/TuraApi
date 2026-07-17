using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingTura.Domain.Entities;

namespace BookingTura.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Hood)
            .HasMaxLength(150);

        builder.Property(l => l.Piso)
            .HasMaxLength(50);

        builder.Property(l => l.Commune)
            .HasMaxLength(150);

        builder.Property(l => l.Address)
            .IsRequired();
    }
}
