using Application.DTO;
using Domain.Entities;
using Domain.Repositories;

namespace Application.Commands.Customers;

/// <summary>
/// Handler for <see cref="CreateCustomerCommand"/>.
///
/// This class demonstrates the standard Application Layer use-case flow:
///   1. Optional: validate the command input (pre-domain guard)
///   2. Delegate to the Domain to create the aggregate (business rules enforced in the entity)
///   3. Persist through the repository abstraction (never a concrete DB class)
///   4. Return a Result with the new entity's Id
///
/// DEPENDENCY INJECTION:
///   Register in the DI container as:
///     services.AddScoped&lt;ICommandHandler&lt;CreateCustomerCommand, Guid&gt;, CreateCustomerCommandHandler&gt;();
///
/// NOTE ON DOMAIN EVENTS:
///   After SaveChanges(), the infrastructure layer (or a mediator pipeline behaviour)
///   should dispatch any domain events collected on the Customer aggregate
///   (e.g., CustomerCreatedEvent → sends welcome email).
/// </summary>
public class CreateCustomerCommandHandler(ICustomerRepository customerRepository)
    : ICommandHandler<CreateCustomerCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository = customerRepository;

    public async Task<ResultGeneric<Guid>> HandleAsync(
        CreateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        // 1. Pre-domain guard: check for uniqueness (a domain rule that requires a DB look-up).
        //    Pure business invariants (like "name cannot be empty") are enforced inside Customer.Create().
        var existing = await _customerRepository.FindByEmailAsync(command.Email, cancellationToken);
        if (existing is not null)
            return ResultGeneric<Guid>.Failure($"A customer with email '{command.Email}' already exists.");

        // 2. Delegate creation to the domain entity.
        //    Customer.Create() validates inputs, constructs the object, and raises CustomerCreatedEvent.
        var customer = Customer.Create(command.Name, command.Email);

        // 3. Persist. The repository abstracts away all EF Core / SQL concerns.
        _customerRepository.Insert(customer);
        _customerRepository.SaveChanges();

        // 4. Return the new Id so the caller can respond with a 201 Created + Location header.
        return ResultGeneric<Guid>.Success(customer.Id);
    }
}
