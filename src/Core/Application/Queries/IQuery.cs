namespace Application.Queries;

/// <summary>
/// Marker interface for all Queries.
///
/// A QUERY reads state without modifying it (the Q in CQRS).
/// Queries are named as questions: GetCustomerById, ListActiveOrders, FindProductBySkU.
///
/// RULES:
///   - Queries MUST NOT change any state (no side effects).
///   - Queries return data — a typed result (via ResultGeneric&lt;T&gt;) or null if not found.
///   - A query is handled by exactly one handler.
///   - In advanced CQRS setups, queries bypass the domain entirely and hit a read model
///     (e.g., a denormalized SQL view or a Redis cache) for maximum performance.
///
/// USAGE:
///   public record GetCustomerByIdQuery(Guid CustomerId) : IQuery&lt;CustomerDto&gt;;
/// </summary>
/// <typeparam name="TResult">The type of data this query returns.</typeparam>
public interface IQuery<TResult> { }
