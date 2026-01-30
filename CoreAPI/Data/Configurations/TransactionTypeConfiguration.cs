using CoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAPI.Data.Configurations;

public class TransactionTypeConfiguration : IEntityTypeConfiguration<TransactionType>
{
    public void Configure(EntityTypeBuilder<TransactionType> builder)
    {
        builder.ToTable("TransactionTypes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();
        builder.Property(e => e.Slug)
            .HasColumnType("VARCHAR(15)")
            .IsRequired();
        builder.Property(e => e.Name)
            .HasColumnType("VARCHAR(15)")
            .IsRequired();
        builder.Property(e => e.Description)
            .HasColumnType("VARCHAR(MAX)")
            .IsRequired(false);
        builder.Property(e => e.Url)
            .HasColumnType("VARCHAR(200)")
            .IsRequired();
        builder.Property(e => e.TenantId)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();
        builder.Property(e => e.Multiplier)
            .HasColumnType("INT")
            .IsRequired();
        builder.Property(e => e.AllowNegative)
            .HasColumnType("BIT")
            .HasDefaultValue(false);
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
        builder.HasIndex(e => e.Id)
            .IsUnique()
            .HasFilter($"[{nameof(TransactionType.IsDeleted)}] = 0");
        builder.HasIndex(e => e.PerformBy);
        builder.HasIndex(e => new { e.Name, e.TenantId })
            .IsUnique()
            .HasFilter($"[{nameof(TransactionType.IsDeleted)}] = 0");
        builder.HasIndex(e => new { e.TenantId, e.Slug })
            .IsUnique()
            .HasFilter($"[{nameof(TransactionType.Slug)}] <> '' AND [{nameof(TransactionType.IsDeleted)}] = 0");
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.IsActive, e.IsDeleted });

        // Relationships
        builder.HasMany(e => e.Transactions)
            .WithOne(e => e.TransactionType)
            .HasForeignKey(e => e.TransactionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Performer)
            .WithMany()
            .HasForeignKey(pt => pt.PerformBy);

        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(pt => pt.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}