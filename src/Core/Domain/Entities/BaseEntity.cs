using Domain.Events;

namespace Domain.Entities;

/// <summary>
/// Base class for all entities in the domain.
///
/// KEY CONCEPTS:
/// - Every entity has a unique identity (Id) that persists through its lifetime.
/// - Entities are not compared by value but by identity (same Id = same entity).
/// - Entities can raise domain events to communicate state changes across boundaries.
///
/// USAGE:
/// Inherit from BaseEntity for any object that has a distinct lifecycle and identity.
/// Example: Customer, Order, Product.
/// Do NOT use for Value Objects (e.g. Email, Money) — those have no identity.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// Initialized with a new Guid by default, but can be set in the constructor
    /// of derived classes (e.g., when rehydrating from a database).
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    // Domain events are collected in-memory and dispatched AFTER persistence.
    // This ensures consistency: events are only published when the state is saved.
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Collection of domain events raised by this entity during the current operation.
    /// Exposed as read-only to prevent external modification.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Registers a domain event to be dispatched after the current unit of work commits.
    /// Call this inside aggregate methods when state changes need to be communicated
    /// to other parts of the system (e.g., send a welcome email when a customer is created).
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    /// <summary>
    /// Clears all domain events from the entity.
    /// Typically called by the infrastructure layer (e.g., in SaveChanges) 
    /// after events have been dispatched.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    // Two entities are equal if and only if they have the same Id.
    // This overrides the default reference equality from object.
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(BaseEntity? left, BaseEntity? right) =>
        Equals(left, right);

    public static bool operator !=(BaseEntity? left, BaseEntity? right) =>
        !Equals(left, right);
}
