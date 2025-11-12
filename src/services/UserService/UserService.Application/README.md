# UserService.Application

## Overview

The Application Layer contains the business logic and orchestration of the UserService. It implements the **CQRS (Command Query Responsibility Segregation)** pattern using **MediatR** and provides a clean separation between read and write operations.

## Architecture

This layer follows **Clean Architecture** principles:

- **No dependencies** on Infrastructure or Presentation layers
- **Depends only** on the Domain layer
- Uses **Dependency Inversion** (interfaces defined here, implemented in Infrastructure)
- Fully **testable** without external dependencies

## Project Structure

```
UserService.Application/
├── Common/
│   ├── Behaviors/              # MediatR pipeline behaviors
│   │   ├── LoggingBehavior.cs
│   │   ├── ValidationBehavior.cs
│   │   ├── PerformanceBehavior.cs
│   │   └── TransactionBehavior.cs
│   ├── DTOs/                   # Data Transfer Objects
│   │   ├── UserDto.cs
│   │   ├── AuthenticationTokensDto.cs
│   │   ├── AuthenticationResultDto.cs
│   │   └── UserSessionDto.cs
│   └── Interfaces/             # Service abstractions
│       ├── Persistence/
│       │   ├── IRepository.cs
│       │   ├── IUserRepository.cs
│       │   ├── IUserSessionRepository.cs
│       │   └── IUnitOfWork.cs
│       └── Services/
│           ├── ITokenService.cs
│           ├── IEventPublisher.cs
│           ├── ICacheService.cs
│           └── IDateTimeProvider.cs
├── Features/
│   ├── Authentication/
│   │   └── Commands/
│   │       ├── SignUp/
│   │       ├── SignIn/
│   │       ├── SignOut/
│   │       └── RefreshToken/
│   └── Users/
│       └── Queries/
│           ├── GetUserProfile/
│           └── GetUserSessions/
└── DependencyInjection.cs
```

## Key Patterns

### 1. CQRS with MediatR

**Commands** - Write operations that modify state:

```csharp
public record SignUpCommand(
    string Email,
    string Password,
    string FullName) : IRequest<AuthenticationResultDto>;
```

**Queries** - Read operations that return data:

```csharp
public record GetUserProfileQuery(Guid UserId) : IRequest<UserDto>;
```

### 2. Pipeline Behaviors

MediatR behaviors provide cross-cutting concerns:

- **LoggingBehavior**: Logs all requests and responses
- **ValidationBehavior**: Automatic FluentValidation before handlers
- **PerformanceBehavior**: Warns on slow requests (>500ms)
- **TransactionBehavior**: Wraps commands in database transactions

Execution order:

```
Request → Logging → Validation → Performance → Transaction → Handler
```

### 3. Validation with FluentValidation

Each command/query has a validator:

```csharp
public class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
    public SignUpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"[0-9]").WithMessage("Password must contain a digit")
            .Matches(@"[A-Z]").WithMessage("Password must contain an uppercase letter");
    }
}
```

### 4. Repository Pattern

Generic repository with specialized interfaces:

```csharp
public interface IRepository<TEntity, TId> where TEntity : Entity<TId>
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
```

### 5. Unit of Work Pattern

Manages transactions and coordinates repositories:

```csharp
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IUserSessionRepository UserSessions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
}
```

## Features

### Authentication

#### SignUp

Creates a new user account with email/password.

- Validates email uniqueness
- Enforces password policy
- Creates initial session
- Publishes `UserRegisteredEvent` to Kafka
- Returns authentication tokens

#### SignIn

Authenticates existing user.

- Validates credentials
- Implements account lockout (5 failed attempts)
- Records login attempts
- Creates new session
- Publishes `UserSignedInEvent`
- Returns authentication tokens

#### SignOut

Terminates a user session.

- Validates user and session
- Revokes session
- Publishes `UserSignedOutEvent`

#### RefreshToken

Refreshes authentication without password.

- Validates refresh token
- Checks session validity
- Generates new tokens
- Updates session last used time

### Users

#### GetUserProfile

Retrieves user information with caching.

- Checks Redis cache first (5-minute TTL)
- Falls back to database
- Returns `UserDto`

#### GetUserSessions

Lists all active sessions for a user.

- Returns session details (IP, user agent, creation time)
- Useful for security dashboard

## DTOs

All responses use immutable record types:

```csharp
public record UserDto(
    Guid Id,
    string Email,
    string FullName,
    bool IsEmailVerified,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public record AuthenticationTokensDto(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn);
```

## Usage

### Registration

Add to `Program.cs`:

```csharp
builder.Services.AddApplication();
```

### Sending Commands/Queries

Inject `IMediator` into controllers:

```csharp
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("signup")]
    public async Task<ActionResult<AuthenticationResultDto>> SignUp(
        SignUpCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
```

## Dependencies

Required NuGet packages:

- **MediatR** (12.x): CQRS implementation
- **FluentValidation** (11.x): Declarative validation
- **FluentValidation.DependencyInjectionExtensions**: DI integration

## Error Handling

Validation errors throw `FluentValidation.ValidationException` with detailed error messages.

Domain exceptions are defined in the Domain layer and should be caught in the Presentation layer middleware.

## Performance Considerations

- **Caching**: GetUserProfile uses Redis with 5-minute TTL
- **Queries**: Read-only database queries for better performance
- **Async/Await**: All operations are asynchronous
- **Performance Logging**: Automatic warnings for slow requests (>500ms)

## Security

- **Password Validation**: Enforces strong password policy
- **Account Lockout**: 5 failed attempts lock account for 15 minutes
- **Token Expiry**: Access tokens 1 hour, refresh tokens 7 days
- **Session Management**: Track and revoke sessions by device

## Event-Driven Architecture

Commands publish domain events to Kafka:

- `UserRegisteredEvent`
- `UserSignedInEvent`
- `UserSignedOutEvent`
- `EmailVerifiedEvent`

These events can be consumed by:

- Email service (send welcome email)
- Analytics service (track user behavior)
- Audit service (compliance logging)

## Testing

The Application layer is highly testable:

```csharp
// Mock dependencies
var unitOfWork = Substitute.For<IUnitOfWork>();
var tokenService = Substitute.For<ITokenService>();

// Test handler
var handler = new SignUpCommandHandler(unitOfWork, tokenService, eventPublisher);
var result = await handler.Handle(command, CancellationToken.None);

// Verify
result.Should().NotBeNull();
unitOfWork.Users.Received(1).AddAsync(Arg.Any<User>());
```

## Best Practices

1. **Keep handlers thin**: Complex business logic belongs in Domain entities
2. **Use DTOs**: Never expose domain entities outside this layer
3. **Validate everything**: FluentValidation ensures data integrity
4. **Publish domain events**: Enable event-driven workflows
5. **Use cancellation tokens**: Support request cancellation
6. **Cache read operations**: Improve query performance

## Next Steps

To complete the application:

1. Implement Infrastructure layer (repositories, services)
2. Create Presentation layer (controllers, middleware)
3. Add database migrations
4. Configure authentication in `Program.cs`
5. Add integration tests
