using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingTura.Domain.Entities;

namespace BookingTura.Infrastructure.Configurations;

public class PropertyImageConfiguration : IEntityTypeConfiguration<PropertyImage>
{
    public void Configure(EntityTypeBuilder<PropertyImage> builder)
    {
        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.Url)
            .IsRequired();

        builder.HasOne(pi => pi.Property)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.PropertyId);
    }
}