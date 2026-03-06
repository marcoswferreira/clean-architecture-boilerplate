namespace Domain.Entities;

/// <summary>
/// Marker interface for Aggregate Roots.
///
/// KEY CONCEPTS:
/// - An Aggregate is a cluster of domain objects (entities + value objects) treated as a unit.
/// - The Aggregate Root is the single entry point to the aggregate; external objects may only
///   hold references to the root, never to internal entities.
/// - The root enforces all invariants (business rules) for the objects within its boundary.
///
/// EXAMPLE:
/// An Order aggregate might contain OrderLines.
/// External code works with Order (the root), never directly with OrderLine.
/// This guarantees that Order can enforce rules like "total items must be &gt; 0".
///
/// USAGE:
/// Implement this interface on entities that act as the root of their aggregate.
/// All aggregate roots should also inherit from <see cref="BaseEntity"/>.
/// </summary>
public interface IAggregateRoot { }
