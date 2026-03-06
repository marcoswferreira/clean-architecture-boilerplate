namespace Application.Queries.Customers;

/// <summary>
/// Sample Query: Retrieve a single customer by their Id.
///
/// Using a record ensures the query is immutable and safe to pass across layers.
/// </summary>
public record GetCustomerByIdQuery(Guid CustomerId) : IQuery<CustomerDto>;
