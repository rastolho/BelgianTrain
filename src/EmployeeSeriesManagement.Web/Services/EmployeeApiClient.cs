using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using EmployeeSeriesManagement.Application.DTOs;
using Microsoft.Extensions.Configuration;

namespace EmployeeSeriesManagement.Web.Services;

/// <summary>
/// HTTP client for employee REST endpoints. Uses <see cref="HttpClient.BaseAddress"/>
/// from <c>ApiBaseUrl</c> in <c>wwwroot/appsettings.json</c> (see Web <c>Program.cs</c>).
/// </summary>
public class EmployeeApiClient
{
    private readonly HttpClient _httpClient;

    public EmployeeApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        if (_httpClient.BaseAddress is null)
        {
            var apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7280/";
            _httpClient.BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute);
        }
    }

    public async Task<IReadOnlyList<string>> GetWorkCitiesAsync(CancellationToken cancellationToken = default)
    {
        var cities = await _httpClient.GetFromJsonAsync<List<string>>("api/employees/work-cities", cancellationToken);
        return cities ?? [];
    }

    public async Task<IReadOnlyList<EmployeePersonalAddressDto>> GetPersonalAddressesByWorkCityAsync(
        string workCity,
        CancellationToken cancellationToken = default)
    {
        var encodedCity = Uri.EscapeDataString(workCity);
        var results = await _httpClient.GetFromJsonAsync<List<EmployeePersonalAddressDto>>(
            $"api/employees/personal-addresses?workCity={encodedCity}",
            cancellationToken);
        return results ?? [];
    }

    public async Task<EmployeeAddressesResult> GetEmployeeAddressesAsync(
        int employeeExternalId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"api/employees/{employeeExternalId}/addresses",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return EmployeeAddressesResult.NotFound();
        }

        response.EnsureSuccessStatusCode();
        var addresses = await response.Content.ReadFromJsonAsync<List<AddressDto>>(cancellationToken: cancellationToken);
        return EmployeeAddressesResult.Ok(addresses ?? []);
    }

    public async Task<EmployeeSeriesResult> GetEmployeeSeriesAsync(
        int employeeExternalId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        var start = startDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var end = endDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        using var response = await _httpClient.GetAsync(
            $"api/employees/{employeeExternalId}/series?startDate={start}&endDate={end}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return EmployeeSeriesResult.NotFound();
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var message = await ReadErrorMessageAsync(response, cancellationToken);
            return EmployeeSeriesResult.BadRequest(message);
        }

        response.EnsureSuccessStatusCode();
        var series = await response.Content.ReadFromJsonAsync<List<SeriesDto>>(cancellationToken: cancellationToken);
        return EmployeeSeriesResult.Ok(series ?? []);
    }

    public async Task<AssignSeriesResult> AssignSeriesAsync(
        int employeeExternalId,
        int seriesCode,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        var body = new AssignSeriesBody(seriesCode, startDate, endDate);
        using var response = await _httpClient.PostAsJsonAsync(
            $"api/employees/{employeeExternalId}/series",
            body,
            cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var created = await response.Content.ReadFromJsonAsync<SeriesDto>(cancellationToken: cancellationToken);
            return AssignSeriesResult.Ok(created!);
        }

        var message = await ReadErrorMessageAsync(response, cancellationToken);
        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => AssignSeriesResult.NotFound(message),
            HttpStatusCode.Conflict => AssignSeriesResult.Conflict(message),
            HttpStatusCode.BadRequest => AssignSeriesResult.BadRequest(message),
            _ => AssignSeriesResult.Error($"Unexpected response: {(int)response.StatusCode} {response.ReasonPhrase}. {message}")
        };
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ErrorBody>(cancellationToken: cancellationToken);
            if (!string.IsNullOrWhiteSpace(problem?.Message))
            {
                return problem.Message!;
            }
        }
        catch
        {
            // Fall through to raw body
        }

        try
        {
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch
        {
            return string.Empty;
        }
    }

    private sealed record AssignSeriesBody(int SeriesCode, DateOnly StartDate, DateOnly EndDate);

    private sealed record ErrorBody(string? Message);
}

public sealed record EmployeeAddressesResult(bool Found, IReadOnlyList<AddressDto> Addresses)
{
    public static EmployeeAddressesResult Ok(IReadOnlyList<AddressDto> addresses) => new(true, addresses);
    public static EmployeeAddressesResult NotFound() => new(false, []);
}

public sealed record EmployeeSeriesResult(
    EmployeeSeriesStatus Status,
    IReadOnlyList<SeriesDto> Series,
    string? ErrorMessage)
{
    public static EmployeeSeriesResult Ok(IReadOnlyList<SeriesDto> series) => new(EmployeeSeriesStatus.Ok, series, null);
    public static EmployeeSeriesResult NotFound() => new(EmployeeSeriesStatus.NotFound, [], null);
    public static EmployeeSeriesResult BadRequest(string message) => new(EmployeeSeriesStatus.BadRequest, [], message);
}

public enum EmployeeSeriesStatus
{
    Ok,
    NotFound,
    BadRequest
}

public sealed record AssignSeriesResult(AssignSeriesStatus Status, SeriesDto? Created, string? ErrorMessage)
{
    public static AssignSeriesResult Ok(SeriesDto created) => new(AssignSeriesStatus.Created, created, null);
    public static AssignSeriesResult BadRequest(string message) => new(AssignSeriesStatus.BadRequest, null, message);
    public static AssignSeriesResult NotFound(string message) => new(AssignSeriesStatus.NotFound, null, message);
    public static AssignSeriesResult Conflict(string message) => new(AssignSeriesStatus.Conflict, null, message);
    public static AssignSeriesResult Error(string message) => new(AssignSeriesStatus.Error, null, message);
}

public enum AssignSeriesStatus
{
    Created,
    BadRequest,
    NotFound,
    Conflict,
    Error
}
