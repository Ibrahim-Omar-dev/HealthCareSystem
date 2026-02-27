using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Enums;
using HealthCare.Domain.Interface;
using HealthCare.Domain.User;
using HealthCare.Infreastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E_Commerce.Infreastructure.Repository.Authentication
{
    public class UserManagement : IUserManagement
    {
        private readonly UserManager<AppUser> userManager;
        private readonly AppDbContext context;
        private readonly IRoleManagement roleManagement;

        public UserManagement(UserManager<AppUser> userManager, AppDbContext context,IRoleManagement roleManagement)
        {
            this.userManager = userManager;
            this.context = context;
            this.roleManagement = roleManagement;
        }
        public async Task<bool> CreateUser(CreateUser createUser)
        {
            var existingUser = await GetUserByEmail(createUser.Email);
            if (existingUser != null)
                return false;

            var user = new AppUser
            {
                UserName = createUser.Email,      
                Email = createUser.Email,
                DisplayName = createUser.UserName 
            };

            var result = await userManager.CreateAsync(user, createUser.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    Console.WriteLine($"Code: {error.Code} | Description: {error.Description}");
                return false;
            }
            return true;
        }

        public async Task<bool> DeleteUserByEmail(string email)
        {
            var user =await context.Users.FirstOrDefaultAsync(_ => _.Email == email);
            if (user != null)
            {
                return false;
            }
            context.Users.Remove(user!);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AppUser>> GetAllUser()
        {
            return await context.Users.ToListAsync();
        }

        public async Task<AppUser> GetUserByEmail(string email)
        {
            return await userManager.FindByEmailAsync(email);
        }

        public async Task<AppUser> GetUserById(string id)
        {
            return await userManager.FindByIdAsync(id);
        }

        public async Task<IEnumerable<Claim>> GetUserClaims(string email)
        {
            var roleName = await roleManagement.GetUserRole(email);
            var user = await GetUserByEmail(email);
            var claims = new List<Claim>
            {
                new Claim("FullName",user.UserName),
                new Claim(ClaimTypes.Email,user.Email!),
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(ClaimTypes.Role,roleName!),
            };

            return claims;
        }

        public async Task<bool> LoginUser(LoginUser user)
        {
            var _user = await GetUserByEmail(user.Email!);
            if (_user == null) return false;
            string? roleName = await roleManagement.GetUserRole(_user!.Email!);
            if(string.IsNullOrEmpty(roleName)) return false;

            return await userManager.CheckPasswordAsync(_user, user.Password);
        }
       

        public async Task<bool> UpdateUserByEmail(AppUser user, string email)
        {
            var _user = await context.Users.FirstOrDefaultAsync(_ => _.Email == email);
            if (_user == null) return false;
            context.Users.Update(user);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
