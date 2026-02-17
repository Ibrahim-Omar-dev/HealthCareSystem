using HealthCare.Application.DependencyInjection;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Infrastructure.Seeder;
using HealthCare.Infreastructure.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

builder.Host.UseSerilog();
Log.Logger.Information("App Is building...........");

// Add services
builder.Services.AddInfreastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.RespectRequiredConstructorParameters = true;
    });

try
{
    var app = builder.Build();

    // Seed roles and admin user
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        await SeedRoles.SeedRolesAsync(roleManager, userManager);
    }

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "swagger";
        });
    }

    app.MapGet("/", () => Results.Redirect("/swagger"));
    app.UseHttpsRedirection();

    app.UseInfreastructureServices();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Logger.Information("App is Running..............");
    app.Run();
}
catch (Exception ex)
{
    Log.Logger.Fatal(ex, "App Failed To Start..............");
}
finally
{
    Log.CloseAndFlush();
}