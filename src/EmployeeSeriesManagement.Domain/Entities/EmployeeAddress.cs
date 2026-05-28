namespace EmployeeSeriesManagement.Domain.Entities;

/// <summary>
/// Many-to-many link between employees and addresses, mapped to <c>EmployeesAddresses</c>.
/// </summary>
public class EmployeeAddress
{
    /// <summary>Composite key part; foreign key to <see cref="Employee"/>.</summary>
    public int EmployeesExternalId { get; set; }

    /// <summary>Composite key part; foreign key to <see cref="Address"/>.</summary>
    public int AddressesId { get; set; }

    /// <summary>Linked employee.</summary>
    public Employee Employee { get; set; } = null!;

    /// <summary>Linked address.</summary>
    public Address Address { get; set; } = null!;
}
