namespace EmployeeSeriesManagement.Domain.Entities;

/// <summary>
/// Address classification lookup mapped to the <c>AddressType</c> table (e.g. Personal, Work).
/// </summary>
public class AddressType
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Human-readable type description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Addresses classified with this type.</summary>
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}
