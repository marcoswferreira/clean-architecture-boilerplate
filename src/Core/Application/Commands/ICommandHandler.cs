using Application.DTO;

namespace Application.Commands;

/// <summary>
/// Defines the handler for a command that returns only a success/failure result.
///
/// The handler contains the use case logic:
///   1. Retrieve the aggregate(s) needed.
///   2. Call domain methods to enforce business rules.
///   3. Persist changes through the repository.
///   4. Return a <see cref="Result"/> indicating outcome.
///
/// DEPENDENCY INJECTION:
///   Register as: services.AddScoped&lt;ICommandHandler&lt;MyCommand&gt;, MyCommandHandler&gt;();
///
/// USAGE:
///   public class CreateCustomerHandler : ICommandHandler&lt;CreateCustomerCommand&gt; { ... }
/// </summary>
public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines the handler for a command that returns a typed result value
/// (e.g., the Id of the newly created resource).
/// </summary>
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<ResultGeneric<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
