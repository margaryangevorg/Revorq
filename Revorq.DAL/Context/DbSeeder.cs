using Revorq.DAL.Enums;
using Microsoft.AspNetCore.Identity;

namespace Revorq.DAL.Context;

public static class DbSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
    {
        foreach (var role in Enum.GetValues<Role>())
        {
            var roleName = role.ToString();
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole<int>(roleName));
        }
    }
}
