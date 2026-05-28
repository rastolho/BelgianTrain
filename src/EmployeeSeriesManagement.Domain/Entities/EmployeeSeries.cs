namespace EmployeeSeriesManagement.Domain.Entities;

/// <summary>
/// Assignment of a series to an employee with an active date range, mapped to <c>EmployeesSeries</c>.
/// </summary>
public class EmployeeSeries
{
    /// <summary>Composite key part; foreign key to <see cref="Employee"/>.</summary>
    public int EmployeesExternalId { get; set; }

    /// <summary>Composite key part; foreign key to <see cref="Series"/>.</summary>
    public int SeriesCode { get; set; }

    /// <summary>Composite key part; assignment start date.</summary>
    public DateOnly StartDate { get; set; }

    /// <summary>Assignment end date.</summary>
    public DateOnly EndDate { get; set; }

    /// <summary>Assigned employee.</summary>
    public Employee Employee { get; set; } = null!;

    /// <summary>Assigned series.</summary>
    public Series Series { get; set; } = null!;
}
