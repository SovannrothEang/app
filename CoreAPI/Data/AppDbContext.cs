using System.Reflection;
using CoreAPI.Models;
using CoreAPI.Models.Shared;
using CoreAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreAPI.Data;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentUserProvider currentUserProvider,
    IConfiguration configuration) 
    : IdentityDbContext<
        User,
        Role,
        string,
        IdentityUserClaim<string>,
        UserRole,
        IdentityUserLogin<string>,
        IdentityRoleClaim<string>,
        IdentityUserToken<string>>(options)
{
    private readonly ICurrentUserProvider _currentUserProvider = currentUserProvider;
    private readonly string _hostTenantId = configuration["Tenancy:Host"] ?? throw new Exception("Tenancy:Host is missing.");

    public DbSet<Tenant>  Tenants { get; set; }
    // public DbSet<TenantUser>  TenantUsers { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionType> TransactionTypes { get; set; }
    public DbSet<AccountType> AccountTypes { get; set; }

    private void ApplyTenantFilter<TEntity>(ModelBuilder builder)
        where TEntity : class, ITenantEntity
    {
        builder.Entity<TEntity>()
            .HasQueryFilter(e => (e.TenantId == _currentUserProvider.TenantId));
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                case EntityState.Modified:
                    if (string.IsNullOrEmpty(entry.Entity.TenantId) &&
                        _currentUserProvider.TenantId is not null)
                    {
                        entry.Entity.TenantId = _currentUserProvider.TenantId;
                    }

                    // If the current user is SuperAdmin (Host User), they can bypass this check.
                    if (_currentUserProvider.TenantId == _hostTenantId)
                    {
                        continue;
                    }
                    
                    // Otherwise, strictly enforce that the entity's TenantId matches the User's TenantId.
                    if (entry.Entity.TenantId != _currentUserProvider.TenantId &&
                        _currentUserProvider.TenantId is not null)
                    {
                        throw new UnauthorizedAccessException(
                            $"Cross-tenant write denied. User from tenant '{_currentUserProvider.TenantId}' " +
                            $"cannot modify entity belonging to '{entry.Entity.TenantId}'.");
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
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
    }
}