using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using Crosscutting.SqlServer.DbContexts;

namespace Crosscutting.SqlServer.Repositories;

/// <summary>
/// Sample concrete repository: CustomerRepository.
///
/// This class is the ADAPTER in Hexagonal Architecture terminology:
///   - The PORT (ICustomerRepository) is defined in the Domain layer.
///   - The ADAPTER (this class) lives in Infrastructure and provides the EF Core implementation.
///   - Application code depends only on ICustomerRepository — never on this class directly.
///
/// FEATURES DEMONSTRATED:
///   - Inheriting BaseRepository&lt;T&gt; for all generic CRUD + query operations
///   - Adding domain-specific async query methods
///   - Using AsNoTracking() for read-only queries
///
/// REGISTRATION (in DI container, e.g., Program.cs or an extension method):
///   services.AddScoped&lt;ICustomerRepository, CustomerRepository&gt;();
/// </summary>
public class CustomerRepository(ApplicationDbContext context)
    : BaseRepository<Customer>(context), ICustomerRepository
{
    /// <summary>
    /// Finds a customer by email address using a case-insensitive, no-tracking query.
    /// Returns null if not found.
    ///
    /// NOTE: Calling ToLower() inside a LINQ expression translates directly to
    /// LOWER() in SQL, ensuring case-insensitive comparison at the database level.
    /// </summary>
    public async Task<Customer?> FindByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Email.Value.ToLower() == email.ToLower(),
                cancellationToken);
    }

    /// <summary>
    /// Returns all active customers using the inherited Query() method
    /// with a filter predicate and no-tracking for read performance.
    /// </summary>
    public async Task<IEnumerable<Customer>> GetAllActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);
    }
}
