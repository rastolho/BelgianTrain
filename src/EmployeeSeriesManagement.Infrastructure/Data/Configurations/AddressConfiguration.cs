using EmployeeSeriesManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeSeriesManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF mapping for <see cref="Address"/> → <c>Addresses</c>.
/// </summary>
public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Country).HasMaxLength(50).IsRequired();
        builder.Property(a => a.City).HasMaxLength(50).IsRequired();
        builder.Property(a => a.ZipCode).HasMaxLength(5).IsRequired();
        builder.Property(a => a.Street).HasMaxLength(60).IsRequired();
        builder.Property(a => a.Number).HasMaxLength(10).IsRequired();
        builder.Property(a => a.MailboxNumber).HasMaxLength(10);
        builder.Property(a => a.Building).HasMaxLength(40);
        builder.Property(a => a.Floor).HasMaxLength(10);

        builder.HasOne(a => a.AddressType)
            .WithMany(t => t.Addresses)
            .HasForeignKey(a => a.AddressTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
