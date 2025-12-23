using CoreAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, Role, string>(options)
{
    public DbSet<Tenant>  Tenants { get; set; }
    // public DbSet<TenantUser>  TenantUsers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        builder.Entity<User>(buildAction =>
        {
            buildAction.ToTable("Users");
            buildAction.HasKey(e => e.Id);
            buildAction.Property(e => e.Id)
                .HasColumnType("VARCHAR(100)")
                .IsRequired();
            buildAction.Property(e => e.UserName)
                .HasColumnType("VARCHAR(50)")
                .IsRequired();
            buildAction.Property(e => e.NormalizedUserName)
                .HasColumnType("VARCHAR(50)")
                .IsRequired();
            buildAction.Property(e => e.Email)
                .HasColumnType("VARCHAR(100)")
                .IsRequired();
            buildAction.Property(e => e.NormalizedEmail)
                .HasColumnType("VARCHAR(100)")
                .IsRequired();
            buildAction.Property(e => e.TenantId)
                .HasColumnType("VARCHAR(100)");
            buildAction.HasIndex(e => e.TenantId)
                .HasFilter($"[{nameof(User.TenantId)}] IS NOT NULL AND [{nameof(User.IsDeleted)}] = 0");
            buildAction.HasIndex(e => e.UserName)
                .IsUnique()
                .HasFilter($"[{nameof(User.UserName)}] <> '' AND [{nameof(User.IsDeleted)}] = 0");
            buildAction.HasIndex(e => e.Email)
                .IsUnique()
                .HasFilter($"[{nameof(User.Email)}] <> '' AND [{nameof(User.IsDeleted)}] = 0");
        });
        builder.Entity<Role>(buildAction =>
        {
            buildAction.ToTable("Roles");
            buildAction.HasKey(e => e.Id);
            buildAction.Property(e => e.Id)
                .HasColumnType("VARCHAR(100)")
                .IsRequired();
            buildAction.Property(e => e.Name)
                .HasColumnType("VARCHAR(50)")
                .IsRequired();
            buildAction.HasIndex(e => e.Name)
                .IsUnique()
                .HasFilter($"[{nameof(Role.Name)}] <> '' AND [{nameof(Role.IsDeleted)}] = 0");
        });
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
    }
}
