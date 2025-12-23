using CoreAPI.Models;
using CoreAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAPI.Data.Configurations;

public class LoyaltyAccountConfiguration : IEntityTypeConfiguration<LoyaltyAccount>
{
    public void Configure(EntityTypeBuilder<LoyaltyAccount> builder)
    {
        builder.ToTable("LoyaltyAccounts");
        builder.HasKey(e => new { e.TenantId, e.CustomerId });
        // builder.HasQueryFilter(e =>
        //     // Tenant owner retrieving all available accounts within their scope.
        //     (!e.IsDeleted && e.Id.TenantId == _currentUserService.TenantId) ||
        //     // Customer retrieving all their accounts within all the tenant
        //     (!e.IsDeleted && _currentUserService.IsInRole("Customer") && e.Id.CustomerId == _currentUserService.UserId) || 
        //     // SuperAdmin retrieving all accounts
        //     (_currentUserService.IsInRole("SuperAdmin")));

        builder.Property(e => e.CustomerId)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();

        builder.Property(e => e.TenantId)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        
        builder.HasKey("TenantId", "CustomerId");
        
        builder.Property(e => e.Balance)
            .HasColumnType("INT")
            .IsRequired();
        
        builder.Property(e => e.Tier)
            .HasColumnType("TINYINT")
            .IsRequired();

        builder.HasMany(e => e.PointTransactions)
            .WithOne()
            .HasForeignKey(e => new { e.TenantId, e.CustomerId })
            .OnDelete(DeleteBehavior.Cascade);
    }
}