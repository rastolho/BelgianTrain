using System.Net;
using System.Net.Http.Json;
using EmployeeSeriesManagement.Application.Common;
using EmployeeSeriesManagement.Application.DTOs;
using FluentAssertions;

namespace EmployeeSeriesManagement.Tests.Api;

[Collection(EmployeesApiCollection.Name)]
public class EmployeesPersonalAddressesApiTests
{
    private readonly HttpClient _client;

    public EmployeesPersonalAddressesApiTests(EmployeesApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPersonalAddresses_LowercaseBrussels_ReturnsOkWithTwoRows()
    {
        var response = await _client.GetAsync("/api/Employees/personal-addresses?workCity=brussels");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<IReadOnlyList<EmployeePersonalAddressDto>>();
        results.Should().NotBeNull();
        results!.Should().HaveCount(2);
        results.Select(r => r.LastName).Should().BeEquivalentTo(["Dupont", "Janssens"]);
        results.Should().OnlyContain(r => r.PersonalAddress.AddressType == AddressTypeNames.Personal);
    }

    [Fact]
    public async Task GetPersonalAddresses_MixedCaseBrussels_ReturnsOkWithTwoRows()
    {
        var response = await _client.GetAsync("/api/Employees/personal-addresses?workCity=Brussels");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<IReadOnlyList<EmployeePersonalAddressDto>>();
        results!.Select(r => r.LastName).Should().BeEquivalentTo(["Dupont", "Janssens"]);
    }

    [Fact]
    public async Task GetPersonalAddresses_Antwerp_ReturnsOneRowForPeeters()
    {
        var response = await _client.GetAsync("/api/Employees/personal-addresses?workCity=antwerp");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<IReadOnlyList<EmployeePersonalAddressDto>>();
        results!.Should().HaveCount(1);
        results.Single().LastName.Should().Be("Peeters");
        results.Single().PersonalAddress.City.Should().Be("Uccle");
    }

    [Fact]
    public async Task GetPersonalAddresses_UnknownCity_ReturnsOkWithEmptyArray()
    {
        var response = await _client.GetAsync("/api/Employees/personal-addresses?workCity=ghent");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var results = await response.Content.ReadFromJsonAsync<IReadOnlyList<EmployeePersonalAddressDto>>();
        results!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPersonalAddresses_BlankWorkCity_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/Employees/personal-addresses?workCity=");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
