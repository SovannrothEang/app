using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(e => new { e.UserId, e.RoleId });
        builder.Property(e => e.UserId)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();
        builder.Property(e => e.RoleId)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();
        builder.Property(e => e.TenantId)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();

        builder.HasOne(e => e.User)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => new { e.UserId, e.TenantId })
            .HasPrincipalKey(e => new { e.Id, e.TenantId })
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        builder.HasOne(e => e.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(e => new  { e.RoleId, e.TenantId })
            .HasPrincipalKey(e => new { e.Id, e.TenantId })
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        builder.HasIndex(e => new { e.UserId, e.RoleId, e.TenantId })
            .IsUnique();
    }
}