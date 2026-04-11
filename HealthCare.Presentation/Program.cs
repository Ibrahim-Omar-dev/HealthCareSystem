using HealthCare.Application.DependencyInjection;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Infrastructure.Seeder;
using HealthCare.Infreastructure.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Serilog;

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

try
{
    Log.Information("App Is Building...........");

    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                  optional: true, 
                  reloadOnChange: true)
    .AddEnvironmentVariables();

    builder.Host.UseSerilog();

    // ── CORS ─────────────────────────────────────────────────────────────────
    // Single policy that covers both needs — AllowCredentials requires specific origin
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
            policy.WithOrigins(
                      builder.Configuration["Frontend:BaseUrl"]
                          ?? "http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());
    });

    // ── App Services ──────────────────────────────────────────────────────────
    builder.Services.AddInfreastructureServices(builder.Configuration);
    builder.Services.AddApplicationServices();

    // ── Swagger ───────────────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ── Controllers ───────────────────────────────────────────────────────────
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.RespectRequiredConstructorParameters = true;
        });

    // ── Cookie fix: prevent Identity from redirecting API calls to /Account/Login
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();  
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        await SeedRoles.SeedRolesAsync(roleManager, userManager);
    }
    app.UseDeveloperExceptionPage();
    app.UseSerilogRequestLogging();

    app.UseSwagger();
    app.UseSwaggerUI(c => c.RoutePrefix = "swagger");
    app.MapGet("/", () => Results.Redirect("/swagger"));

    app.UseHttpsRedirection();

    app.UseCors("AllowFrontend"); 
    app.UseInfreastructureServices();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("App Is Running..............");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "App Failed To Start..............");
}
finally
{
    Log.CloseAndFlush();
}