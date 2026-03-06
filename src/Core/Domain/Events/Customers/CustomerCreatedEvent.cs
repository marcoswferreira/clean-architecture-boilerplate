using Domain.Events;
using Domain.ValueObjects;

namespace Domain.Events.Customers;

/// <summary>
/// Domain event raised when a new customer is successfully created.
///
/// This is a sample implementation showing the recommended convention:
///   - Immutable record (cannot be changed after creation)
///   - Named in the PAST TENSE (CustomerCreated, not CreateCustomer)
///   - Carries only the data needed by downstream handlers
///
/// EXAMPLE HANDLERS that might react to this event:
///   - SendWelcomeEmailHandler  → sends an onboarding email
///   - CreateCustomerProjection → updates a read-model / cache
///   - AuditLogHandler          → writes an audit entry
/// </summary>
public record CustomerCreatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    /// <summary>The Id of the customer who was just created.</summary>
    public Guid CustomerId { get; init; }

    /// <summary>The customer's name at the time of creation.</summary>
    public string Name { get; init; }

    /// <summary>The customer's email at the time of creation.</summary>
    public string Email { get; init; }

    public CustomerCreatedEvent(Guid customerId, string name, string email)
    {
        CustomerId = customerId;
        Name = name;
        Email = email;
    }
}
