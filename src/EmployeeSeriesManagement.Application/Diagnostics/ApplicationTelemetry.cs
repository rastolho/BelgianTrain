using System.Diagnostics;
using System.Reflection;

namespace EmployeeSeriesManagement.Application.Diagnostics;

/// <summary>
/// OpenTelemetry instrumentation surface for the Application layer.
/// Use <see cref="ActivitySource"/> to emit spans from application services.
/// </summary>
public static class ApplicationTelemetry
{
    /// <summary>Assembly-level activity source name (also exposed as the OTel instrumentation name).</summary>
    public static readonly string SourceName = typeof(ApplicationTelemetry).Assembly.GetName().Name!;

    /// <summary>Assembly version used as the activity source version.</summary>
    public static readonly string Version =
        typeof(ApplicationTelemetry).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(ApplicationTelemetry).Assembly.GetName().Version?.ToString()
        ?? "unknown";

    /// <summary>Activity source for application services. Listeners must subscribe to <see cref="SourceName"/>.</summary>
    public static readonly ActivitySource ActivitySource = new(SourceName, Version);
}

/// <summary>
/// Span attribute names (tags) used by the Application layer. Centralised here so callers don't drift.
/// </summary>
public static class TelemetryTags
{
    /// <summary>Employee external id (business key).</summary>
    public const string EmployeeExternalId = "app.employee.external_id";

    /// <summary>Series primary-key code.</summary>
    public const string SeriesCode = "app.series.code";

    /// <summary>Work-city filter value (normalised).</summary>
    public const string WorkCity = "app.work_city";

    /// <summary>Inclusive period start.</summary>
    public const string PeriodStart = "app.period.start";

    /// <summary>Inclusive period end.</summary>
    public const string PeriodEnd = "app.period.end";

    /// <summary>Number of rows returned by a query.</summary>
    public const string ResultCount = "app.result.count";

    /// <summary>Outcome of an operation (e.g. ok, not_found, conflict, validation_error).</summary>
    public const string Outcome = "app.outcome";
}
