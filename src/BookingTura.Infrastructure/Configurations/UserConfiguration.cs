using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingTura.Domain.Entities;

namespace BookingTura.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.Phone)
            .HasMaxLength(50);

        builder.HasMany(u => u.Properties)
            .WithOne(p => p.Owner)
            .HasForeignKey(p => p.OwnerId);
    }
}
