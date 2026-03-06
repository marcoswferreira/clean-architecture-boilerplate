using Domain.Events.Customers;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Sample Aggregate Root entity: Customer.
///
/// This class demonstrates:
///   - Inheriting from <see cref="BaseEntity"/> for identity + domain events
///   - Implementing <see cref="IAggregateRoot"/> to mark this as an aggregate boundary
///   - Using a Value Object (<see cref="Email"/>) for typed, validated attributes
///   - Raising Domain Events on meaningful state changes
///   - Encapsulating state — only expose mutations through domain methods (not raw setters)
///
/// REPLACE THIS CLASS with your own domain entities when scaffolding a new project.
/// </summary>
public class Customer : BaseEntity, IAggregateRoot
{
    /// <summary>Full name of the customer.</summary>
    public string Name { get; private set; }

    /// <summary>
    /// Email as a Value Object, guaranteeing it is always valid and normalized.
    /// Using a VO instead of a plain string prevents invalid emails from ever entering the domain.
    /// </summary>
    public Email Email { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Private parameterless constructor required by EF Core for materialization.
    // Properties are always initialised via the factory method; suppress the nullable warning.
#pragma warning disable CS8618
    private Customer() { }
#pragma warning restore CS8618

    /// <summary>
    /// Factory method to create a new Customer.
    /// Using a factory method instead of a public constructor allows us to:
    ///   1. Enforce invariants before the object is ever created.
    ///   2. Raise domain events as part of creation.
    ///   3. Return different subtypes in more advanced scenarios.
    /// </summary>
    public static Customer Create(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name cannot be empty.", nameof(name));

        var customer = new Customer
        {
            Name = name,
            Email = Email.Create(email)  // delegates validation to the Email VO
        };

        // Raise a domain event so other parts of the system can react
        // (e.g., send a welcome email, update a read model, write an audit log).
        customer.AddDomainEvent(new CustomerCreatedEvent(customer.Id, name, email));

        return customer;
    }

    /// <summary>
    /// Updates the customer's name.
    /// All mutations happen through explicit domain methods that enforce rules.
    /// Direct property setters are private to prevent bypassing business logic.
    /// </summary>
    public void ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name;
    }

    /// <summary>
    /// Updates the customer's email address.
    /// Passing through the Email value object guarantees that the new value is valid.
    /// </summary>
    public void ChangeEmail(string email) =>
        Email = Email.Create(email);

    /// <summary>Marks the customer as inactive (soft delete pattern).</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>Reactivates a previously deactivated customer.</summary>
    public void Activate() => IsActive = true;
}
