namespace Application.Services;

/// <summary>
/// Marker interface for Application Services.
///
/// An Application Service coordinates a use case by:
///   1. Receiving input (from a controller, worker, or event handler).
///   2. Calling domain objects (entities, domain services) to implement business rules.
///   3. Using repositories and other infrastructure abstractions (never directly).
///   4. Returning a result or raising events.
///
/// WHEN TO USE vs COMMAND HANDLERS:
///   - In a CQRS-first design, prefer ICommandHandler / IQueryHandler for each use case.
///   - Use IApplicationService for services that orchestrate multiple use cases
///     or provide a richer API surface (e.g., CustomerService with CreateCustomer + ChangeEmail).
///
/// RULES:
///   - Application Services should NOT contain business rules — those belong in the Domain.
///   - Application Services should NOT reference the Infrastructure layer directly;
///     use repository interfaces defined in the Domain.
///
/// USAGE:
///   public class CustomerService : IApplicationService { ... }
/// </summary>
public interface IApplicationService { }
