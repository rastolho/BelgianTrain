using System.Diagnostics;
using EmployeeSeriesManagement.Application.Common;
using EmployeeSeriesManagement.Application.Diagnostics;
using EmployeeSeriesManagement.Application.DTOs;
using EmployeeSeriesManagement.Application.Interfaces;
using EmployeeSeriesManagement.Domain.Entities;
using EmployeeSeriesManagement.Infrastructure.Data;
using EmployeeSeriesManagement.Infrastructure.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EmployeeSeriesManagement.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IEmployeeRepository"/>.
/// </summary>
public class EmployeeRepository : IEmployeeRepository
{
    private readonly EmployeeSeriesDbContext _context;
    private readonly ILogger<EmployeeRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeRepository"/> class.
    /// </summary>
    /// <param name="context">Database context.</param>
    /// <param name="logger">Structured logger.</param>
    public EmployeeRepository(EmployeeSeriesDbContext context, ILogger<EmployeeRepository>? logger = null)
    {
        _context = context;
        _logger = logger ?? NullLogger<EmployeeRepository>.Instance;
    }

    /// <inheritdoc />
    public async Task<bool> EmployeeExistsAsync(int employeeExternalId, CancellationToken cancellationToken = default)
    {
        using var activity = InfrastructureTelemetry.ActivitySource.StartActivity(
            "EmployeeRepository.EmployeeExists",
            ActivityKind.Internal);
        activity?.SetTag(TelemetryTags.EmployeeExternalId, employeeExternalId);

        _logger.LogDebug(
            "Repository {Operation} starting for employee {EmployeeExternalId}",
            nameof(EmployeeExistsAsync),
            employeeExternalId);

        var exists = await _context.Employees.AnyAsync(e => e.ExternalId == employeeExternalId, cancellationToken);
        activity?.SetTag("app.employee.exists", exists);
        return exists;
    }

