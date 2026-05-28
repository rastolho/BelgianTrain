using System.Net;
using System.Net.Http.Json;
using EmployeeSeriesManagement.Application.Common;
using EmployeeSeriesManagement.Application.DTOs;
using FluentAssertions;

namespace EmployeeSeriesManagement.Tests.Api;

[Collection(EmployeesApiCollection.Name)]
public class EmployeeAddressesApiTests
{
    private readonly HttpClient _client;

    public EmployeeAddressesApiTests(EmployeesApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAddresses_KnownEmployee_ReturnsOkWithWorkAndPersonal()
    {
        var response = await _client.GetAsync("/api/Employees/1001/addresses");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var addresses = await response.Content.ReadFromJsonAsync<IReadOnlyList<AddressDto>>();
        addresses.Should().NotBeNull();
        addresses!.Should().HaveCount(2);
        addresses.Select(a => a.AddressType).Should().BeEquivalentTo(
            [AddressTypeNames.Work, AddressTypeNames.Personal]);
        addresses.Should().Contain(a => a.AddressType == AddressTypeNames.Work && a.City == "Brussels");
        addresses.Should().Contain(a => a.AddressType == AddressTypeNames.Personal && a.City == "Ixelles");
    }

    [Fact]
    public async Task GetAddresses_PeetersEmployee_ReturnsAntwerpWorkAndUcclePersonal()
    {
        var response = await _client.GetAsync("/api/Employees/1003/addresses");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var addresses = await response.Content.ReadFromJsonAsync<IReadOnlyList<AddressDto>>();
        addresses!.Should().Contain(a => a.AddressType == AddressTypeNames.Work && a.City == "Antwerp");
        addresses.Should().Contain(a => a.AddressType == AddressTypeNames.Personal && a.City == "Uccle");
    }

    [Fact]
    public async Task GetAddresses_UnknownEmployee_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/Employees/99999/addresses");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
