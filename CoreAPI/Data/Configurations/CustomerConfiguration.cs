using CoreAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreAPI.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        
        builder.Property(e => e.UserId)
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
        
        builder.HasIndex(e => e.Id)
            .IsUnique()
            .HasFilter($"[{nameof(Customer.IsDeleted)}] = 0");

        builder.HasIndex(e => e.PerformBy);
        
        builder.HasIndex(e => e.UserId)
            .IsUnique()
            .HasFilter($"[{nameof(Customer.IsDeleted)}] = 0");
        
        builder.HasMany(e => e.Accounts)
            .WithOne(e => e.Customer)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<Customer>(e => e.UserId)
            .IsRequired();

        builder.HasOne(e => e.PerformByUser)
            .WithMany()
            .HasForeignKey(e => e.PerformBy);
    }
}