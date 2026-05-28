using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace EmployeeSeriesManagement.Tests.Api;

[Collection(EmployeesApiCollection.Name)]
public class WorkCitiesApiTests
{
    private readonly HttpClient _client;

    public WorkCitiesApiTests(EmployeesApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWorkCities_ReturnsOkWithDistinctOrderedCities()
    {
        var response = await _client.GetAsync("/api/Employees/work-cities");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var cities = await response.Content.ReadFromJsonAsync<IReadOnlyList<string>>();
        cities.Should().NotBeNull();
        cities!.Should().BeEquivalentTo(["Antwerp", "Brussels"]);
        cities.Should().BeInAscendingOrder();
    }
}
