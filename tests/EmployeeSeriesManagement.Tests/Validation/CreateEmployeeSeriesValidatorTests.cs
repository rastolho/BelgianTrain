using EmployeeSeriesManagement.Application.DTOs;
using EmployeeSeriesManagement.Application.Validation;
using FluentAssertions;

namespace EmployeeSeriesManagement.Tests.Validation;

public class CreateEmployeeSeriesValidatorTests
{
    [Fact]
    public void Validate_ReturnsNoErrors_WhenRequestIsValid()
    {
        var request = new CreateEmployeeSeriesRequest(1001, 501, new DateOnly(2026, 6, 1), new DateOnly(2026, 8, 31));

        var errors = CreateEmployeeSeriesValidator.Validate(request);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ReturnsError_WhenEndDateBeforeStartDate()
    {
        var request = new CreateEmployeeSeriesRequest(1001, 501, new DateOnly(2026, 8, 31), new DateOnly(2026, 6, 1));

        var errors = CreateEmployeeSeriesValidator.Validate(request);

        errors.Should().Contain(e => e.Contains("EndDate", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(0, 501)]
    [InlineData(-1, 501)]
    public void Validate_ReturnsError_WhenEmployeeIdInvalid(int employeeId, int seriesCode)
    {
        var request = new CreateEmployeeSeriesRequest(employeeId, seriesCode, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

        var errors = CreateEmployeeSeriesValidator.Validate(request);

        errors.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(1001, 0)]
    [InlineData(1001, -5)]
    public void Validate_ReturnsError_WhenSeriesCodeInvalid(int employeeId, int seriesCode)
    {
        var request = new CreateEmployeeSeriesRequest(employeeId, seriesCode, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

        var errors = CreateEmployeeSeriesValidator.Validate(request);

        errors.Should().Contain(e => e.Contains("SeriesCode", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ReturnsMultipleErrors_WhenAllFieldsInvalid()
    {
        var request = new CreateEmployeeSeriesRequest(0, 0, new DateOnly(2026, 12, 31), new DateOnly(2026, 1, 1));

        var errors = CreateEmployeeSeriesValidator.Validate(request);

        errors.Should().HaveCountGreaterThanOrEqualTo(3);
    }
}
