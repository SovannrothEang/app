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
        builder.Property(pt => pt.Type)
            .HasColumnType("TINYINT")
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

        builder.Property<string>("TenantId")
            .HasColumnType("VARCHAR(100)")
            .IsRequired();

        builder.Property<string>("CustomerId")
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        
        builder.HasIndex(e => e.PerformBy);
        builder.HasIndex(pt => pt.Id)
            .IsUnique();
        builder.HasIndex(pt => pt.Type);
        builder.HasIndex(pt => new { pt.TenantId, pt.CustomerId });

        builder.HasOne(e => e.PerformByUser)
            .WithMany()
            .HasForeignKey(pt => pt.PerformBy);
    }
}