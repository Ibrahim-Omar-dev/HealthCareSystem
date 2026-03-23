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

        Task<bool> ForgotPasswordAsync(string email);
        Task<(bool IsSuccess, string Message)> ResetPasswordAsync(string token, string newPassword);

        // External login (e.g., Google). Finds or creates a user and returns authentication tokens.
        Task<LoginResponse> ExternalLogin(string email, string? userName);

        // Passwordless email login: send numeric code to email
        Task<bool> SendLoginCodeAsync(string email);

        // Verify code and login user, returning JWT + refresh token
        Task<LoginResponse> LoginWithCodeAsync(string email, string code);
    }
}
