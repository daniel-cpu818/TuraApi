using BookingTura.Infrastructure.Data;
using BookingTura.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);

var auth0Domain = builder.Configuration["Auth0:Domain"];
var auth0Audience = builder.Configuration["Auth0:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://dev-xkmh41n67u01oflw.us.auth0.com/";
        options.Audience = auth0Audience;
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BookingTuraDbContext>();

    if (app.Environment.IsDevelopment())
    {
        context.Database.Migrate();
    }

    SeedData.SeedPropertyTypes(context);
}

app.UseCors("AllowFrontend");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
