using CoreAPI.Models;
using CoreAPI.Repositories.Interfaces;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CoreAPI.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleMgr = serviceProvider.GetRequiredService<RoleManager<Role>>();
        var userMgr = serviceProvider.GetRequiredService<UserManager<User>>();
        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var tenantRepository = serviceProvider.GetRequiredService<ITenantRepository>();
        var tenantContext = serviceProvider.GetRequiredService<ICurrentUserProvider>();

        // Failed cuz we did not manage the unique while the db is enforcing the multi-tenancy!!
        var tenantHost = config["Tenancy:Host"]
            ?? throw new Exception("Tenancy:Host not found");
        if (!await tenantRepository.IsExistByIdAsync(tenantHost))
        {
            await tenantRepository.CreateAsync(new Tenant(tenantHost, tenantHost));
            await tenantRepository.SaveChangeAsync();
        }
        tenantContext.SetTenantId(tenantHost);
        
        (string Id, string Name)[] roles =
        [
            ("SUPERADMIN6BF-3063-484E-8D3B-3600F209B391","SuperAdmin"),
            ("ADMIN7598-C50F-439D-AAF4-AE5EC63D1B93","Admin")
        ];

        foreach (var (id, name) in roles)
        {
            var roleExist = await roleMgr.FindByIdAsync(id);
            if (roleExist != null) continue;
            if (!await roleMgr.RoleExistsAsync(name))
                await roleMgr.CreateAsync(new Role(id, name, tenantHost));
        }

        const string superAdminUser = "superadmin";
        const string superAdminPass = "Admin123!"; // change in production
        const string superAdminEmail = "superadmin@example.com";

        var isExist = await userMgr.FindByEmailAsync(superAdminEmail);
        var isExistById = await userMgr.FindByIdAsync(roles[0].Id);
        if (isExist == null && isExistById == null)
        {
            var tenantExist = await tenantRepository.IsExistByIdAsync(tenantHost);
            if (!tenantExist)
                throw new Exception("Tenant not found");
            var superAdmin = new User(roles[0].Id, superAdminEmail, superAdminUser, tenantHost);
            var created = await userMgr.CreateAsync(superAdmin, superAdminPass);
            if (created.Succeeded)
                await userMgr.AddToRoleAsync(superAdmin, roles[0].Name);
        }
    }
}
