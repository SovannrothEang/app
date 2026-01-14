using CoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAPI.Data.Configurations;

public class AccountTypeConfiguration : IEntityTypeConfiguration<AccountType>
{
    public void Configure(EntityTypeBuilder<AccountType> builder)
    {
        builder.ToTable("AccountTypes");
        builder.Property(e => e.Id)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();
        builder.Property(e => e.Name)
            .HasColumnType("VARCHAR(15)")
            .IsRequired();
        builder.Property(e => e.TenantId)
            .HasColumnType("VARCHAR(36)")
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
            .HasColumnType("VARCHAR(36)")
            .IsRequired(false);

        // Index
        builder.HasIndex(e => e.Id)
            .IsUnique()
            .HasFilter($"[{nameof(AccountType.IsDeleted)}] = 0");
        builder.HasIndex(e => e.PerformBy);
        builder.HasIndex(e => new { e.Name, e.TenantId })
            .IsUnique()
            .HasFilter($"[{nameof(AccountType.IsDeleted)}] = 0");
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.IsActive, e.IsDeleted });

        // Relationships
        builder.HasMany(e => e.Accounts)
            .WithOne(e => e.AccountType)
            .HasForeignKey(e => e.AccountTypeId )
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(e => e.Performer)
            .WithMany()
            .HasForeignKey(pt => pt.PerformBy);

        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(pt => pt.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}