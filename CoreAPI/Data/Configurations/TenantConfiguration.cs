using CoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAPI.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();
        builder.Property(e => e.Name)
            .HasColumnType("VARCHAR(50)")
            .IsRequired();
        builder.Property(e => e.Slug)
            .HasColumnType("VARCHAR(50)")
            .IsRequired();
        builder.Property(e => e.Status)
            .HasColumnType("TINYINT");
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

        // Indexes
        builder.HasIndex(e => e.Slug)
            .IsUnique()
            .HasFilter(
                $"[{nameof(Tenant.Slug)}] <> '' AND  [{nameof(Tenant.Slug)}] IS NOT NULL AND [{nameof(Tenant.IsDeleted)}] = 0");
        
        builder.HasIndex(e => e.Id)
            .IsUnique()
            .HasFilter($"[{nameof(Tenant.IsDeleted)}] = 0");

        builder.HasIndex(e => e.PerformBy);

        // Relationships
        builder.HasMany(e => e.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.PerformByUser)
            .WithMany()
            .HasForeignKey(e => e.PerformBy);
        builder.HasMany(e => e.Accounts)
            .WithOne(a => a.Tenant)
            .HasForeignKey(a => a.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(e => e.Setting);
    }
}