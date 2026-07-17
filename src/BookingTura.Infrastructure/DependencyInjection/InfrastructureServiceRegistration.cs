using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookingTura.Infrastructure.Data;
using BookingTura.Application.Interfaces;
using BookingTura.Infrastructure.Services;

namespace BookingTura.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<BookingTuraDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)));
        services.AddHttpClient<IGeocodingService, GeocodingService>(client =>
        {
            client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("BookingTura/1.0 (+https://bookingtura.local)");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        });
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IPublicationService, PublicationService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPropertyTypeService, PropertyTypeService>();
        services.AddScoped<ILocationService, LocationService>();

        return services;
    }
}
