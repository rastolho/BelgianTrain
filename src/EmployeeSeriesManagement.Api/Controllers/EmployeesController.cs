using EmployeeSeriesManagement.Application.DTOs;
using EmployeeSeriesManagement.Application.Exceptions;
using EmployeeSeriesManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeSeriesManagement.Api.Controllers;

/// <summary>
/// REST endpoints for employee addresses, work-city lookups, and series assignments.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeesController"/> class.
    /// </summary>
    /// <param name="employeeService">Application service for employee operations.</param>
    /// <param name="logger">Structured logger.</param>
    public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all addresses (work and personal) linked to an employee.
    /// </summary>
    /// <param name="employeeExternalId">Employee business key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All addresses for the employee.</returns>
    /// <response code="200">Addresses returned successfully.</response>
    /// <response code="404">Employee was not found.</response>
    [HttpGet("{employeeExternalId:int}/addresses")]
    [ProducesResponseType(typeof(IReadOnlyList<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AddressDto>>> GetAddresses(
        int employeeExternalId,
        CancellationToken cancellationToken)
    {
        try
        {
            var addresses = await _employeeService.GetEmployeeAddressesAsync(employeeExternalId, cancellationToken);
            return Ok(addresses);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Employee {EmployeeExternalId} not found", employeeExternalId);
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Gets personal addresses for employees whose work address is in the given city.
    /// </summary>
    /// <param name="workCity">Work address city filter (required, trimmed).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Personal address rows for matching employees.</returns>
    /// <response code="200">Results returned (may be empty).</response>
    /// <response code="400"><paramref name="workCity"/> is missing or invalid.</response>
    [HttpGet("personal-addresses")]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeePersonalAddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<EmployeePersonalAddressDto>>> GetPersonalAddressesByWorkCity(
        [FromQuery] string workCity,
        CancellationToken cancellationToken)
    {
        try
        {
            var results = await _employeeService.GetPersonalAddressesByWorkCityAsync(workCity, cancellationToken);
            return Ok(results);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lists distinct cities used on work addresses (for UI dropdowns).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Distinct work cities, ordered alphabetically.</returns>
    /// <response code="200">Cities returned successfully.</response>
    [HttpGet("work-cities")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetWorkCities(CancellationToken cancellationToken) =>
        Ok(await _employeeService.GetWorkCitiesAsync(cancellationToken));

    /// <summary>
    /// Gets series assigned to an employee that overlap the requested inclusive period.
    /// </summary>
    /// <param name="employeeExternalId">Employee business key.</param>
    /// <param name="startDate">Period start (inclusive).</param>
    /// <param name="endDate">Period end (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Series assignments overlapping the period.</returns>
    /// <response code="200">Series returned successfully.</response>
    /// <response code="400">Period is invalid (end before start).</response>
    /// <response code="404">Employee was not found.</response>
    [HttpGet("{employeeExternalId:int}/series")]
    [ProducesResponseType(typeof(IReadOnlyList<SeriesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SeriesDto>>> GetSeriesForPeriod(
        int employeeExternalId,
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var series = await _employeeService.GetEmployeeSeriesForPeriodAsync(
                employeeExternalId,
                startDate,
                endDate,
                cancellationToken);
            return Ok(series);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Assigns a series to an employee for a date range.
    /// </summary>
    /// <param name="employeeExternalId">Employee business key.</param>
    /// <param name="body">Series code and assignment date range.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created assignment.</returns>
    /// <response code="201">Assignment created.</response>
    /// <response code="400">Request validation failed.</response>
    /// <response code="404">Employee or series was not found.</response>
    /// <response code="409">An assignment with the same employee, series, and start date already exists.</response>
    [HttpPost("{employeeExternalId:int}/series")]
    [ProducesResponseType(typeof(SeriesDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SeriesDto>> AssignSeries(
        int employeeExternalId,
        [FromBody] AssignSeriesBody body,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new CreateEmployeeSeriesRequest(
                employeeExternalId,
                body.SeriesCode,
                body.StartDate,
                body.EndDate);

            var created = await _employeeService.AssignSeriesToEmployeeAsync(request, cancellationToken);
            return CreatedAtAction(
                nameof(GetSeriesForPeriod),
                new { employeeExternalId, startDate = body.StartDate, endDate = body.EndDate },
                created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DuplicateAssignmentException ex)
        {
            _logger.LogWarning(
                ex,
                "Duplicate series assignment for employee {EmployeeExternalId}, series {SeriesCode}, start {StartDate}",
                ex.EmployeeExternalId,
                ex.SeriesCode,
                ex.StartDate);
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Request body for assigning a series to an employee.
    /// </summary>
    /// <param name="SeriesCode">Series primary key.</param>
    /// <param name="StartDate">Assignment start date (composite key component).</param>
    /// <param name="EndDate">Assignment end date.</param>
    public record AssignSeriesBody(int SeriesCode, DateOnly StartDate, DateOnly EndDate);
}
