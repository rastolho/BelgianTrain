using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MsSql;

namespace EmployeeSeriesManagement.Tests.Api;

/// <summary>
/// <see cref="WebApplicationFactory{TEntryPoint}"/> backed by a Testcontainers MS SQL Server instance.
/// Shared across API test classes through <see cref="EmployeesApiCollection"/> so the container
/// (and its seed) is initialized once per test run.
/// </summary>
public sealed class EmployeesApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public Task InitializeAsync() => _container.StartAsync();

    public new async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", _container.GetConnectionString());
        builder.UseSetting("Database:UseEnsureCreated", "true");
        // No collector is running during tests; disable OTLP export so the OTel SDK
        // doesn't spam connection errors against localhost:4317.
        builder.UseSetting("OpenTelemetry:OtlpEndpoint", string.Empty);
        builder.UseSetting("OpenTelemetry:EnableConsoleExporter", "false");
        builder.UseEnvironment("Development");
    }
}

[CollectionDefinition(Name)]
public sealed class EmployeesApiCollection : ICollectionFixture<EmployeesApiFactory>
{
    public const string Name = "EmployeesApi";
}
