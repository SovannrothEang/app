using CoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAPI.Data.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(e => new { e.UserId, e.RoleId, e.TenantId });
        builder.Property(e => e.UserId)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        builder.Property(e => e.RoleId)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        builder.Property(e => e.TenantId)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();

        builder.HasOne(e => e.User)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => new { e.UserId, e.TenantId })
            .HasPrincipalKey(e => new { e.Id, e.TenantId })
            .IsRequired();
        builder.HasOne(e => e.Role)
            .WithMany()
            .HasForeignKey(e => new  { e.RoleId, e.TenantId })
            .HasPrincipalKey(e => new { e.Id, e.TenantId })
            .IsRequired();
        
        builder.HasIndex(e => new { e.UserId, e.RoleId, e.TenantId })
            .IsUnique();
    }
}