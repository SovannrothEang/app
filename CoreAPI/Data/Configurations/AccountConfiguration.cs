using CoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAPI.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(e => new { e.TenantId, e.CustomerId, e.AccountTypeId });

        builder.Property(e => e.CustomerId)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();

        builder.Property(e => e.TenantId)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();
        
        builder.Property(e => e.AccountTypeId)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();
        
        builder.Property(e => e.Balance)
            .HasColumnType("DECIMAL(18,2)")
            .IsRequired();
        
        builder.Property(e => e.IsActive)
            .HasColumnType("BIT")
            .HasDefaultValue(true);
        
        builder.Property(e => e.IsDeleted)
            .HasColumnType("BIT")
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("DATETIMEOFFSET(3)")
            .IsRequired();
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("DATETIMEOFFSET(3)")
            .HasDefaultValue(null);
        
        builder.Property(e => e.DeletedAt)
            .HasColumnType("DATETIMEOFFSET(3)")
            .HasDefaultValue(null);

        builder.Property(e => e.PerformBy)
            .HasColumnType("VARCHAR(36)");

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.PerformBy);
        builder.HasIndex(e => new { e.TenantId, e.CustomerId, e.AccountTypeId })
            .IsUnique()
            .HasFilter($"[{nameof(Account.IsDeleted)}] = 0");
        builder.HasIndex(e => e.Balance);
        builder.HasIndex(e => e.AccountTypeId);

        // builder.HasOne(e => ) // TODO: introduce new Account type for account

        // Relationships
        builder.HasMany(e => e.Transactions)
            .WithOne(e => e.Account)
            .HasForeignKey(e => new { e.TenantId, e.CustomerId, e.AccountTypeId})
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Tenant)
            .WithMany(e => e.Accounts)
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Performer)
            .WithMany()
            .HasForeignKey(e => e.PerformBy);
    }
}