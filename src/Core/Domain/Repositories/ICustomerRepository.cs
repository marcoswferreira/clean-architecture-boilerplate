using Domain.Entities;

namespace Domain.Repositories;

/// <summary>
/// Sample domain repository interface: ICustomerRepository.
///
/// This is where domain-specific query contracts are defined.
/// Every method here relates to a Customer-centric business need,
/// not to generic CRUD operations (those come from IBaseRepository&lt;T&gt;).
///
/// CONVENTION:
/// Name query methods after the domain concept they serve, not the implementation:
///   ✅ FindByEmailAsync     (domain language)
///   ❌ SelectWhereEmail     (database language)
///
/// REPLACE this interface with your own domain-specific repository contracts.
/// </summary>
public interface ICustomerRepository : IBaseRepository<Customer>
{
    /// <summary>
    /// Finds a customer by their email address.
    /// Returns null if no customer exists with the given email.
    /// </summary>
    Task<Customer?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all active customers.
    /// Named after the business intent — the SQL detail is the repository's concern.
    /// </summary>
    Task<IEnumerable<Customer>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
