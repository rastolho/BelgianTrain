using EmployeeSeriesManagement.Domain.Entities;
using EmployeeSeriesManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeSeriesManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF mapping for <see cref="Employee"/> → <c>Employees</c>.
/// </summary>
public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.ExternalId);
        builder.Property(e => e.ExternalId).ValueGeneratedNever();

        builder.Property(e => e.UserId).HasMaxLength(10).IsRequired();
        builder.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.SecondName).HasColumnType("TEXT");
        builder.Property(e => e.Language)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<EmployeeLanguage>(v))
            .HasMaxLength(2)
            .IsRequired();
        builder.Property(e => e.BirthPlace).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Nationality).HasMaxLength(3).IsRequired();
        builder.Property(e => e.EmailAddress).HasMaxLength(250).IsRequired();
        builder.Property(e => e.PhoneNumber).HasMaxLength(30).IsRequired();
    }
}
