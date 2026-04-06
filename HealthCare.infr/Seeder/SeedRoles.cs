using HealthCare.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace HealthCare.Infrastructure.Seeder
{
    public static class SeedRoles
    {
        public static async Task SeedRolesAsync(
            RoleManager<IdentityRole<Guid>> roleManager,
            UserManager<AppUser> userManager)
        {
            string[] roles = { "Admin", "User", "Doctor", "Patient" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));  
            }

            var adminEmail = "admin@gmail.com";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

            if (existingAdmin == null)
            {
                var adminUser = new AppUser
                {
                    UserName = "Admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    DisplayName = "Admin"
                };

                var result = await userManager.CreateAsync(adminUser, "Hema-2003");

                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}