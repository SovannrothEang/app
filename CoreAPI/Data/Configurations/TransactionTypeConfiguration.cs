using CoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAPI.Data.Configurations;

public class TransactionTypeConfiguration : IEntityTypeConfiguration<TransactionType>
{
    public void Configure(EntityTypeBuilder<TransactionType> builder)
    {
        builder.Property(e => e.Id)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnType("VARCHAR(15)")
            .IsRequired();
        
        builder.Property(e => e.TenantId)
            .HasColumnType("VARCHAR(100)")
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
        
        // Index
        builder.HasIndex(e => e.Id)
            .IsUnique()
            .HasFilter($"[{nameof(Customer.IsDeleted)}] = 0");
        builder.HasIndex(e => e.PerformBy);
        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasFilter($"[{nameof(TransactionType.IsDeleted)}] = 0");
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.IsActive, e.IsDeleted });
        
        // Relationships
        builder.HasMany(e => e.Transactions)
            .WithOne()
            .HasForeignKey(e => e.TransactionTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(e => e.Performer)
            .WithMany()
            .HasForeignKey(pt => pt.PerformBy);
    }
}