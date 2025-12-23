using CoreAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace CoreAPI.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleMgr = serviceProvider.GetRequiredService<RoleManager<Role>>();
        var userMgr = serviceProvider.GetRequiredService<UserManager<User>>();

        (string Id, string Name)[] roles =
        [
            ("SUPERADMIN6BF-3063-484E-8D3B-3600F209B391","SuperAdmin"),
            ("ADMIN7598-C50F-439D-AAF4-AE5EC63D1B93","Admin")
        ];

        foreach (var (id, name) in roles)
        {
            if (!await roleMgr.RoleExistsAsync(name))
                await roleMgr.CreateAsync(new Role { Id = id, Name = name });
        }

        const string superAdminUser = "superadmin";
        const string superAdminPass = "Admin123!"; // change in production
        const string superAdminEmail = "superadmin@example.com";

        var superAdmin = await userMgr.FindByNameAsync(superAdminUser);

        if (superAdmin is null)
        {
            superAdmin = new User(superAdminEmail, superAdminUser);
            var isExist = await userMgr.FindByEmailAsync(superAdminEmail);
            if (isExist == null)
            {
                var created = await userMgr.CreateAsync(superAdmin, superAdminPass);
                if (created.Succeeded)
                    await userMgr.AddToRoleAsync(superAdmin, roles[0].Name);
            }
        }
    }
}
