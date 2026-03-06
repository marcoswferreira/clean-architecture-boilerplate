namespace Application.Commands.Customers;

/// <summary>
/// Sample Command: Create a new customer.
///
/// A command is a simple data container (DTO) describing the user's intent.
/// It carries only the input data needed to perform the operation.
/// No business logic here — validation and domain rules live in the handler
/// and the domain entities respectively.
///
/// Using a C# record ensures immutability (commands should not be mutated in-flight).
/// </summary>
/// <param name="Name">The customer's full name.</param>
/// <param name="Email">The customer's email address (will be validated by the Email value object).</param>
public record CreateCustomerCommand(string Name, string Email) : ICommand<Guid>;
