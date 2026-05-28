namespace EmployeeSeriesManagement.Domain.Entities;

/// <summary>
/// Employee identification card mapped to <c>EmployeesIdCards</c> (seeded data only; not exposed via API).
/// </summary>
public class EmployeeIdCard
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Foreign key to <see cref="Employee"/>.</summary>
    public int EmployeesExternalId { get; set; }

    /// <summary>Card number.</summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>Card validity date.</summary>
    public DateOnly Validity { get; set; }

    /// <summary>Card holder.</summary>
    public Employee Employee { get; set; } = null!;
}
