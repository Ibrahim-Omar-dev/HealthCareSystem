using HealthCare.Application.Mapping;
using HealthCare.Application.Services;
using HealthCare.Application.Services.Authentication;
using HealthCare.Application.Services.Interfaces;
using HealthCare.Application.Services.Interfaces.IAuthentication;
using HealthCare.Application.Validation;
using HealthCare.Application.Validation.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace HealthCare.Application.DependencyInjection
{
    public static class ContainerServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingConfig));


            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<LoginUserValidation>();
            services.AddScoped<IValidationServices, ValidationServices>();
            services.AddScoped<IAuthenticationServices, AuthenticationServices>();
            return services;
        }
    }
}