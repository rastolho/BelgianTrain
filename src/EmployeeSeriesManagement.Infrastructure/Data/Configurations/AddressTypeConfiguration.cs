using EmployeeSeriesManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeSeriesManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF mapping for <see cref="AddressType"/> → <c>AddressType</c>.
/// </summary>
public class AddressTypeConfiguration : IEntityTypeConfiguration<AddressType>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AddressType> builder)
    {
        builder.ToTable("AddressType");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Description).HasMaxLength(10).IsRequired();
    }
}
