using EmployeeSeriesManagement.Application.DTOs;
using EmployeeSeriesManagement.Domain.Entities;

namespace EmployeeSeriesManagement.Application.Interfaces;

/// <summary>
/// Persistence abstraction for employee addresses, work-city queries, and series assignments.
/// </summary>
public interface IEmployeeRepository
{
    /// <summary>
    /// Returns all addresses linked to the employee identified by <paramref name="employeeExternalId"/>.
    /// </summary>
    /// <param name="employeeExternalId">Employee business key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<Address>> GetEmployeeAddressesAsync(int employeeExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns personal addresses for employees with a work address in <paramref name="workCity"/>
    /// (trimmed, case-insensitive city match).
    /// </summary>
    /// <param name="workCity">Work address city filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<EmployeePersonalAddressDto>> GetPersonalAddressesByWorkCityAsync(string workCity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns distinct cities from work addresses, ordered alphabetically.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<string>> GetDistinctWorkCitiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns employee–series assignments overlapping the inclusive period
    /// [<paramref name="periodStart"/>, <paramref name="periodEnd"/>].
    /// </summary>
    /// <param name="employeeExternalId">Employee business key.</param>
    /// <param name="periodStart">Period start (inclusive).</param>
    /// <param name="periodEnd">Period end (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<EmployeeSeries>> GetEmployeeSeriesInPeriodAsync(
        int employeeExternalId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether an employee with the given external id exists.
    /// </summary>
    /// <param name="employeeExternalId">Employee business key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> EmployeeExistsAsync(int employeeExternalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether a series with the given code exists.
    /// </summary>
    /// <param name="seriesCode">Series primary key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> SeriesExistsAsync(int seriesCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a new employee–series assignment (call <see cref="SaveChangesAsync"/> to persist).
    /// </summary>
    /// <param name="assignment">Assignment entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddEmployeeSeriesAsync(EmployeeSeries assignment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
