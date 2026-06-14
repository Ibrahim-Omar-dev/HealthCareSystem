using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.User;
using System.Security.Claims;

namespace HealthCare.Domain.Interface
{
    public interface IUserManagement
    {
        Task<AppUser> GetUserByEmail(string email);
        Task<AppUser> GetUserById(string Id);
        Task<bool> CreateUser(CreateUser user);
        Task<bool> LoginUser(LoginUser User);
        Task<bool> DeleteUserByEmail(string Email);
        Task<bool> UpdateUserByEmail(AppUser user, string Email);
        Task<IEnumerable<AppUser>> GetAllUser();
        Task<IEnumerable<Claim>> GetUserClaims(string email);
    }
}
