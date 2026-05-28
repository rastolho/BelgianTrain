using EmployeeSeriesManagement.Application.DTOs;

namespace EmployeeSeriesManagement.Application.Interfaces;

/// <summary>
/// Application service for employee addresses, work-city lookups, and series assignments.
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Returns all addresses linked to the employee identified by <paramref name="employeeExternalId"/>.
    /// </summary>
    /// <param name="employeeExternalId">Employee business key (<c>Employees.ExternalId</c>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Address DTOs for the employee.</returns>
    /// <exception cref="KeyNotFoundException">Employee does not exist.</exception>
    Task<IReadOnlyList<AddressDto>> GetEmployeeAddressesAsync(int employeeExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns personal addresses for employees who have a work address in the given city.
    /// </summary>
    /// <param name="workCity">Work address city; trimmed and matched case-insensitively.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Employee personal address rows for matching employees.</returns>
    /// <exception cref="ArgumentException"><paramref name="workCity"/> is null or whitespace.</exception>
    Task<IReadOnlyList<EmployeePersonalAddressDto>> GetPersonalAddressesByWorkCityAsync(string workCity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns distinct cities from work addresses, ordered alphabetically.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<string>> GetWorkCitiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns series assignments for an employee that overlap the inclusive period
    /// [<paramref name="periodStart"/>, <paramref name="periodEnd"/>].
    /// </summary>
    /// <param name="employeeExternalId">Employee business key.</param>
    /// <param name="periodStart">Period start (inclusive).</param>
    /// <param name="periodEnd">Period end (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="ArgumentException"><paramref name="periodEnd"/> is before <paramref name="periodStart"/>.</exception>
    /// <exception cref="KeyNotFoundException">Employee does not exist.</exception>
    Task<IReadOnlyList<SeriesDto>> GetEmployeeSeriesForPeriodAsync(
        int employeeExternalId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and persists a new employee–series assignment.
    /// </summary>
    /// <param name="request">Assignment payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created assignment as a <see cref="SeriesDto"/>.</returns>
    /// <exception cref="ArgumentException">Validation failed.</exception>
    /// <exception cref="KeyNotFoundException">Employee or series does not exist.</exception>
    Task<SeriesDto> AssignSeriesToEmployeeAsync(CreateEmployeeSeriesRequest request, CancellationToken cancellationToken = default);
}
