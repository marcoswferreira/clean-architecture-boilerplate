namespace Domain.Events;

/// <summary>
/// Marker interface for all domain events.
///
/// KEY CONCEPTS:
/// - A Domain Event represents something significant that happened in the domain (past tense).
/// - Events are immutable records of what occurred — they should never be changed after creation.
/// - They decouple the entity that raises the event from the logic that reacts to it.
///
/// EVENT SOURCING:
/// When using Event Sourcing, domain events become the source of truth.
/// Instead of persisting the current state, you persist the sequence of events.
/// The current state is rebuilt by replaying the events in order.
///
/// DISPATCH FLOW:
///   1. Entity raises event via AddDomainEvent() during a domain operation.
///   2. Infrastructure (e.g., DbContext.SaveChanges) persists the state.
///   3. After commit, the dispatcher publishes all collected events.
///   4. Event handlers react (send emails, update read models, trigger other aggregates, etc.).
///
/// USAGE:
/// Implement this interface on record or class types that describe a past occurrence.
/// Name events in the past tense: CustomerCreated, OrderPlaced, PaymentFailed.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Unique identifier for this event occurrence.</summary>
    Guid EventId { get; }

    /// <summary>The UTC timestamp when this event occurred.</summary>
    DateTime OccurredOn { get; }
}
