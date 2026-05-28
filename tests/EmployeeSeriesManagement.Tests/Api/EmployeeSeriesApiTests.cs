using System.Net;
using System.Net.Http.Json;
using EmployeeSeriesManagement.Application.DTOs;
using FluentAssertions;

namespace EmployeeSeriesManagement.Tests.Api;

[Collection(EmployeesApiCollection.Name)]
public class EmployeeSeriesApiTests
{
    private readonly HttpClient _client;

    public EmployeeSeriesApiTests(EmployeesApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSeries_WithinSeededWindow_ReturnsAssignment501()
    {
        var response = await _client.GetAsync(
            "/api/Employees/1001/series?startDate=2026-01-01&endDate=2026-12-31");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var series = await response.Content.ReadFromJsonAsync<IReadOnlyList<SeriesDto>>();
        series!.Should().Contain(s => s.Code == 501);
        series.Should().NotContain(s => s.Code == 502 && s.AssignmentEndDate.Year == 2025);
    }

    [Fact]
    public async Task GetSeries_OutsideSeededWindow_ReturnsEmpty()
    {
        var response = await _client.GetAsync(
            "/api/Employees/1002/series?startDate=2020-01-01&endDate=2020-12-31");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var series = await response.Content.ReadFromJsonAsync<IReadOnlyList<SeriesDto>>();
        series!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSeries_EndBeforeStart_ReturnsBadRequest()
    {
        var response = await _client.GetAsync(
            "/api/Employees/1001/series?startDate=2026-12-31&endDate=2026-01-01");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSeries_UnknownEmployee_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            "/api/Employees/99999/series?startDate=2026-01-01&endDate=2026-12-31");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
