using System.Diagnostics;
using EmployeeSeriesManagement.Application.Diagnostics;
using EmployeeSeriesManagement.Application.DTOs;
using EmployeeSeriesManagement.Application.Exceptions;
using EmployeeSeriesManagement.Application.Interfaces;
using EmployeeSeriesManagement.Application.Validation;
using EmployeeSeriesManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EmployeeSeriesManagement.Application.Services;

/// <summary>
/// Application service for employee addresses, work-city lookups, and series assignments.
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly ILogger<EmployeeService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeService"/> class.
    /// </summary>
    /// <param name="repository">Employee persistence abstraction.</param>
    /// <param name="logger">Structured logger.</param>
    public EmployeeService(IEmployeeRepository repository, ILogger<EmployeeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AddressDto>> GetEmployeeAddressesAsync(
        int employeeExternalId,
        CancellationToken cancellationToken = default)
    {
        using var activity = ApplicationTelemetry.ActivitySource.StartActivity(
            "EmployeeService.GetEmployeeAddresses",
            ActivityKind.Internal);
        activity?.SetTag(TelemetryTags.EmployeeExternalId, employeeExternalId);

        _logger.LogDebug(
            "Starting {Operation} for employee {EmployeeExternalId}",
            nameof(GetEmployeeAddressesAsync),
            employeeExternalId);

        if (!await _repository.EmployeeExistsAsync(employeeExternalId, cancellationToken))
        {
            activity?.SetTag(TelemetryTags.Outcome, "not_found");
            activity?.SetStatus(ActivityStatusCode.Error, "Employee not found");
            throw new KeyNotFoundException($"Employee with external id {employeeExternalId} was not found.");
        }

        var addresses = await _repository.GetEmployeeAddressesAsync(employeeExternalId, cancellationToken);
        var result = addresses.Select(MapAddress).ToList();

        activity?.SetTag(TelemetryTags.ResultCount, result.Count);
        activity?.SetTag(TelemetryTags.Outcome, "ok");
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EmployeePersonalAddressDto>> GetPersonalAddressesByWorkCityAsync(
        string workCity,
        CancellationToken cancellationToken = default)
    {
        using var activity = ApplicationTelemetry.ActivitySource.StartActivity(
            "EmployeeService.GetPersonalAddressesByWorkCity",
            ActivityKind.Internal);

        if (string.IsNullOrWhiteSpace(workCity))
        {
            activity?.SetTag(TelemetryTags.Outcome, "validation_error");
            activity?.SetStatus(ActivityStatusCode.Error, "workCity is required");
            throw new ArgumentException("Work city is required.", nameof(workCity));
        }

        var normalizedWorkCity = workCity.Trim();
        activity?.SetTag(TelemetryTags.WorkCity, normalizedWorkCity);

        _logger.LogDebug(
            "Starting {Operation} for work city {WorkCity}",
            nameof(GetPersonalAddressesByWorkCityAsync),
            normalizedWorkCity);

        var result = await _repository.GetPersonalAddressesByWorkCityAsync(normalizedWorkCity, cancellationToken);
        activity?.SetTag(TelemetryTags.ResultCount, result.Count);
        activity?.SetTag(TelemetryTags.Outcome, "ok");
        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetWorkCitiesAsync(CancellationToken cancellationToken = default)
    {
        using var activity = ApplicationTelemetry.ActivitySource.StartActivity(
            "EmployeeService.GetWorkCities",
            ActivityKind.Internal);

        _logger.LogDebug("Starting {Operation}", nameof(GetWorkCitiesAsync));

        var cities = await _repository.GetDistinctWorkCitiesAsync(cancellationToken);
        activity?.SetTag(TelemetryTags.ResultCount, cities.Count);
        activity?.SetTag(TelemetryTags.Outcome, "ok");
        return cities;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SeriesDto>> GetEmployeeSeriesForPeriodAsync(
        int employeeExternalId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default)
    {
        using var activity = ApplicationTelemetry.ActivitySource.StartActivity(
            "EmployeeService.GetEmployeeSeriesForPeriod",
            ActivityKind.Internal);
        activity?.SetTag(TelemetryTags.EmployeeExternalId, employeeExternalId);
        activity?.SetTag(TelemetryTags.PeriodStart, periodStart.ToString("yyyy-MM-dd"));
        activity?.SetTag(TelemetryTags.PeriodEnd, periodEnd.ToString("yyyy-MM-dd"));

        if (periodEnd < periodStart)
        {
            activity?.SetTag(TelemetryTags.Outcome, "validation_error");
            activity?.SetStatus(ActivityStatusCode.Error, "Period end before period start");
            throw new ArgumentException("Period end must be on or after period start.");
        }

        _logger.LogDebug(
            "Starting {Operation} for employee {EmployeeExternalId} between {PeriodStart} and {PeriodEnd}",
            nameof(GetEmployeeSeriesForPeriodAsync),
            employeeExternalId,
            periodStart,
            periodEnd);

        if (!await _repository.EmployeeExistsAsync(employeeExternalId, cancellationToken))
        {
            activity?.SetTag(TelemetryTags.Outcome, "not_found");
            activity?.SetStatus(ActivityStatusCode.Error, "Employee not found");
            throw new KeyNotFoundException($"Employee with external id {employeeExternalId} was not found.");
        }

        var assignments = await _repository.GetEmployeeSeriesInPeriodAsync(
            employeeExternalId,
            periodStart,
            periodEnd,
            cancellationToken);

        var result = assignments
            .Select(es => new SeriesDto(
                es.Series.Code,
                es.Series.ExternalId,
                es.Series.Name,
                es.Series.StartDate,
                es.Series.EndDate,
                es.StartDate,
                es.EndDate))
            .ToList();

        activity?.SetTag(TelemetryTags.ResultCount, result.Count);
        activity?.SetTag(TelemetryTags.Outcome, "ok");
        return result;
    }

    /// <inheritdoc />
    public async Task<SeriesDto> AssignSeriesToEmployeeAsync(
        CreateEmployeeSeriesRequest request,
        CancellationToken cancellationToken = default)
    {
        using var activity = ApplicationTelemetry.ActivitySource.StartActivity(
            "EmployeeService.AssignSeriesToEmployee",
            ActivityKind.Internal);
        activity?.SetTag(TelemetryTags.EmployeeExternalId, request.EmployeeExternalId);
        activity?.SetTag(TelemetryTags.SeriesCode, request.SeriesCode);
        activity?.SetTag(TelemetryTags.PeriodStart, request.StartDate.ToString("yyyy-MM-dd"));
        activity?.SetTag(TelemetryTags.PeriodEnd, request.EndDate.ToString("yyyy-MM-dd"));

        _logger.LogDebug(
            "Starting {Operation} for employee {EmployeeExternalId} and series code {SeriesCode}",
            nameof(AssignSeriesToEmployeeAsync),
            request.EmployeeExternalId,
            request.SeriesCode);

        var validationErrors = CreateEmployeeSeriesValidator.Validate(request);
        if (validationErrors.Count > 0)
        {
            activity?.SetTag(TelemetryTags.Outcome, "validation_error");
            activity?.SetStatus(ActivityStatusCode.Error, string.Join(' ', validationErrors));
            throw new ArgumentException(string.Join(' ', validationErrors));
        }

        if (!await _repository.EmployeeExistsAsync(request.EmployeeExternalId, cancellationToken))
        {
            activity?.SetTag(TelemetryTags.Outcome, "not_found");
            activity?.SetStatus(ActivityStatusCode.Error, "Employee not found");
            throw new KeyNotFoundException($"Employee with external id {request.EmployeeExternalId} was not found.");
        }

        if (!await _repository.SeriesExistsAsync(request.SeriesCode, cancellationToken))
        {
            activity?.SetTag(TelemetryTags.Outcome, "not_found");
            activity?.SetStatus(ActivityStatusCode.Error, "Series not found");
            throw new KeyNotFoundException($"Series with code {request.SeriesCode} was not found.");
        }

        var existingAssignments = await _repository.GetEmployeeSeriesInPeriodAsync(
            request.EmployeeExternalId,
            request.StartDate,
            request.StartDate,
            cancellationToken);

        if (existingAssignments.Any(es =>
                es.SeriesCode == request.SeriesCode && es.StartDate == request.StartDate))
        {
            activity?.SetTag(TelemetryTags.Outcome, "conflict");
            activity?.SetStatus(ActivityStatusCode.Error, "Duplicate assignment");
            throw new DuplicateAssignmentException(
                request.EmployeeExternalId,
                request.SeriesCode,
                request.StartDate);
        }

        var assignment = new EmployeeSeries
        {
            EmployeesExternalId = request.EmployeeExternalId,
            SeriesCode = request.SeriesCode,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        await _repository.AddEmployeeSeriesAsync(assignment, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Assigned series {SeriesCode} to employee {EmployeeExternalId} from {StartDate} to {EndDate}",
            request.SeriesCode,
            request.EmployeeExternalId,
            request.StartDate,
            request.EndDate);

        var saved = (await _repository.GetEmployeeSeriesInPeriodAsync(
            request.EmployeeExternalId,
            request.StartDate,
            request.EndDate,
            cancellationToken))
            .First(es => es.SeriesCode == request.SeriesCode && es.StartDate == request.StartDate);

        activity?.SetTag(TelemetryTags.Outcome, "created");
        return new SeriesDto(
            saved.Series.Code,
            saved.Series.ExternalId,
            saved.Series.Name,
            saved.Series.StartDate,
            saved.Series.EndDate,
            saved.StartDate,
            saved.EndDate);
    }

    private static AddressDto MapAddress(Address address) =>
        new(
            address.Id,
            address.AddressType.Description,
            address.Country,
            address.City,
            address.ZipCode,
            address.Street,
            address.Number,
            address.MailboxNumber,
            address.Building,
            address.Floor);
}
