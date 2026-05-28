using EmployeeSeriesManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeSeriesManagement.Infrastructure.Data.Configurations;

/// <summary>
/// EF mapping for <see cref="Series"/> → <c>Series</c>.
/// </summary>
public class SeriesConfiguration : IEntityTypeConfiguration<Series>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Series> builder)
    {
        builder.ToTable("Series");
        builder.HasKey(s => s.Code);
        builder.Property(s => s.Code).ValueGeneratedNever();
        builder.Property(s => s.Name).HasMaxLength(255).IsRequired();
    }
}
