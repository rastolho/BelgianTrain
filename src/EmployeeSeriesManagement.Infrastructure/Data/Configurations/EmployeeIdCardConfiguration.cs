using EmployeeSeriesManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeSeriesManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF mapping for <see cref="EmployeeIdCard"/> → <c>EmployeesIdCards</c>.
/// </summary>
public class EmployeeIdCardConfiguration : IEntityTypeConfiguration<EmployeeIdCard>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EmployeeIdCard> builder)
    {
        builder.ToTable("EmployeesIdCards");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Number).HasMaxLength(250).IsRequired();

        builder.HasIndex(c => c.EmployeesExternalId);

        builder.HasOne(c => c.Employee)
            .WithMany(e => e.IdCards)
            .HasForeignKey(c => c.EmployeesExternalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
