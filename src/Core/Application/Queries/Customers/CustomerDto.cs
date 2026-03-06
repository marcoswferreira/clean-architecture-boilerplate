namespace Application.Queries.Customers;

/// <summary>
/// DTO returned by <see cref="GetCustomerByIdQuery"/>.
///
/// A DTO (Data Transfer Object) is a projection of the domain entity
/// shaped specifically for the consumer's needs.
///
/// IMPORTANT: Never expose domain entities directly through the API.
///   - Entities contain internal state and behavior that callers don't need.
///   - Exposing entities tightly couples the API contract to the domain model.
///   - DTOs let you evolve the domain without breaking the API contract.
/// </summary>
public record CustomerDto(
    Guid Id,
    string Name,
    string Email,
    bool IsActive,
    DateTime CreatedAt);
