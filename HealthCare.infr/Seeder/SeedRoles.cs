using HealthCare.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace HealthCare.Infrastructure.Seeder
{
    public static class SeedRoles
    {
        public static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager)
        {
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@gmail.com";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var adminUser = new AppUser
                {

                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Hema-2003");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
