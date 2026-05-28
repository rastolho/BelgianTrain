using System.Net;
using System.Net.Http.Json;
using EmployeeSeriesManagement.Application.DTOs;
using FluentAssertions;

namespace EmployeeSeriesManagement.Tests.Api;

[Collection(EmployeesApiCollection.Name)]
public class AssignSeriesApiTests
{
    private readonly HttpClient _client;

    public AssignSeriesApiTests(EmployeesApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostSeries_NewAssignment_Returns201AndPersists()
    {
        var body = new { SeriesCode = 501, StartDate = "2026-04-15", EndDate = "2026-05-15" };

        var response = await _client.PostAsJsonAsync("/api/Employees/1002/series", body);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<SeriesDto>();
        created.Should().NotBeNull();
        created!.Code.Should().Be(501);
        created.AssignmentStartDate.Should().Be(new DateOnly(2026, 4, 15));
        created.AssignmentEndDate.Should().Be(new DateOnly(2026, 5, 15));

        response.Headers.Location.Should().NotBeNull();

        var verify = await _client.GetAsync(
            "/api/Employees/1002/series?startDate=2026-04-15&endDate=2026-05-15");
        verify.StatusCode.Should().Be(HttpStatusCode.OK);
        var reloaded = await verify.Content.ReadFromJsonAsync<IReadOnlyList<SeriesDto>>();
        reloaded!.Should().Contain(s => s.Code == 501 && s.AssignmentStartDate == new DateOnly(2026, 4, 15));
    }

    [Fact]
    public async Task PostSeries_DuplicateOfSeededAssignment_Returns409()
    {
        var body = new { SeriesCode = 501, StartDate = "2026-06-01", EndDate = "2026-08-31" };

        var response = await _client.PostAsJsonAsync("/api/Employees/1001/series", body);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostSeries_UnknownEmployee_Returns404()
    {
        var body = new { SeriesCode = 501, StartDate = "2026-07-01", EndDate = "2026-07-31" };

        var response = await _client.PostAsJsonAsync("/api/Employees/99999/series", body);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostSeries_UnknownSeries_Returns404()
    {
        var body = new { SeriesCode = 88888, StartDate = "2026-08-01", EndDate = "2026-08-31" };

        var response = await _client.PostAsJsonAsync("/api/Employees/1001/series", body);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostSeries_EndBeforeStart_Returns400()
    {
        var body = new { SeriesCode = 501, StartDate = "2026-09-30", EndDate = "2026-09-01" };

        var response = await _client.PostAsJsonAsync("/api/Employees/1001/series", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostSeries_NegativeSeriesCode_Returns400()
    {
        var body = new { SeriesCode = -1, StartDate = "2026-10-01", EndDate = "2026-10-31" };

        var response = await _client.PostAsJsonAsync("/api/Employees/1001/series", body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
