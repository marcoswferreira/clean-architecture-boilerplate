using Application.DTO;

namespace Application.Queries;

/// <summary>
/// Defines the handler for a query that returns a typed result.
///
/// The handler retrieves and projects data into a DTO suitable for the caller.
/// It should NOT return domain entities — expose only what the caller needs.
///
/// DEPENDENCY INJECTION:
///   Register as: services.AddScoped&lt;IQueryHandler&lt;MyQuery, MyDto&gt;, MyQueryHandler&gt;();
///
/// USAGE:
///   public class GetCustomerByIdHandler : IQueryHandler&lt;GetCustomerByIdQuery, CustomerDto&gt; { ... }
/// </summary>
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<ResultGeneric<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
