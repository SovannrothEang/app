using System.Reflection;
using CoreAPI.Models;
using CoreAPI.Models.Shared;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserProvider currentUserProvider) : IdentityDbContext<User, Role, string>(options)
{
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;

    public DbSet<Tenant>  Tenants { get; set; }
    // public DbSet<TenantUser>  TenantUsers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Account> LoyaltyAccounts { get; set; }
    public DbSet<Transaction> PointTransactions { get; set; }

    private void ApplyTenantFilter<TEntity>(ModelBuilder builder)
        where TEntity : class, ITenantEntity
    {
        builder.Entity<TEntity>()
            .HasQueryFilter(e => (e.TenantId == _currentUserProvider.TenantId));
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Keep tracking, and ensure tenant owner can only modify their data
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                case EntityState.Modified:
                    if (string.IsNullOrEmpty(entry.Entity?.TenantId) &&
                        _currentUserProvider.TenantId is not null)
                    {
                        entry.Entity?.TenantId = _currentUserProvider.TenantId;
                    }
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ApplyTenantFilter),
                        BindingFlags.NonPublic |
                        BindingFlags.Instance)
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, [builder]);
            }
        }
        
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
            // Fixed Index Filter later
            buildAction.HasIndex(e => new { e.TenantId, e.Id })
                .IsUnique()
                .HasFilter($"[{nameof(User.TenantId)}] IS NOT NULL AND [{nameof(User.IsDeleted)}] = 0");
            buildAction.HasIndex(e => e.TenantId)
                .HasFilter($"[{nameof(User.TenantId)}] IS NOT NULL AND [{nameof(User.IsDeleted)}] = 0");
            buildAction.HasIndex(e => new { e.TenantId, e.UserName })
                .IsUnique() 
                .HasFilter($"[{nameof(User.UserName)}] <> '' AND [{nameof(User.IsDeleted)}] = 0");
            buildAction.HasIndex(e => new { e.TenantId, e.Email })
                .IsUnique() 
                .HasFilter($"[{nameof(User.Email)}] <> '' AND [{nameof(User.IsDeleted)}] = 0");
            buildAction.HasOne(e => e.Tenant)
                .WithMany(e => e.Users)
                .HasForeignKey(e => e.TenantId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
        });
        builder.Entity<Role>(buildAction =>
        {
            buildAction.ToTable("Roles");
            buildAction.HasKey(e => e.Id);
            buildAction.Property(e => e.Id)
                .HasColumnType("VARCHAR(100)")
                .IsRequired();
            buildAction.Property(e => e.TenantId)
                .HasColumnType("VARCHAR(100)");
            buildAction.Property(e => e.Name)
                .HasColumnType("VARCHAR(50)")
                .IsRequired();
            buildAction.HasIndex(e => new { e.TenantId, e.Id })
                .IsUnique()
                .HasFilter($"[{nameof(Role.Id)}] <> '' AND [{nameof(Role.IsDeleted)}] = 0");
            buildAction.HasIndex(e => new { e.TenantId, e.Name })
                .IsUnique()
                .HasFilter($"[{nameof(Role.Name)}] <> '' AND [{nameof(Role.IsDeleted)}] = 0");
            buildAction.HasIndex(e => e.TenantId)
                .HasFilter($"[{nameof(Role.IsDeleted)}] = 0");
            buildAction.HasOne(e => e.Tenant)
                .WithMany(e => e.Roles)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.NoAction);
        });
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
    }
}