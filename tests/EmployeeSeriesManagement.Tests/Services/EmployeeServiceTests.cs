using EmployeeSeriesManagement.Application.Common;
using EmployeeSeriesManagement.Application.DTOs;
using EmployeeSeriesManagement.Application.Exceptions;
using EmployeeSeriesManagement.Application.Services;
using EmployeeSeriesManagement.Infrastructure.Data;
using EmployeeSeriesManagement.Infrastructure.Data.Seed;
using EmployeeSeriesManagement.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace EmployeeSeriesManagement.Tests.Services;

public class EmployeeServiceTests : IClassFixture<MsSqlContainerFixture>, IDisposable
{
    private readonly string _databaseName = $"esm_test_{Guid.NewGuid():N}";
    private readonly string _connectionString;
    private readonly EmployeeSeriesDbContext _context;
    private readonly EmployeeService _service;

    public EmployeeServiceTests(MsSqlContainerFixture fixture)
    {
        var builder = new SqlConnectionStringBuilder(fixture.ConnectionString)
        {
            InitialCatalog = _databaseName
        };
        _connectionString = builder.ConnectionString;

        var options = new DbContextOptionsBuilder<EmployeeSeriesDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        _context = new EmployeeSeriesDbContext(options);
        _context.Database.EnsureCreated();
        DatabaseSeeder.SeedAsync(_context, NullLogger.Instance).GetAwaiter().GetResult();

        var repository = new EmployeeRepository(_context);
        _service = new EmployeeService(repository, NullLogger<EmployeeService>.Instance);
    }

    [Fact]
    public async Task GetEmployeeAddressesAsync_Employee1001_ReturnsWorkAndPersonal()
    {
        var addresses = await _service.GetEmployeeAddressesAsync(1001);

        addresses.Should().HaveCount(2);
        addresses.Select(a => a.AddressType).Should().BeEquivalentTo(
            [AddressTypeNames.Work, AddressTypeNames.Personal]);
        addresses.Should().Contain(a => a.AddressType == AddressTypeNames.Work && a.City == "Brussels");
        addresses.Should().Contain(a => a.AddressType == AddressTypeNames.Personal && a.City == "Ixelles");
    }

