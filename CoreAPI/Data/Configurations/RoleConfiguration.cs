using CoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAPI.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
            builder.ToTable("Roles");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnType("VARCHAR(100)")
                .IsRequired();
            builder.Property(e => e.TenantId)
                .HasColumnType("VARCHAR(100)");
            builder.Property(e => e.Name)
                .HasColumnType("VARCHAR(50)")
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
                .HasColumnType("VARCHAR(100)");
            
            builder.HasIndex(e => new { e.TenantId, e.Id })
                .IsUnique()
                .HasFilter($"[{nameof(Role.Id)}] <> '' AND [{nameof(Role.IsDeleted)}] = 0");
            builder.HasIndex(e => e.PerformBy);
            builder.HasIndex(e => new { e.TenantId, e.Name })
                .IsUnique()
                .HasFilter($"[{nameof(Role.Name)}] <> '' AND [{nameof(Role.IsDeleted)}] = 0");
            builder.HasIndex(e => e.TenantId)
                .HasFilter($"[{nameof(Role.IsDeleted)}] = 0");
            builder.HasOne(e => e.Tenant)
                .WithMany(e => e.Roles)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(e => e.Performer)
                .WithMany()
                .HasForeignKey(e => e.PerformBy);
    }
}