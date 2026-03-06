# .NET Boilerplate Web API

A production-ready **Clean Architecture** template for .NET 10 Web APIs.  
Built on **DDD**, **CQRS**, **Event Sourcing**, **Hexagonal Architecture**, and **SOLID** principles.

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Project Structure](#project-structure)
- [Domain Layer](#domain-layer)
- [Application Layer](#application-layer)
- [Infrastructure Layer](#infrastructure-layer)
- [Presentation Layer](#presentation-layer)
- [Getting Started](#getting-started)
- [Testing](#testing)
- [Deployment](#deployment)
- [Contributing](#contributing)

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                  Presentation (WebApi)                   │
│              Controllers · DTOs · Swagger                │
├─────────────────────────────────────────────────────────┤
│                    Application Layer                     │
│         Commands · Queries · Handlers · DTOs            │
├─────────────────────────────────────────────────────────┤
│                      Domain Layer                        │
│   Entities · Value Objects · Events · Repositories      │
├─────────────────────────────────────────────────────────┤
│                  Infrastructure Layer                    │
│         EF Core · DbContext · Repositories              │
└─────────────────────────────────────────────────────────┘
```

Dependencies always point **inward** — Infrastructure and Presentation depend on the inner layers, never the reverse.

---

## Project Structure

```
src/
├── Core/
│   ├── Domain/                      # Innermost layer — no dependencies
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs        # Base class with Id + domain events
│   │   │   ├── IAggregateRoot.cs    # Marker interface for aggregate roots
│   │   │   └── Customer.cs          # Sample aggregate root entity
│   │   ├── Events/
│   │   │   ├── IDomainEvent.cs      # Domain event marker interface
│   │   │   └── Customers/
│   │   │       └── CustomerCreatedEvent.cs
│   │   ├── ValueObjects/
│   │   │   ├── IValueObject.cs      # Value object marker interface
│   │   │   └── Email.cs             # Sample value object with validation
│   │   ├── Repositories/
│   │   │   ├── IBaseRepository.cs   # Generic repository port (interface)
│   │   │   └── ICustomerRepository.cs
│   │   └── Common/
│   │       └── Paginate/            # Pagination abstractions
│   │
│   └── Application/                 # Use cases — depends on Domain only
│       ├── Commands/
│       │   ├── ICommand.cs
│       │   ├── ICommandHandler.cs
│       │   └── Customers/
│       │       ├── CreateCustomerCommand.cs
│       │       └── CreateCustomerCommandHandler.cs
│       ├── Queries/
│       │   ├── IQuery.cs
│       │   ├── IQueryHandler.cs
│       │   └── Customers/
│       │       ├── CustomerDto.cs
│       │       ├── GetCustomerByIdQuery.cs
│       │       └── GetCustomerByIdQueryHandler.cs
│       ├── Services/
│       │   └── IApplicationService.cs
│       └── DTO/
│           ├── Result.cs            # Non-generic result (success/failure)
│           └── ResultGeneric.cs     # Generic result with typed value
│
├── Infra/
│   └── Crosscutting.SqlServer/      # EF Core adapter — depends on Domain
│       ├── DbContexts/
│       │   └── ApplicationDbContext.cs
│       └── Repositories/
│           ├── BaseRepository.cs    # Generic EF Core repository
│           └── CustomerRepository.cs
│
├── Host/
│   └── Boilerplate.WebApi/          # Presentation — depends on Application + Infra
│       ├── Controllers/
│       │   └── CustomersController.cs
│       ├── Extensions/
│       │   └── AppExtensions.cs    # Swagger + versioning setup
│       ├── ExceptionHandler.cs
│       └── Program.cs
│
└── Tests/
    └── Unit.Tests/
```

---

## Domain Layer

The **innermost layer** — has zero dependencies on any other project or NuGet package (except EF Core for `IQueryable` pagination helpers).

### `BaseEntity`

Every domain entity inherits from `BaseEntity`. It provides:

- **`Id`** — `Guid`, initialized on construction; never changes.
- **Domain Events** — entities raise events via `AddDomainEvent()`. Events are dispatched *after* persistence to guarantee consistency.
- **Identity Equality** — two entities are equal if they share the same `Id`, regardless of other properties.

```csharp
public class Order : BaseEntity, IAggregateRoot
{
    public void Place()
    {
        // ... business logic ...
        AddDomainEvent(new OrderPlacedEvent(Id));
    }
}
```

### `IAggregateRoot`

Marker interface. Apply it to entities that are the **boundary** of a consistency group (e.g., `Order` owns `OrderLine`s). External code must always interact through the aggregate root, never with its internal entities directly.

### `IDomainEvent`

Marker interface for domain events. Events must be:
- **Immutable** — use C# `record` types.
- **Named in past tense** — `CustomerCreated`, `OrderPlaced`, `PaymentFailed`.

Events carry only the data needed by downstream handlers. They decouple the producer from consumers.

### `IValueObject` and `Email` (sample)

A **Value Object** is defined entirely by its attributes, has no identity, and is always immutable:

```csharp
var email = Email.Create("user@example.com"); // validated, normalized
string raw = email;                           // implicit string conversion
```

`Email.Create()` throws `ArgumentException` on invalid input — making invalid state unrepresentable.

### `IBaseRepository<T>` and `ICustomerRepository` (sample)

Repository interfaces live in the **Domain** layer (the port definition). This keeps domain logic free of any persistence detail.

| Method | Description |
|---|---|
| `Query(predicate, orderBy, include, enableTracking)` | Composable LINQ query with optional filter, sort, and eager-load |
| `Insert(entity)` | Stages entity for insert; call `SaveChanges()` to persist |
| `InsertNotExists(predicate, entity)` | Idempotent insert — returns existing if found |
| `Update(entity)` | Marks entity as modified |
| `Delete(entity)` | Stages entity for deletion |
| `Delete(IEnumerable<T>)` | Stages a collection for deletion |
| `SaveChanges()` | Commits all pending changes; returns `true` if rows affected |

Create domain-specific interfaces that extend `IBaseRepository<T>`:

```csharp
public interface IOrderRepository : IBaseRepository<Order>
{
    Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken ct = default);
}
```

---

## Application Layer

Coordinates **use cases** using Domain objects and repository ports. Never contains business rules — those belong in entities.

### `Result` and `ResultGeneric<T>`

Every handler returns a `Result` or `ResultGeneric<T>` to provide a uniform response contract:

```csharp
// Failure
return Result.Failure("Email already exists.");

// Success with value
return ResultGeneric<Guid>.Success(customer.Id);
```

### Commands — CQRS Write Side

A **Command** is an intent to change state. It uses `ICommand` / `ICommandHandler<TCommand, TResult>`:

```csharp
// Define the command
public record CreateCustomerCommand(string Name, string Email) : ICommand<Guid>;

// Implement the handler
public class CreateCustomerCommandHandler(ICustomerRepository repo)
    : ICommandHandler<CreateCustomerCommand, Guid>
{
    public async Task<ResultGeneric<Guid>> HandleAsync(CreateCustomerCommand cmd, CancellationToken ct)
    {
        var customer = Customer.Create(cmd.Name, cmd.Email);
        repo.Insert(customer);
        repo.SaveChanges();
        return ResultGeneric<Guid>.Success(customer.Id);
    }
}
```

**Rules:**
- Commands **mutate** state, never return domain entities.
- Handlers are processed 1-to-1 (one command → one handler).
- Register in DI: `services.AddScoped<ICommandHandler<CreateCustomerCommand, Guid>, CreateCustomerCommandHandler>()`

### Queries — CQRS Read Side

A **Query** reads state without side effects. It uses `IQuery<TResult>` / `IQueryHandler<TQuery, TResult>`:

```csharp
public record GetCustomerByIdQuery(Guid CustomerId) : IQuery<CustomerDto>;
```

The handler projects the domain entity to a **DTO** — never expose raw entities outside the application layer.

### `IApplicationService`

Marker interface for Application Services — higher-level orchestrators that may coordinate multiple use cases. In a strict CQRS design, prefer individual `CommandHandler`/`QueryHandler` classes per use case over a monolithic service.

---

## Infrastructure Layer

Implements all **ports** defined in the Domain layer. Depends on both Domain and EF Core.

### `ApplicationDbContext`

The EF Core `DbContext`. Add `DbSet<T>` for each aggregate root and configure mappings in `OnModelCreating`:

```csharp
public DbSet<Customer> Customers => Set<Customer>();

protected override void OnModelCreating(ModelBuilder builder)
{
    builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
}
```

### `BaseRepository<T>`

Provides full generic CRUD for any entity. Concrete repositories inherit from it and implement domain-specific interfaces:

```csharp
public class OrderRepository(ApplicationDbContext ctx)
    : BaseRepository<Order>(ctx), IOrderRepository
{
    public async Task<IEnumerable<Order>> GetPendingOrdersAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking().Where(o => o.Status == OrderStatus.Pending).ToListAsync(ct);
}
```

**Key patterns demonstrated in `CustomerRepository`:**
- `FindByEmailAsync` — case-insensitive lookup with `AsNoTracking()` for read performance
- `GetAllActiveAsync` — filtered query via `Where` predicate

### DI Registration

Register infrastructure services in `Program.cs` (or a dedicated extension method):

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICommandHandler<CreateCustomerCommand, Guid>, CreateCustomerCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetCustomerByIdQuery, CustomerDto>, GetCustomerByIdHandler>();
```

---

## Presentation Layer

Thin HTTP layer — controllers accept input, forward to handlers, and return HTTP responses.

### `CustomersController` (sample)

Shows the full REST pattern with API versioning:

```
GET  /api/v1/customers/{id}  → 200 OK (CustomerDto) | 404 Not Found
POST /api/v1/customers       → 201 Created (Guid)   | 400 Bad Request
```

**Controller rules:**
- Zero business logic.
- Always delegates to a command/query handler.
- Returns `Result`-based responses with appropriate HTTP status codes.

### `ExceptionHandler`

Global exception handler converts unhandled exceptions to [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) **ProblemDetails** responses:
- `ArgumentException` → `400 Bad Request`
- All others → `500 Internal Server Error`

### Swagger + API Versioning

Swagger UI is available at `/swagger` in Development mode. API versioning is configured via URL segment (`/api/v1/`) and `X-Api-Version` header.

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or update `ApplicationDbContext` to use another provider)

### Setup

```bash
git clone https://github.com/MarcosFerreira17/clean-architecture-boilerplate.git
cd clean-architecture-boilerplate
dotnet restore
```

Configure your database connection in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=BoilerplateDb;Trusted_Connection=True;"
  }
}
```

Run the application:

```bash
dotnet run --project src/Host/Boilerplate.WebApi
```

The API is available at `http://localhost:5000`. Swagger UI at `http://localhost:5000/swagger`.

---

## Testing

Unit tests live in `src/Tests/Unit.Tests`. Run with:

```bash
dotnet test
```

### Recommended Test Targets

| Layer | What to test |
|---|---|
| Domain | Entity factory methods, value object validation, domain event assertions |
| Application | Command handler flows, query projections, boundary conditions |
| Infrastructure | Repository queries against an in-memory or test DB |

---

## Deployment

```bash
dotnet publish src/Host/Boilerplate.WebApi --configuration Release --output ./publish
```

Docker:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0
COPY ./publish /app
WORKDIR /app
ENTRYPOINT ["dotnet", "Boilerplate.WebApi.dll"]
```

---

## Contributing

1. Fork the repository.
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Commit changes: `git commit -m "Add my feature"`
4. Push to your fork: `git push origin feature/my-feature`
5. Submit a pull request.

Please include tests and follow the existing code style.