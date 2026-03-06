using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Domain.Repositories;

/// <summary>
/// Generic repository interface (Port) — defines the contract for data access.
///
/// CLEAN ARCHITECTURE PLACEMENT:
/// This interface lives in the DOMAIN layer because the Domain defines WHAT it needs
/// (the port). The Infrastructure layer provides HOW it is done (the adapter/implementation).
/// This keeps the domain free of any EF Core, SQL, or persistence details.
///
/// CQRS NOTE:
/// In strict CQRS setups, repositories handle Commands (writes) only.
/// Queries are served by separate read models.  This base repository supports both
/// for simplicity; feel free to split into IReadRepository / IWriteRepository.
///
/// USAGE:
/// Do not inject IBaseRepository&lt;T&gt; directly. Instead, create a specific interface:
///
///   public interface ICustomerRepository : IBaseRepository&lt;Customer&gt;
///   {
///       Task&lt;Customer?&gt; FindByEmailAsync(string email, CancellationToken ct = default);
///   }
///
/// This keeps consumer code coupled only to the contract it needs.
/// </summary>
/// <typeparam name="T">The entity type this repository manages.</typeparam>
public interface IBaseRepository<T> where T : class
{
    /// <summary>
    /// Builds a flexible, composable query against the entity set.
    /// All parameters are optional — combine as needed.
    /// </summary>
    /// <param name="predicate">Filter expression (WHERE clause).</param>
    /// <param name="orderBy">Ordering function (ORDER BY clause).</param>
    /// <param name="include">Eager-loading function (JOIN / INCLUDE).</param>
    /// <param name="enableTracking">
    ///   True = EF Core tracks the returned entities for change detection (use for writes).
    ///   False = AsNoTracking, better performance for read-only queries.
    /// </param>
    IQueryable<T> Query(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
        bool enableTracking = true);

    /// <summary>
    /// Adds a new entity to the context (does NOT immediately save to the database).
    /// Call <see cref="SaveChanges"/> or commit the Unit of Work to persist.
    /// </summary>
    T Insert(T entity);

    /// <summary>
    /// Inserts the entity only if no entity matching the predicate already exists.
    /// Returns the existing entity if found, otherwise inserts and returns the new one.
    /// Useful for idempotent seeding or upsert-like scenarios.
    /// </summary>
    T InsertNotExists(Expression<Func<T, bool>> predicate, T entity);

    /// <summary>
    /// Marks the entity as modified so EF Core will generate an UPDATE statement on save.
    /// The entity must already be tracked; if not, use context.Entry(entity).State = Modified.
    /// </summary>
    void Update(T entity);

    /// <summary>Removes a single entity from the context (does NOT immediately delete from DB).</summary>
    void Delete(T entity);

    /// <summary>Removes a collection of entities in a single operation.</summary>
    void Delete(IEnumerable<T> entities);

    /// <summary>
    /// Persists all pending changes in the current DbContext to the database.
    /// Returns true if at least one row was affected.
    /// </summary>
    bool SaveChanges();
}