    [Fact]
    public async Task GetPersonalAddressesByWorkCityAsync_LowercaseBrussels_ReturnsTwoRows()
    {
        var results = await _service.GetPersonalAddressesByWorkCityAsync("brussels");

        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.PersonalAddress.AddressType == AddressTypeNames.Personal);
        results.Select(r => r.LastName).Should().BeEquivalentTo(["Dupont", "Janssens"]);
    }

    [Fact]
    public async Task GetPersonalAddressesByWorkCityAsync_MixedCaseBrussels_ReturnsTwoRows()
    {
        var results = await _service.GetPersonalAddressesByWorkCityAsync("Brussels");

        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.PersonalAddress.AddressType == AddressTypeNames.Personal);
        results.Select(r => r.LastName).Should().BeEquivalentTo(["Dupont", "Janssens"]);
    }

    [Fact]
    public async Task GetEmployeeSeriesForPeriodAsync_ReturnsOverlappingAssignments()
    {
        var series = await _service.GetEmployeeSeriesForPeriodAsync(
            1001,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31));

        series.Should().Contain(s => s.Code == 501);
        series.Should().NotContain(s => s.Code == 502 && s.AssignmentEndDate.Year == 2025);
    }

    [Fact]
    public async Task AssignSeriesToEmployeeAsync_PersistsNewAssignment()
    {
        var created = await _service.AssignSeriesToEmployeeAsync(
            new CreateEmployeeSeriesRequest(1002, 502, new DateOnly(2026, 3, 1), new DateOnly(2026, 9, 30)));

        created.Code.Should().Be(502);
        created.AssignmentStartDate.Should().Be(new DateOnly(2026, 3, 1));

        var reloaded = await _service.GetEmployeeSeriesForPeriodAsync(
            1002,
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 9, 30));

        reloaded.Should().Contain(s => s.Code == 502 && s.AssignmentStartDate == new DateOnly(2026, 3, 1));
    }

    [Fact]
    public async Task AssignSeriesToEmployeeAsync_DuplicateSameKey_ThrowsDuplicateAssignmentException()
    {
        var duplicateRequest = new CreateEmployeeSeriesRequest(
            1001,
            501,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 8, 31));

        var act = () => _service.AssignSeriesToEmployeeAsync(duplicateRequest);

        var ex = await act.Should().ThrowAsync<DuplicateAssignmentException>();
        ex.Which.EmployeeExternalId.Should().Be(1001);
        ex.Which.SeriesCode.Should().Be(501);
        ex.Which.StartDate.Should().Be(new DateOnly(2026, 6, 1));
    }

    [Fact]
    public async Task GetEmployeeAddressesAsync_UnknownEmployee_ThrowsKeyNotFoundException()
    {
        var act = () => _service.GetEmployeeAddressesAsync(99999);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*99999*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetPersonalAddressesByWorkCityAsync_BlankCity_ThrowsArgumentException(string? workCity)
    {
        var act = () => _service.GetPersonalAddressesByWorkCityAsync(workCity!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetPersonalAddressesByWorkCityAsync_AntwerpReturnsPeeters()
    {
        var results = await _service.GetPersonalAddressesByWorkCityAsync("Antwerp");

        results.Should().HaveCount(1);
        results.Single().LastName.Should().Be("Peeters");
        results.Single().PersonalAddress.City.Should().Be("Uccle");
        results.Single().PersonalAddress.AddressType.Should().Be(AddressTypeNames.Personal);
    }

    [Fact]
    public async Task GetPersonalAddressesByWorkCityAsync_UnknownCity_ReturnsEmpty()
    {
        var results = await _service.GetPersonalAddressesByWorkCityAsync("Ghent");

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWorkCitiesAsync_ReturnsDistinctOrdered()
    {
        var cities = await _service.GetWorkCitiesAsync();

        cities.Should().BeEquivalentTo(["Antwerp", "Brussels"]);
        cities.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetEmployeeSeriesForPeriodAsync_UnknownEmployee_ThrowsKeyNotFoundException()
    {
        var act = () => _service.GetEmployeeSeriesForPeriodAsync(
            99999,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31));

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*99999*");
    }

    [Fact]
    public async Task GetEmployeeSeriesForPeriodAsync_EndBeforeStart_ThrowsArgumentException()
    {
        var act = () => _service.GetEmployeeSeriesForPeriodAsync(
            1001,
            new DateOnly(2026, 12, 31),
            new DateOnly(2026, 1, 1));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetEmployeeSeriesForPeriodAsync_NoOverlap_ReturnsEmpty()
    {
        var series = await _service.GetEmployeeSeriesForPeriodAsync(
            1002,
            new DateOnly(2020, 1, 1),
            new DateOnly(2020, 12, 31));

        series.Should().BeEmpty();
    }

    [Fact]
    public async Task AssignSeriesToEmployeeAsync_UnknownEmployee_ThrowsKeyNotFoundException()
    {
        var act = () => _service.AssignSeriesToEmployeeAsync(
            new CreateEmployeeSeriesRequest(99999, 501, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*99999*");
    }

    [Fact]
    public async Task AssignSeriesToEmployeeAsync_UnknownSeries_ThrowsKeyNotFoundException()
    {
        var act = () => _service.AssignSeriesToEmployeeAsync(
            new CreateEmployeeSeriesRequest(1001, 88888, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*88888*");
    }

    [Fact]
    public async Task AssignSeriesToEmployeeAsync_ValidationFailure_ThrowsArgumentException()
    {
        var act = () => _service.AssignSeriesToEmployeeAsync(
            new CreateEmployeeSeriesRequest(1001, 501, new DateOnly(2026, 12, 31), new DateOnly(2026, 1, 1)));

        await act.Should().ThrowAsync<ArgumentException>();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
