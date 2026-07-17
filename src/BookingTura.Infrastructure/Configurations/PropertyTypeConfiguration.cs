using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingTura.Domain.Entities;

namespace BookingTura.Infrastructure.Configurations;

public class PropertyTypeConfiguration : IEntityTypeConfiguration<PropertyType>
{
    public void Configure(EntityTypeBuilder<PropertyType> builder)
    {
        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}