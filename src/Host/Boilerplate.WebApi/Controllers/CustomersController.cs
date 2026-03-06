using Application.Commands;
using Application.Commands.Customers;
using Application.Queries;
using Application.Queries.Customers;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Boilerplate.WebApi.Controllers;

/// <summary>
/// Sample API controller: Customers.
///
/// Demonstrates the recommended Presentation Layer patterns:
///   - Thin controller — all logic delegated to Command/Query handlers
///   - API versioning via [ApiVersion] attribute
///   - Consistent HTTP status codes and ProblemDetails error responses
///   - RESTful resource naming conventions
///
/// IMPORTANT:
/// Controllers should contain ZERO business logic.
/// They are responsible only for:
///   1. Accepting HTTP input and mapping it to a Command/Query
///   2. Dispatching to the appropriate handler
///   3. Translating the Result to an HTTP response
/// </summary>
[ApiController]
[ApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class CustomersController(
    ICommandHandler<CreateCustomerCommand, Guid> createHandler,
    IQueryHandler<GetCustomerByIdQuery, CustomerDto> getByIdHandler) : ControllerBase
{
    private readonly ICommandHandler<CreateCustomerCommand, Guid> _createHandler = createHandler;
    private readonly IQueryHandler<GetCustomerByIdQuery, CustomerDto> _getByIdHandler = getByIdHandler;

    /// <summary>
    /// GET api/v1/customers/{id}
    /// Retrieves a single customer by their unique identifier.
    /// Returns 404 Not Found if no customer exists with that Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getByIdHandler.HandleAsync(
            new GetCustomerByIdQuery(id), cancellationToken);

        // If the Result signals failure, return a 404 with the error message as ProblemDetails.
        if (!result.IsSuccess)
            return NotFound(new { errors = result.Errors });

        return Ok(result.Value);
    }

    /// <summary>
    /// POST api/v1/customers
    /// Creates a new customer.
    /// Returns 201 Created with a Location header pointing to the new resource.
    /// Returns 400 Bad Request if the input is invalid (e.g., duplicate email, empty name).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _createHandler.HandleAsync(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors });

        // Return 201 Created with a Location header to maintain REST conventions.
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }
}
