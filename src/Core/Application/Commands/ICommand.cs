using Application.DTO;

namespace Application.Commands;

/// <summary>
/// Marker interface for all Commands.
///
/// A COMMAND is an intent to change state in the system (the C in CQRS).
/// Commands are named in the imperative: CreateCustomer, PlaceOrder, CancelSubscription.
///
/// RULES:
///   - A command may only CHANGE state. It should not return domain data.
///   - Commands should be validated before being handled.
///   - Commands are processed by exactly one handler (1-to-1 relationship).
///   - Commands return a <see cref="Result"/> indicating success or failure,
///     never the mutated entity itself (that would couple the caller to the write model).
///
/// USAGE:
///   public record CreateCustomerCommand(string Name, string Email) : ICommand;
/// </summary>
public interface ICommand { }

/// <summary>
/// Marker interface for Commands that return a typed value.
/// Use when the caller needs a specific piece of data after the command executes
/// (e.g., the Id of the newly created resource).
/// </summary>
public interface ICommand<TResult> { }
