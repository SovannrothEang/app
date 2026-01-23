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
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        var tenantRepository = unitOfWork.GetRepository<Tenant>();
        var repository = unitOfWork.GetRepository<Tenant>();
        var tenantContext = serviceProvider.GetRequiredService<ICurrentUserProvider>();

        // Failed cuz we did not manage the unique while the db is enforcing the multi-tenancy!!
        var tenantHost = config["Tenancy:Host"]
            ?? throw new Exception("Tenancy:Host not found");
        if (!await tenantRepository.ExistsAsync(e => e.Id == tenantHost))
        {
            await repository.CreateAsync(new Tenant(tenantHost, tenantHost, tenantHost));
            
            // I think tenant host never gonna need that
            // IEnumerable<TransactionTypeCreateDto> types =
            // [
            //     new("earn", "Earn", "Points earned from activities", 1, false),
            //     new("redeem", "Redeem", "Points redeems for rewards", 1, false),
            //     new("adjust", "Adjust", "Manual points adjustment", 1, false),
            // ];
            // await transactionType.CreateBatchAsync(types);

            await unitOfWork.CompleteAsync();
        }
        tenantContext.SetTenantId(tenantHost);
        
        (string Id, string Name)[] roles =
        [
            ("SUPERADMIN6BF-3063-484E-8D3B-36F2091","SuperAdmin"),
            ("ADMIN758-C50F-439D-AAF4-AE5EC63D1B93","Admin")
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
            var tenantExist = await tenantRepository.ExistsAsync(e => e.Id == tenantHost);
            if (!tenantExist)
                throw new Exception("Tenant not found");
            var superAdmin = new User(roles[0].Id, superAdminEmail, superAdminUser, tenantHost)
            {
                FirstName = superAdminUser,
                LastName = superAdminUser,
            };
            var created = await userMgr.CreateAsync(superAdmin, superAdminPass);
            if (created.Succeeded)
                await userMgr.AddToRoleAsync(superAdmin, roles[0].Name);
        }
    }
}
