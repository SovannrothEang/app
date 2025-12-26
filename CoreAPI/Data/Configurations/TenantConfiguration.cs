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
            .HasColumnType("VARCHAR(100)")
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnType("VARCHAR(50)")
            .IsRequired();
        
        // builder.Property(e => e.Slug)
        //     .HasColumnType("VARCHAR(50)")
        //     .IsRequired();
        
        builder.Property(e => e.Status)
            .HasColumnType("TINYINT");
        
        builder.Property(e => e.IsActive)
            .HasColumnType("BIT")
            .HasDefaultValue(true);
        
        builder.Property(e => e.IsDeleted)
            .HasColumnType("BIT")
            .HasDefaultValue(false);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("DATETIME2(3)")
            .IsRequired();
        
        builder.Property(e => e.UpdatedAt)
            .HasColumnType("DATETIME2(3)")
            .HasDefaultValue(null);
        
        builder.HasIndex(e => e.Id)
            .IsUnique()
            .HasFilter($"[{nameof(Tenant.IsDeleted)}] = 0");
        
        builder.OwnsOne(e => e.Setting);
        

    }
}