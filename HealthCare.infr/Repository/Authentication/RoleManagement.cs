using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Interface;
using Microsoft.AspNetCore.Identity;


namespace HealthCare.Infreastructure.Repository.Authentication
{
    public class RoleManagement : IRoleManagement
    {
        private readonly UserManager<AppUser> userManager;

        public RoleManagement(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }
        public async Task<string?> GetUserRole(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            return (await userManager.GetRolesAsync(user!)).FirstOrDefault();
        }
        public async Task<bool> AddUserRole(AppUser user, string roleName)
        {
            return (await userManager.AddToRoleAsync(user, roleName)).Succeeded;
        }
        
    }
}
