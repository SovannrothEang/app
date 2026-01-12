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
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        builder.Property(pt => pt.Amount)
            .HasColumnType("DECIMAL(18,2)")
            .IsRequired();
        builder.Property(pt => pt.TransactionTypeId)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        builder.Property(pt => pt.Reason)
            .HasColumnType("NVARCHAR(100)");
        builder.Property(pt => pt.ReferenceId)
            .HasColumnType("VARCHAR(100)");
        builder.Property(pt => pt.OccurredAt)
            .HasColumnType("DATETIMEOFFSET(3)")
            .IsRequired();
        builder.Property(pt => pt.CreatedAt)
            .HasColumnType("DATETIMEOFFSET(3)")
            .IsRequired();
        builder.Property(e => e.PerformBy)
            .HasColumnType("VARCHAR(100)");

        builder.Property(e => e.TenantId)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();

        builder.Property(e => e.CustomerId)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        
        builder.Property(e => e.AccountTypeId)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        
        builder.HasIndex(e => e.Id).IsUnique();
        builder.HasIndex(e => e.PerformBy);
        builder.HasIndex(e => e.TransactionTypeId);
        builder.HasIndex(e => new { e.TenantId, e.CustomerId, e.AccountTypeId });
        
        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.ReferenceId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(e => e.Performer)
            .WithMany()
            .HasForeignKey(e => e.PerformBy);
    }
}