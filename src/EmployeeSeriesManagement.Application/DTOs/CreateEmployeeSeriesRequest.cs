namespace EmployeeSeriesManagement.Application.DTOs;

/// <summary>
/// Payload for assigning a series to an employee over an inclusive date range.
/// </summary>
/// <param name="EmployeeExternalId">Employee business key (<c>Employees.ExternalId</c>).</param>
/// <param name="SeriesCode">Series identifier (<c>Series.Code</c>).</param>
/// <param name="StartDate">Assignment start date (inclusive).</param>
/// <param name="EndDate">Assignment end date (inclusive).</param>
public record CreateEmployeeSeriesRequest(
    int EmployeeExternalId,
    int SeriesCode,
    DateOnly StartDate,
    DateOnly EndDate);
