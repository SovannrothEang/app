using CoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAPI.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        builder.Property(e => e.UserName)
            .HasColumnType("VARCHAR(50)")
            .IsRequired();
        builder.Property(e => e.FirstName)
            .HasColumnType("VARCHAR(50)")
            .IsRequired();
        builder.Property(e => e.LastName)
            .HasColumnType("VARCHAR(50)")
            .IsRequired();
        builder.Property(e => e.NormalizedUserName)
            .HasColumnType("VARCHAR(50)")
            .IsRequired();
        builder.Property(e => e.Email)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        builder.Property(e => e.NormalizedEmail)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        builder.Property(e => e.AuthProvider)
            .HasColumnType("VARCHAR(10)")
            .IsRequired()
            .HasDefaultValue("Local");
        builder.Property(e => e.ProviderKey)
            .HasColumnType("VARCHAR(100)");
        builder.Property(e => e.PasswordHash)
            .HasColumnType("VARCHAR(MAX)");
        builder.Property(e => e.TenantId)
            .HasColumnType("VARCHAR(100)");
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
            
        // Fixed Index Filter later
        builder.HasIndex(e => new { e.TenantId, e.Id })
            .IsUnique()
            .HasFilter($"[{nameof(User.TenantId)}] IS NOT NULL AND [{nameof(User.IsDeleted)}] = 0");
        builder.HasIndex(e => e.TenantId)
            .HasFilter($"[{nameof(User.TenantId)}] IS NOT NULL AND [{nameof(User.IsDeleted)}] = 0");
        builder.HasIndex(e => new { e.TenantId, e.UserName })
            .IsUnique() 
            .HasFilter($"[{nameof(User.UserName)}] <> '' AND [{nameof(User.IsDeleted)}] = 0");
        builder.HasIndex(e => new { e.TenantId, e.Email })
            .IsUnique() 
            .HasFilter($"[{nameof(User.Email)}] <> '' AND [{nameof(User.IsDeleted)}] = 0");
        builder.HasOne(e => e.Tenant)
            .WithMany(e => e.Users)
            .HasForeignKey(e => e.TenantId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}