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
        
        builder.Property(e => e.Name)
            .HasColumnType("NVARCHAR(100)")
            .IsRequired();
        
        builder.Property(e => e.Email)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        
        builder.Property(e => e.PhoneNumber)
            .HasColumnType("VARCHAR(25)")
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
        
        builder.Property(e => e.UserId)
            .HasColumnType("VARCHAR(100)")
            .IsRequired();
        
        builder.HasIndex(e => e.Id)
            .IsUnique()
            .HasFilter($"[{nameof(Customer.IsDeleted)}] = 0");
        
        builder.HasIndex(e => e.UserId)
            .IsUnique()
            .HasFilter($"[{nameof(Customer.IsDeleted)}] = 0");
        
        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasFilter($"[{nameof(Customer.Email)}] <> '' AND [{nameof(Customer.IsDeleted)}] = 0");

        builder.HasMany(e => e.LoyaltyAccounts)
            .WithOne()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.User)
            .WithOne()
            .HasForeignKey<Customer>(e => e.UserId)
            .IsRequired();
    }
}