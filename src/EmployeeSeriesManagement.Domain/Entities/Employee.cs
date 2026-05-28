using EmployeeSeriesManagement.Domain.Enums;

namespace EmployeeSeriesManagement.Domain.Entities;

/// <summary>
/// SNCB employee master record mapped to the <c>Employees</c> table.
/// </summary>
public class Employee
{
    /// <summary>Primary key; external employee identifier.</summary>
    public int ExternalId { get; set; }

    /// <summary>User identifier (max 10 characters).</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Internal employee number.</summary>
    public int Number { get; set; }

    /// <summary>Given name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Family name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Optional middle or second name.</summary>
    public string? SecondName { get; set; }

    /// <summary>Preferred communication language (persisted as a 2-char string).</summary>
    public EmployeeLanguage Language { get; set; }

    /// <summary>Date of birth.</summary>
    public DateOnly BirthDate { get; set; }

    /// <summary>Place of birth.</summary>
    public string BirthPlace { get; set; } = string.Empty;

    /// <summary>Profile image binary payload.</summary>
    public byte[]? ProfileImage { get; set; }

    /// <summary>ISO nationality code (3 characters).</summary>
    public string Nationality { get; set; } = string.Empty;

    /// <summary>Employment exit date, when applicable.</summary>
    public DateOnly? ExitDate { get; set; }

    /// <summary>Work email address.</summary>
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>Organizational unit identifier.</summary>
    public int OrganizationalUnit { get; set; }

    /// <summary>Contact phone number.</summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Address links for this employee (<c>EmployeesAddresses</c>).</summary>
    public ICollection<EmployeeAddress> EmployeeAddresses { get; set; } = new List<EmployeeAddress>();

    /// <summary>Series assignments for this employee (<c>EmployeesSeries</c>).</summary>
    public ICollection<EmployeeSeries> EmployeeSeries { get; set; } = new List<EmployeeSeries>();

    /// <summary>Identification cards held by this employee (<c>EmployeesIdCards</c>).</summary>
    public ICollection<EmployeeIdCard> IdCards { get; set; } = new List<EmployeeIdCard>();
}
