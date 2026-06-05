using AssetTrack.Data.Models;
using AssetTrack.Data.Seeding;
using Microsoft.AspNetCore.Identity;

namespace AssetTrack.Web.Infrastructure
{
    /// <summary>
    /// Seeds the Identity roles and the default Administrator account at startup.
    /// Identity passwords are hashed, so this cannot be done reliably via HasData;
    /// categories are still seeded inside the EF migration (see CategoryConfiguration).
    /// </summary>
    public static class IdentityDataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            await EnsureRoleAsync(roleManager, SeedConstants.AdministratorRoleName);
            await EnsureRoleAsync(roleManager, SeedConstants.EmployeeRoleName);

            var admin = await userManager.FindByEmailAsync(SeedConstants.AdministratorEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    Id = SeedConstants.AdministratorUserId,
                    UserName = SeedConstants.AdministratorEmail,
                    Email = SeedConstants.AdministratorEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Administrator",
                    Department = "IT"
                };

                var result = await userManager.CreateAsync(
                    admin, SeedConstants.AdministratorDefaultPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException(
                        $"Failed to seed administrator account: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(admin, SeedConstants.AdministratorRoleName))
            {
                await userManager.AddToRoleAsync(admin, SeedConstants.AdministratorRoleName);
            }
        }

        private static async Task EnsureRoleAsync(
            RoleManager<IdentityRole> roleManager, string role)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
