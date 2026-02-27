using HealthCare.Application.Dto;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.User;

namespace HealthCare.Application.Services.Interfaces.IAuthentication
{
    public interface IAuthenticationServices
    {
        public Task<bool> CreateUser(CreateUser user);
        Task<LoginResponse> Login(LoginUser loginUser);
        Task<LoginResponse> ReviveToken(string refreshToken);
        
        // Generate a password reset token for the given email. Returns token string or null if user not found.
        Task<string?> GeneratePasswordResetToken(string email);

        // Reset the user's password using token and new password. Returns true if successful.
        Task<bool> ResetPassword(ResetPassword resetPassword);

    }
}
