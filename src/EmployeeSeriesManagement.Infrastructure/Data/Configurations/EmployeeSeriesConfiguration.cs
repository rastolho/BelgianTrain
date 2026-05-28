using EmployeeSeriesManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeSeriesManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF mapping for <see cref="EmployeeSeries"/> → <c>EmployeesSeries</c> (composite PK).
/// </summary>
public class EmployeeSeriesConfiguration : IEntityTypeConfiguration<EmployeeSeries>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<EmployeeSeries> builder)
    {
        builder.ToTable("EmployeesSeries");
        builder.HasKey(es => new { es.EmployeesExternalId, es.SeriesCode, es.StartDate });

        builder.HasOne(es => es.Employee)
            .WithMany(e => e.EmployeeSeries)
            .HasForeignKey(es => es.EmployeesExternalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(es => es.Series)
            .WithMany(s => s.EmployeeSeries)
            .HasForeignKey(es => es.SeriesCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
