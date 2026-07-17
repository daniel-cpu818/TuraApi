using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingTura.Domain.Entities;

namespace BookingTura.Infrastructure.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .IsRequired();

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        builder.HasOne(p => p.PropertyType)
            .WithMany(pt => pt.Properties)
            .HasForeignKey(p => p.PropertyTypeId);

        builder.HasOne(p => p.Location)
            .WithMany(l => l.Properties)
            .HasForeignKey(p => p.LocationId);
    }
}