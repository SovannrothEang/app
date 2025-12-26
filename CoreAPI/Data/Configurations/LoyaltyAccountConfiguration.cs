using CoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAPI.Data.Configurations;

public class LoyaltyAccountConfiguration : IEntityTypeConfiguration<LoyaltyAccount>
{
    public void Configure(EntityTypeBuilder<LoyaltyAccount> builder)
    {
        builder.ToTable("LoyaltyAccounts");
        builder.HasKey(e => new { e.TenantId, e.CustomerId });

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

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.TenantId, e.CustomerId })
            .IsUnique();

        builder.HasMany(e => e.PointTransactions)
            .WithOne()
            .HasForeignKey(e => new { e.TenantId, e.CustomerId })
            .OnDelete(DeleteBehavior.Cascade);
    }
}