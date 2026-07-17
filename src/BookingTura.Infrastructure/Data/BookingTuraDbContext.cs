using Microsoft.EntityFrameworkCore;
using BookingTura.Domain.Entities;

namespace BookingTura.Infrastructure.Data;

public class BookingTuraDbContext : DbContext
{
    public BookingTuraDbContext(DbContextOptions<BookingTuraDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Property> Properties => Set<Property>();

    public DbSet<PropertyType> PropertyTypes => Set<PropertyType>();

    public DbSet<Location> Locations => Set<Location>();

    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();

    public DbSet<Publication> Publications => Set<Publication>();

    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingTuraDbContext).Assembly);
    }
}