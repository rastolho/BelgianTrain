namespace EmployeeSeriesManagement.Application.Exceptions;

/// <summary>
/// Thrown when an employee–series assignment with the same composite key already exists.
/// </summary>
public sealed class DuplicateAssignmentException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateAssignmentException"/> class.
    /// </summary>
    /// <param name="employeeExternalId">Employee business key.</param>
    /// <param name="seriesCode">Series primary key.</param>
    /// <param name="startDate">Assignment start date (part of the composite key).</param>
    public DuplicateAssignmentException(int employeeExternalId, int seriesCode, DateOnly startDate)
        : base(
            $"An assignment for employee {employeeExternalId}, series {seriesCode}, starting {startDate:yyyy-MM-dd} already exists.")
    {
        EmployeeExternalId = employeeExternalId;
        SeriesCode = seriesCode;
        StartDate = startDate;
    }

    /// <summary>Employee business key.</summary>
    public int EmployeeExternalId { get; }

    /// <summary>Series primary key.</summary>
    public int SeriesCode { get; }

    /// <summary>Assignment start date (composite key component).</summary>
    public DateOnly StartDate { get; }
}
