using Google.Apis.Auth;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace HealthCare.Infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public GoogleAuthService(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<(bool Success, AppUser? User, string Message)> LoginOrRegisterAsync(string idToken)
        {
            GoogleJsonWebSignature.Payload payload;

            try
            {
                var clientIds = _configuration.GetSection("GoogleAuth:ClientIds").Get<string[]>();

                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = clientIds 
                };

                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch (InvalidJwtException)
            {
                return (false, null, "Invalid Google token.");
            }

            if (!payload.EmailVerified)
                return (false, null, "Google email is not verified.");

            var user = await _userManager.FindByLoginAsync("Google", payload.Subject);

            if (user != null)
                return (true, user, "Login successful.");

            user = await _userManager.FindByEmailAsync(payload.Email);

            if (user != null)
            {
                var linkResult = await _userManager.AddLoginAsync(user,
                    new UserLoginInfo("Google", payload.Subject, "Google"));

                if (!linkResult.Succeeded)
                    return (false, null, "Failed to link Google account.");

                return (true, user, "Login successful.");
            }

            user = new AppUser
            {
                UserName = payload.Email,
                Email = payload.Email,
                DisplayName = payload.Name ?? payload.Email,
                EmailConfirmed = true 
            };

            var createResult = await _userManager.CreateAsync(user); 

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return (false, null, $"Failed to create user: {errors}");
            }

            await _userManager.AddLoginAsync(user,
                new UserLoginInfo("Google", payload.Subject, "Google"));

            return (true, user, "Account created and logged in successfully.");
        }
    }
}