using E_Commerce.Infreastructure.Repository.Authentication;
using HealthCare.Application.Interfaces;
using HealthCare.Application.Services.Interfaces;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Interface;
using HealthCare.Domain.IRepository;
using HealthCare.Infrastructure.Repository;
using HealthCare.Infrastructure.Services.Implementation.Measurement;
using HealthCare.Infreastructure.BackgroundJobs;
using HealthCare.Infreastructure.Data;
using HealthCare.Infreastructure.Logging;
using HealthCare.Infreastructure.MiddleWare;
using HealthCare.Infreastructure.Repository;
using HealthCare.Infreastructure.Repository.Authentication;
using HealthCare.Infreastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace HealthCare.Infreastructure.DependencyInjection
{
    public static class ServicesContainer
    {
        public static IServiceCollection AddInfreastructureServices(
        this IServiceCollection services,
        IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(option =>
            option.UseSqlServer(
            config.GetConnectionString("DefaultConnection"),
            sqloption =>
            {
                sqloption.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name);
                sqloption.EnableRetryOnFailure();
            }));


        services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = null;

            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

            var jwtSettings = config.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey!))
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers["Token-Expired"] = "true";
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IAppLogger), typeof(SerlogLogger));

            services.AddScoped<IUserManagement, UserManagement>();
            services.AddScoped<IRoleManagement, RoleManagement>();
            services.AddScoped<ITokenManagement, TokenManagement>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IMeasurementService, MeasurementService>();
            services.AddScoped<IAlertService, AlertService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IFollowService, FollowService>();
            services.AddScoped<IMedicineService, MedicineService>();
            services.AddScoped<ILocationService, LocationService>();

            // Python ML Service for vitals prediction
            services.AddHttpClient("MLService", client =>
            {
                var baseUrl = config["MLService:BaseUrl"]
                              ?? throw new InvalidOperationException("MLService:BaseUrl is missing");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddScoped<IMLService, MLService>();

            // Hugging Face / Gradio Image ML Service
            services.AddHttpClient("ImageMLService", client =>
            {
                var baseUrl = config["HuggingFaceImageML:BaseUrl"]
                              ?? throw new InvalidOperationException("HuggingFaceImageML:BaseUrl is missing");

                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddScoped<IImageMLService, ImageMLService>();

            services.AddHostedService<MedicineReminderJob>();

            return services;
        }

        public static IApplicationBuilder UseInfreastructureServices(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleWare>();
            return app;
        }
    }


}
