namespace EmployeeSeriesManagement.Application.DTOs;

/// <summary>
/// Series metadata combined with an employee's assignment window.
/// </summary>
/// <param name="Code">Series identifier (<c>Series.Code</c>).</param>
/// <param name="ExternalId">Series business key (<c>Series.ExternalId</c>).</param>
/// <param name="Name">Series display name.</param>
/// <param name="SeriesStartDate">Overall series validity start (inclusive).</param>
/// <param name="SeriesEndDate">Overall series validity end (inclusive).</param>
/// <param name="AssignmentStartDate">Employee assignment start date (inclusive).</param>
/// <param name="AssignmentEndDate">Employee assignment end date (inclusive).</param>
public record SeriesDto(
    int Code,
    int ExternalId,
    string Name,
    DateOnly SeriesStartDate,
    DateOnly SeriesEndDate,
    DateOnly AssignmentStartDate,
    DateOnly AssignmentEndDate);
