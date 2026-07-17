using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingTura.Domain.Entities;

namespace BookingTura.Infrastructure.Configurations;

public class ContactRequestConfiguration : IEntityTypeConfiguration<ContactRequest>
{
    public void Configure(EntityTypeBuilder<ContactRequest> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Message)
            .IsRequired();

        builder.HasOne(c => c.Property)
            .WithMany(p => p.ContactRequests)
            .HasForeignKey(c => c.PropertyId);

        builder.HasOne(c => c.User)
            .WithMany(u => u.ContactRequests)
            .HasForeignKey(c => c.UserId);
    }
}