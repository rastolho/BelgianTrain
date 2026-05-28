namespace EmployeeSeriesManagement.Domain.Enums;

/// <summary>
/// Supported employee communication languages in the SNCB context.
/// Persisted as a two-character string column on <c>Employees.Language</c>.
/// </summary>
public enum EmployeeLanguage
{
    /// <summary>French.</summary>
    FR,

    /// <summary>Dutch.</summary>
    NL
}
