using CoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAPI.Data.Configurations;

public class PointTransactionConfiguration : IEntityTypeConfiguration<PointTransaction>
{
    public void Configure(EntityTypeBuilder<PointTransaction> builder)
    {
        builder.ToTable("PointTransactions");
        builder.HasKey(pt => pt.Id);
        builder.Property(pt => pt.Id)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        builder.Property(pt => pt.Amount)
            .HasColumnType("INT")
            .IsRequired();
        builder.Property(pt => pt.Type)
            .HasColumnType("TINYINT")
            .IsRequired();
        builder.Property(pt => pt.Reason)
            .HasColumnType("NVARCHAR(100)");
        builder.Property(pt => pt.ReferenceId)
            .HasColumnType("VARCHAR(100)");
        builder.Property(pt => pt.OccurredOn)
            .HasColumnType("DATETIME2(3)")
            .IsRequired();

        builder.Property<string>("TenantId")
            .HasColumnType("VARCHAR(100)")
            .IsRequired();

        builder.Property<string>("CustomerId")
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        
        builder.HasIndex(pt => pt.Id)
            .IsUnique();
        builder.HasIndex(pt => pt.Type);
    }
}