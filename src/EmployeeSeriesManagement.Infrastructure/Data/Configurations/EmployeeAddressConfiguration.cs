using EmployeeSeriesManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeSeriesManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF mapping for <see cref="EmployeeAddress"/> → <c>EmployeesAddresses</c> (composite PK).
/// </summary>
public class EmployeeAddressConfiguration : IEntityTypeConfiguration<EmployeeAddress>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EmployeeAddress> builder)
    {
        builder.ToTable("EmployeesAddresses");
        builder.HasKey(ea => new { ea.EmployeesExternalId, ea.AddressesId });

        builder.HasOne(ea => ea.Employee)
            .WithMany(e => e.EmployeeAddresses)
            .HasForeignKey(ea => ea.EmployeesExternalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ea => ea.Address)
            .WithMany(a => a.EmployeeAddresses)
            .HasForeignKey(ea => ea.AddressesId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
