using HealthCare.Domain.User;

namespace HealthCare.Application.Services.Interfaces.IAuthentication
{
    public interface IAuthenticationServices
    {
        public Task<bool> CreateUser(CreateUser user);
        Task<LoginResponse> Login(LoginUser loginUser);
        Task<LoginResponse> ReviveToken(string refreshToken);
        Task<(bool IsSuccess, string Message)> ForgotPasswordAsync(string email);
        Task<(bool IsSuccess, string Message)> VerifyOtpAsync(string email, string otp);
        Task<(bool IsSuccess, string Message)> ResetPasswordAsync(string email, string otp, string newPassword);
    }
}
