namespace EmployeeSeriesManagement.Domain.Entities;

/// <summary>
/// Repetitive job assignment definition mapped to the <c>Series</c> table.
/// </summary>
public class Series
{
    /// <summary>Primary key; series code.</summary>
    public int Code { get; set; }

    /// <summary>External series identifier.</summary>
    public int ExternalId { get; set; }

    /// <summary>Display name (e.g. route description).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Series validity start date.</summary>
    public DateOnly StartDate { get; set; }

    /// <summary>Series validity end date.</summary>
    public DateOnly EndDate { get; set; }

    /// <summary>Employee assignments for this series (<c>EmployeesSeries</c>).</summary>
    public ICollection<EmployeeSeries> EmployeeSeries { get; set; } = new List<EmployeeSeries>();
}
