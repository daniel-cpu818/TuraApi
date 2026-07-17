using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookingTura.Domain.Entities;

namespace BookingTura.Infrastructure.Configurations;

public class PublicationConfiguration : IEntityTypeConfiguration<Publication>
{
    public void Configure(EntityTypeBuilder<Publication> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.Property)
            .WithMany(pr => pr.Publications)
            .HasForeignKey(p => p.PropertyId);
    }
}