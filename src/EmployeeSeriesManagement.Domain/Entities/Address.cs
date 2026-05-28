namespace EmployeeSeriesManagement.Domain.Entities;

/// <summary>
/// Physical address record mapped to the <c>Addresses</c> table.
/// </summary>
public class Address
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Foreign key to <see cref="AddressType"/>.</summary>
    public int AddressTypeId { get; set; }

    /// <summary>Country name.</summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>City name.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Postal or ZIP code.</summary>
    public string ZipCode { get; set; } = string.Empty;

    /// <summary>Street name.</summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>Street number.</summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>Optional mailbox number.</summary>
    public string? MailboxNumber { get; set; }

    /// <summary>Optional building name or identifier.</summary>
    public string? Building { get; set; }

    /// <summary>Optional floor.</summary>
    public string? Floor { get; set; }

    /// <summary>Address classification (Personal or Work).</summary>
    public AddressType AddressType { get; set; } = null!;

    /// <summary>Employee links sharing this address (<c>EmployeesAddresses</c>).</summary>
    public ICollection<EmployeeAddress> EmployeeAddresses { get; set; } = new List<EmployeeAddress>();
}
