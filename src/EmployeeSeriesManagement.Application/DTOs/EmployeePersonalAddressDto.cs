namespace EmployeeSeriesManagement.Application.DTOs;

/// <summary>
/// Employee identity with their personal address, used for work-city lookup results.
/// </summary>
/// <param name="EmployeeExternalId">Employee business key (<c>Employees.ExternalId</c>).</param>
/// <param name="FirstName">Employee given name.</param>
/// <param name="LastName">Employee family name.</param>
/// <param name="PersonalAddress">The employee's personal address.</param>
public record EmployeePersonalAddressDto(
    int EmployeeExternalId,
    string FirstName,
    string LastName,
    AddressDto PersonalAddress);
