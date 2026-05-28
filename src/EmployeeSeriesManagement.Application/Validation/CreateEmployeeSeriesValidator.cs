using EmployeeSeriesManagement.Application.DTOs;

namespace EmployeeSeriesManagement.Application.Validation;

/// <summary>
/// Validates <see cref="CreateEmployeeSeriesRequest"/> before persisting a new assignment.
/// </summary>
public static class CreateEmployeeSeriesValidator
{
    /// <summary>
    /// Validates the request and returns human-readable error messages (empty when valid).
    /// </summary>
    /// <param name="request">Assignment payload to validate.</param>
    /// <returns>Validation errors; empty when the request is valid.</returns>
    public static IReadOnlyList<string> Validate(CreateEmployeeSeriesRequest request)
    {
        var errors = new List<string>();

        if (request.EmployeeExternalId <= 0)
        {
            errors.Add("EmployeeExternalId must be a positive identifier.");
        }

        if (request.SeriesCode <= 0)
        {
            errors.Add("SeriesCode must be a positive identifier.");
        }

        if (request.EndDate < request.StartDate)
        {
            errors.Add("EndDate must be on or after StartDate.");
        }

        return errors;
    }
}
