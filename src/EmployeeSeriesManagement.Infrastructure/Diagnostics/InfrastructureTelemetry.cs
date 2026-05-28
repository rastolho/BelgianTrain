using System.Diagnostics;
using System.Reflection;

namespace EmployeeSeriesManagement.Infrastructure.Diagnostics;

/// <summary>
/// OpenTelemetry instrumentation surface for the Infrastructure layer (EF Core repositories).
/// </summary>
public static class InfrastructureTelemetry
{
    /// <summary>Assembly-level activity source name.</summary>
    public static readonly string SourceName = typeof(InfrastructureTelemetry).Assembly.GetName().Name!;

    /// <summary>Assembly version used as the activity source version.</summary>
    public static readonly string Version =
        typeof(InfrastructureTelemetry).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(InfrastructureTelemetry).Assembly.GetName().Version?.ToString()
        ?? "unknown";

    /// <summary>Activity source for repositories. Listeners must subscribe to <see cref="SourceName"/>.</summary>
    public static readonly ActivitySource ActivitySource = new(SourceName, Version);
}