    /// <inheritdoc />
    public async Task<bool> SeriesExistsAsync(int seriesCode, CancellationToken cancellationToken = default)
    {
        using var activity = InfrastructureTelemetry.ActivitySource.StartActivity(
            "EmployeeRepository.SeriesExists",
            ActivityKind.Internal);
        activity?.SetTag(TelemetryTags.SeriesCode, seriesCode);

        _logger.LogDebug(
            "Repository {Operation} starting for series code {SeriesCode}",
            nameof(SeriesExistsAsync),
            seriesCode);

        var exists = await _context.Series.AnyAsync(s => s.Code == seriesCode, cancellationToken);
        activity?.SetTag("app.series.exists", exists);
        return exists;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Address>> GetEmployeeAddressesAsync(
        int employeeExternalId,
        CancellationToken cancellationToken = default)
    {
        using var activity = InfrastructureTelemetry.ActivitySource.StartActivity(
            "EmployeeRepository.GetEmployeeAddresses",
            ActivityKind.Internal);
        activity?.SetTag(TelemetryTags.EmployeeExternalId, employeeExternalId);

        _logger.LogDebug(
            "Repository {Operation} starting for employee {EmployeeExternalId}",
            nameof(GetEmployeeAddressesAsync),
            employeeExternalId);

        var result = await _context.Addresses
            .AsNoTracking()
            .Include(a => a.AddressType)
            .Where(a => a.EmployeeAddresses.Any(ea => ea.EmployeesExternalId == employeeExternalId))
            .ToListAsync(cancellationToken);

        activity?.SetTag(TelemetryTags.ResultCount, result.Count);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EmployeePersonalAddressDto>> GetPersonalAddressesByWorkCityAsync(
        string workCity,
        CancellationToken cancellationToken = default)
    {
        using var activity = InfrastructureTelemetry.ActivitySource.StartActivity(
            "EmployeeRepository.GetPersonalAddressesByWorkCity",
            ActivityKind.Internal);
        activity?.SetTag(TelemetryTags.WorkCity, workCity);

        _logger.LogDebug(
            "Repository {Operation} starting for work city {WorkCity}",
            nameof(GetPersonalAddressesByWorkCityAsync),
            workCity);

        var normalizedWorkCity = workCity.Trim().ToLower();

        var result = await _context.EmployeesAddresses
            .AsNoTracking()
            .Where(ea =>
                ea.Address.AddressType.Description == AddressTypeNames.Personal &&
                _context.EmployeesAddresses.Any(work =>
                    work.EmployeesExternalId == ea.EmployeesExternalId &&
                    work.Address.AddressType.Description == AddressTypeNames.Work &&
                    work.Address.City.ToLower() == normalizedWorkCity))
            .Select(ea => new EmployeePersonalAddressDto(
                ea.Employee.ExternalId,
                ea.Employee.FirstName,
                ea.Employee.LastName,
                new AddressDto(
                    ea.Address.Id,
                    AddressTypeNames.Personal,
                    ea.Address.Country,
                    ea.Address.City,
                    ea.Address.ZipCode,
                    ea.Address.Street,
                    ea.Address.Number,
                    ea.Address.MailboxNumber,
                    ea.Address.Building,
                    ea.Address.Floor)))
            .ToListAsync(cancellationToken);

        activity?.SetTag(TelemetryTags.ResultCount, result.Count);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetDistinctWorkCitiesAsync(CancellationToken cancellationToken = default)
    {
        using var activity = InfrastructureTelemetry.ActivitySource.StartActivity(
            "EmployeeRepository.GetDistinctWorkCities",
            ActivityKind.Internal);

        _logger.LogDebug("Repository {Operation} starting", nameof(GetDistinctWorkCitiesAsync));

        var result = await _context.Addresses
            .AsNoTracking()
            .Where(a => a.AddressType.Description == AddressTypeNames.Work)
            .Select(a => a.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);

        activity?.SetTag(TelemetryTags.ResultCount, result.Count);
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EmployeeSeries>> GetEmployeeSeriesInPeriodAsync(
        int employeeExternalId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default)
    {
        using var activity = InfrastructureTelemetry.ActivitySource.StartActivity(
            "EmployeeRepository.GetEmployeeSeriesInPeriod",
            ActivityKind.Internal);
        activity?.SetTag(TelemetryTags.EmployeeExternalId, employeeExternalId);
        activity?.SetTag(TelemetryTags.PeriodStart, periodStart.ToString("yyyy-MM-dd"));
        activity?.SetTag(TelemetryTags.PeriodEnd, periodEnd.ToString("yyyy-MM-dd"));

        _logger.LogDebug(
            "Repository {Operation} starting for employee {EmployeeExternalId} between {PeriodStart} and {PeriodEnd}",
            nameof(GetEmployeeSeriesInPeriodAsync),
            employeeExternalId,
            periodStart,
            periodEnd);

        var result = await _context.EmployeesSeries
            .AsNoTracking()
            .Include(es => es.Series)
            .Where(es =>
                es.EmployeesExternalId == employeeExternalId &&
                es.StartDate <= periodEnd &&
                es.EndDate >= periodStart)
            .OrderBy(es => es.StartDate)
            .ToListAsync(cancellationToken);

        activity?.SetTag(TelemetryTags.ResultCount, result.Count);
        return result;
    }

    /// <inheritdoc />
    public Task AddEmployeeSeriesAsync(EmployeeSeries assignment, CancellationToken cancellationToken = default)
    {
        using var activity = InfrastructureTelemetry.ActivitySource.StartActivity(
            "EmployeeRepository.AddEmployeeSeries",
            ActivityKind.Internal);
        activity?.SetTag(TelemetryTags.EmployeeExternalId, assignment.EmployeesExternalId);
        activity?.SetTag(TelemetryTags.SeriesCode, assignment.SeriesCode);

        _logger.LogDebug(
            "Repository {Operation} starting for employee {EmployeeExternalId} and series code {SeriesCode}",
            nameof(AddEmployeeSeriesAsync),
            assignment.EmployeesExternalId,
            assignment.SeriesCode);

        _context.EmployeesSeries.Add(assignment);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        using var activity = InfrastructureTelemetry.ActivitySource.StartActivity(
            "EmployeeRepository.SaveChanges",
            ActivityKind.Internal);

        _logger.LogDebug("Repository {Operation} starting", nameof(SaveChangesAsync));
        return _context.SaveChangesAsync(cancellationToken);
    }
}
