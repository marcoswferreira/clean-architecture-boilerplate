using Application.DTO;
using Domain.Repositories;

namespace Application.Queries.Customers;

/// <summary>
/// Handler for <see cref="GetCustomerByIdQuery"/>.
///
/// Demonstrates the READ side of CQRS:
///   1. Retrieve raw data via the repository (using no-tracking for performance).
///   2. Project the domain entity to a DTO — never return raw entities to callers.
///   3. Wrap in a Result so the controller has a uniform response contract.
///
/// In advanced CQRS setups, the query handler can bypass the domain entirely
/// and query an optimised read model (e.g., a SQL view, Redis, Elasticsearch).
/// </summary>
public class GetCustomerByIdHandler(ICustomerRepository customerRepository)
    : IQueryHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository = customerRepository;

    public Task<ResultGeneric<CustomerDto>> HandleAsync(
        GetCustomerByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        // Use no-tracking for all read queries — EF Core skips change detection overhead.
        var customer = _customerRepository
            .Query(
                predicate: c => c.Id == query.CustomerId,
                enableTracking: false)
            .FirstOrDefault();

        if (customer is null)
            return Task.FromResult(ResultGeneric<CustomerDto>.Failure($"Customer '{query.CustomerId}' not found."));

        // Project the domain entity to the DTO.
        // AutoMapper or Mapperly can replace this manual mapping in larger projects.
        var dto = new CustomerDto(
            customer.Id,
            customer.Name,
            customer.Email.Value,  // unwrap the Email value object to its string value
            customer.IsActive,
            customer.CreatedAt);

        return Task.FromResult(ResultGeneric<CustomerDto>.Success(dto));
    }
}
