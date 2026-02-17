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

    }
}
