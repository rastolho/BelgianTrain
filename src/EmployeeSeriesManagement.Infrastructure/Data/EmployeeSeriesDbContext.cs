using EmployeeSeriesManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeSeriesManagement.Infrastructure.Data;

/// <summary>
/// EF Core context for the standalone ESM schema (<c>ESM_Standalone</c>).
/// </summary>
public class EmployeeSeriesDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeSeriesDbContext"/> class.
    /// </summary>
    /// <param name="options">Context options including the database connection string.</param>
    public EmployeeSeriesDbContext(DbContextOptions<EmployeeSeriesDbContext> options)
        : base(options)
    {
    }

    /// <summary>Employee master records (<c>Employees</c>).</summary>
    public DbSet<Employee> Employees => Set<Employee>();

    /// <summary>Address rows (<c>Addresses</c>).</summary>
    public DbSet<Address> Addresses => Set<Address>();

    /// <summary>Address type lookup (<c>AddressType</c>).</summary>
    public DbSet<AddressType> AddressTypes => Set<AddressType>();

    /// <summary>Employee–address junction (<c>EmployeesAddresses</c>).</summary>
    public DbSet<EmployeeAddress> EmployeesAddresses => Set<EmployeeAddress>();

    /// <summary>Series definitions (<c>Series</c>).</summary>
    public DbSet<Series> Series => Set<Series>();

    /// <summary>Employee–series assignments (<c>EmployeesSeries</c>).</summary>
    public DbSet<EmployeeSeries> EmployeesSeries => Set<EmployeeSeries>();

    /// <summary>Employee ID cards (<c>EmployeesIdCards</c>).</summary>
    public DbSet<EmployeeIdCard> EmployeesIdCards => Set<EmployeeIdCard>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmployeeSeriesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
