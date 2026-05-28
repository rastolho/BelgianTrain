namespace EmployeeSeriesManagement.Application.DTOs;

/// <summary>
/// Postal address details exposed by the application layer.
/// </summary>
/// <param name="Id">Persistent address identifier.</param>
/// <param name="AddressType">Human-readable address type (for example, Personal or Work).</param>
/// <param name="Country">Country name or code.</param>
/// <param name="City">City or locality.</param>
/// <param name="ZipCode">Postal or ZIP code.</param>
/// <param name="Street">Street name.</param>
/// <param name="Number">Street number.</param>
/// <param name="MailboxNumber">Optional mailbox or postbox number.</param>
/// <param name="Building">Optional building name or identifier.</param>
/// <param name="Floor">Optional floor designation.</param>
public record AddressDto(
    int Id,
    string AddressType,
    string Country,
    string City,
    string ZipCode,
    string Street,
    string Number,
    string? MailboxNumber,
    string? Building,
    string? Floor);
