using CoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAPI.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(pt => pt.Id);
        builder.Property(pt => pt.Id)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();
        builder.Property(pt => pt.Amount)
            .HasColumnType("DECIMAL(18,2)")
            .HasAnnotation("CheckConstraint", $"{nameof(Transaction.Amount)} != 0")
            .IsRequired();
        builder.Property(pt => pt.TransactionTypeId)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();
        builder.Property(pt => pt.Reason)
            .HasColumnType("NVARCHAR(100)");
        builder.Property(pt => pt.ReferenceId)
            .HasColumnType("VARCHAR(36)");
        builder.Property(pt => pt.OccurredAt)
            .HasColumnType("DATETIMEOFFSET(3)")
            .HasAnnotation("CheckConstraint", $"{nameof(Transaction.OccurredAt)} <= {nameof(Transaction.CreatedAt)}")
            .IsRequired();
        builder.Property(pt => pt.CreatedAt)
            .HasColumnType("DATETIMEOFFSET(3)")
            .IsRequired();
        builder.Property(e => e.PerformBy)
            .HasColumnType("VARCHAR(36)");
        builder.Property(e => e.TenantId)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();
        builder.Property(e => e.CustomerId)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();
        builder.Property(e => e.AccountTypeId)
            .HasColumnType("VARCHAR(36)")
            .IsRequired();
        
        // Indexes
        builder.HasIndex(e => e.Id).IsUnique();
        builder.HasIndex(e => e.PerformBy);
        builder.HasIndex(e => e.TransactionTypeId);
        builder.HasIndex(e => new { e.TenantId, e.CustomerId, e.AccountTypeId });
        builder.HasIndex(t => t.OccurredAt);
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => new { t.CustomerId, t.OccurredAt });
        builder.HasIndex(t => new { t.TenantId, t.OccurredAt });
        builder.HasIndex(t => t.TransactionTypeId);

        // Relationships
        builder.HasOne(e => e.Referencer)
            .WithMany()
            .HasForeignKey(e => e.ReferenceId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.Performer)
            .WithMany()
            .HasForeignKey(e => e.PerformBy);
    }
}