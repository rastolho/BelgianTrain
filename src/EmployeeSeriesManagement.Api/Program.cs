using EmployeeSeriesManagement.Application;
using EmployeeSeriesManagement.Application.Diagnostics;
using EmployeeSeriesManagement.Infrastructure;
using EmployeeSeriesManagement.Infrastructure.Data;
using EmployeeSeriesManagement.Infrastructure.Data.Seed;
using EmployeeSeriesManagement.Infrastructure.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "EmployeeSeriesManagement.Api";
var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";
var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
var enableConsoleExporter = builder.Configuration.GetValue("OpenTelemetry:EnableConsoleExporter", builder.Environment.IsDevelopment());

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        .AddAttributes(new KeyValuePair<string, object>[]
        {
            new("deployment.environment", builder.Environment.EnvironmentName)
        }))
    .WithTracing(tracing =>
    {
        tracing
            .AddSource(ApplicationTelemetry.SourceName)
            .AddSource(InfrastructureTelemetry.SourceName)
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.Filter = context =>
                    !context.Request.Path.StartsWithSegments("/health") &&
                    !context.Request.Path.StartsWithSegments("/openapi");
            })
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddSqlClientInstrumentation(options =>
            {
                options.RecordException = true;
            });

        if (enableConsoleExporter)
        {
            tracing.AddConsoleExporter();
        }

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter(otlp =>
            {
                otlp.Endpoint = new Uri(otlpEndpoint);
                otlp.Protocol = OtlpExportProtocol.Grpc;
            });
        }
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7231")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EmployeeSeriesDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    if (builder.Configuration.GetValue("Database:UseEnsureCreated", false))
        await context.Database.EnsureCreatedAsync();
    else
        await context.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(context, logger);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("BlazorClient");
app.MapControllers();

app.Run();

public partial class Program;
