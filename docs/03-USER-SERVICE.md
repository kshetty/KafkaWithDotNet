# User Service Implementation - Clean Architecture with SOLID Principles

## Overview

The User Service is built using **Domain-Driven Design (DDD)** with **Clean Architecture**, explicitly applying **SOLID principles** throughout. It implements the **CQRS pattern** with MediatR for clear separation of read and write operations. This document details the implementation of all four Clean Architecture layers with SOLID design patterns.

## Clean Architecture Layers

```
UserService/
├── UserService.Domain/              # ENTERPRISE BUSINESS RULES
│   └── (ZERO dependencies - Pure domain logic)
│
├── UserService.Application/         # APPLICATION BUSINESS RULES
│   └── (Depends ONLY on Domain)
│
├── UserService.Infrastructure/      # INTERFACE ADAPTERS
│   └── (Depends on Application - Implements interfaces)
│
├── UserService.Presentation/        # FRAMEWORKS & DRIVERS
│   └── (Depends on Application - Entry point)
│
└── UserService.Tests/               # Testing
    ├── Unit/                        # Domain & Application tests
    ├── Integration/                 # Full system tests
    └── Architecture/                # Architecture rule tests
```

### Dependency Rule

**Critical**: Dependencies flow INWARD only:

- ✅ Presentation → Application → Domain
- ✅ Infrastructure → Application → Domain
- ❌ Domain NEVER depends on outer layers
- ❌ Application NEVER depends on Infrastructure/Presentation

## SOLID Principles Applied

### 1. Single Responsibility Principle (SRP)

- **Each class has ONE reason to change**
- Commands handle only one operation (SignupUser, SigninUser)
- Repositories handle only data access
- Services have focused, single responsibilities

### 2. Open/Closed Principle (OCP)

- **Open for extension, closed for modification**
- Use interfaces and abstractions
- Strategy pattern for algorithms (IPasswordHasher)
- Pipeline behaviors for cross-cutting concerns

### 3. Liskov Substitution Principle (LSP)

- **Subtypes are substitutable for base types**
- Interface implementations are interchangeable
- Mock implementations for testing
- Polymorphic behavior without breaking contracts

### 4. Interface Segregation Principle (ISP)

- **Clients don't depend on unused methods**
- Focused, role-based interfaces
- `IUserWriteRepository` vs `IUserReadRepository`
- Granular service interfaces

### 5. Dependency Inversion Principle (DIP)

- **Depend on abstractions, not concretions**
- All dependencies injected via interfaces
- Infrastructure implements Application/Domain interfaces
- IoC container manages lifetimes

## Project Structure (Detailed)

```
UserService/
├── UserService.Domain/                      # Layer 1: Enterprise Business Rules
│   ├── Entities/
│   │   ├── User.cs                          # Aggregate Root
│   │   ├── UserSession.cs                   # Entity
│   │   └── Entity.cs                        # Base class
│   ├── ValueObjects/
│   │   ├── Email.cs                         # SRP: Email validation
│   │   ├── PasswordHash.cs                  # SRP: Password representation
│   │   └── ValueObject.cs                   # Base class
│   ├── Events/
│   │   ├── IDomainEvent.cs                  # ISP: Event marker interface
│   │   ├── UserRegisteredEvent.cs
│   │   ├── UserSignedInEvent.cs
│   │   └── UserSignedOutEvent.cs
│   ├── Exceptions/
│   │   ├── DomainException.cs
│   │   ├── UserNotFoundException.cs
│   │   └── InvalidEmailException.cs
│   └── Common/
│       └── IEntity.cs
│
├── UserService.Application/                 # Layer 2: Application Business Rules
│   ├── Commands/
│   │   ├── SignupUser/
│   │   │   ├── SignupUserCommand.cs         # SRP: Signup request
│   │   │   ├── SignupUserCommandHandler.cs  # SRP: Signup logic
│   │   │   └── SignupUserValidator.cs       # SRP: Input validation
│   │   ├── SigninUser/
│   │   │   ├── SigninUserCommand.cs
│   │   │   ├── SigninUserCommandHandler.cs
│   │   │   └── SigninUserValidator.cs
│   │   └── SignoutUser/
│   │       ├── SignoutUserCommand.cs
│   │       └── SignoutUserCommandHandler.cs
│   ├── Queries/
│   │   └── GetUserProfile/
│   │       ├── GetUserProfileQuery.cs       # SRP: Query request
│   │       ├── GetUserProfileQueryHandler.cs # SRP: Query logic
│   │       └── UserProfileDto.cs            # Data transfer
│   ├── DTOs/
│   │   ├── UserDto.cs
│   │   ├── UserSessionDto.cs
│   │   └── TokenResponseDto.cs
│   ├── Interfaces/                          # DIP: Abstractions defined here
│   │   ├── Repositories/
│   │   │   ├── IUserWriteRepository.cs      # ISP: Write operations
│   │   │   └── IUserReadRepository.cs       # ISP: Read operations
│   │   ├── Services/
│   │   │   ├── ITokenService.cs             # ISP: Token operations
│   │   │   ├── ISessionService.cs           # ISP: Session management
│   │   │   ├── IPasswordHasher.cs           # OCP: Strategy pattern
│   │   │   └── IEventPublisher.cs           # ISP: Event publishing
│   │   └── Infrastructure/
│   │       └── IUnitOfWork.cs               # Transaction boundary
│   ├── Validators/
│   │   └── FluentValidation rules
│   └── Behaviors/
│       ├── ValidationBehavior.cs            # Pipeline: Validate all commands
│       ├── LoggingBehavior.cs               # Pipeline: Log all operations
│       └── TransactionBehavior.cs           # Pipeline: Wrap in transaction
│
├── UserService.Infrastructure/              # Layer 3: Interface Adapters
│   ├── Persistence/
│   │   ├── UserDbContext.cs                 # EF Core context
│   │   ├── Configurations/
│   │   │   ├── UserConfiguration.cs         # EF configuration
│   │   │   └── UserSessionConfiguration.cs
│   │   └── Migrations/
│   ├── Repositories/
│   │   ├── UserRepository.cs                # DIP: Implements IUserWriteRepository
│   │   └── UserReadRepository.cs            # DIP: Implements IUserReadRepository
│   ├── Services/
│   │   ├── TokenService.cs                  # DIP: Implements ITokenService
│   │   ├── SessionService.cs                # DIP: Implements ISessionService
│   │   ├── BcryptPasswordHasher.cs          # DIP: Implements IPasswordHasher
│   │   └── KafkaEventPublisher.cs           # DIP: Implements IEventPublisher
│   ├── Kafka/
│   │   ├── KafkaProducerService.cs
│   │   └── KafkaConfiguration.cs
│   └── Configuration/
│       └── DependencyInjection.cs           # IoC registration
│
├── UserService.Presentation/                # Layer 4: Frameworks & Drivers
│   ├── Controllers/
│   │   └── UsersController.cs               # REST API endpoints
│   ├── Middleware/
│   │   ├── ExceptionHandlingMiddleware.cs   # Error handling
│   │   └── AuthenticationMiddleware.cs      # JWT validation
│   ├── Filters/
│   │   └── ValidationFilter.cs              # Model validation
│   ├── Extensions/
│   │   ├── ServiceCollectionExtensions.cs
│   │   └── ApplicationBuilderExtensions.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Dockerfile
│   └── Program.cs                           # Application entry point
│
└── UserService.Tests/
    ├── Unit/
    │   ├── Domain/                          # Test domain logic
    │   │   ├── UserTests.cs
    │   │   └── EmailTests.cs
    │   └── Application/                     # Test handlers
    │       ├── SignupUserCommandHandlerTests.cs
    │       └── SigninUserCommandHandlerTests.cs
    ├── Integration/
    │   ├── Controllers/                     # Test full stack
    │   │   └── UsersControllerTests.cs
    │   └── Repositories/
    │       └── UserRepositoryTests.cs
    └── Architecture/
        └── ArchitectureTests.cs             # Enforce rules
```

---

## 🎯 SOLID Principles in Action

### 1. Single Responsibility Principle (SRP)

**Definition**: A class should have only ONE reason to change.

#### ✅ Good Examples:

```csharp
// ✅ SignupUserCommand - Single responsibility: Capture signup request
public record SignupUserCommand(string Email, string Password, string FirstName, string LastName)
    : IRequest<Result<TokenResponseDto>>;

// ✅ SignupUserCommandHandler - Single responsibility: Handle signup logic
public class SignupUserCommandHandler : IRequestHandler<SignupUserCommand, Result<TokenResponseDto>>
{
    // Handles ONLY signup orchestration
}

// ✅ SignupUserValidator - Single responsibility: Validate signup input
public class SignupUserValidator : AbstractValidator<SignupUserCommand>
{
    // Validates ONLY signup data
}
```

#### ❌ Bad Example (Violates SRP):

```csharp
// ❌ UserService does TOO MUCH - signup, signin, validation, email, logging
public class UserService
{
    public async Task<User> Signup(string email, string password)
    {
        // Validates email
        // Hashes password
        // Saves to database
        // Sends email
        // Logs activity
        // Generates token
        // ALL IN ONE METHOD!
    }
}
```

---

### 2. Open/Closed Principle (OCP)

**Definition**: Classes should be open for extension but closed for modification.

#### ✅ Good Example: Strategy Pattern for Password Hashing

```csharp
// ✅ Interface allows extension without modifying existing code
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

// ✅ Implementation 1: BCrypt
public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}

// ✅ Implementation 2: Argon2 (NEW - No existing code modified!)
public class Argon2PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => Argon2.Hash(password);
    public bool Verify(string password, string hash) => Argon2.Verify(hash, password);
}

// Usage: Switch implementations via configuration
services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
// OR
services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
```

#### ✅ Good Example: Pipeline Behaviors (Decorator Pattern)

```csharp
// ✅ Add cross-cutting concerns WITHOUT modifying handlers
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, /* ... */)
    {
        // Validate ALL commands automatically
    }
}

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, /* ... */)
    {
        // Log ALL operations automatically
    }
}

// New behavior added WITHOUT modifying existing handlers!
public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, /* ... */)
    {
        // Wrap ALL commands in transaction automatically
    }
}
```

---

### 3. Liskov Substitution Principle (LSP)

**Definition**: Objects of a superclass should be replaceable with objects of a subclass without breaking the application.

#### ✅ Good Example: Repository Substitution

```csharp
// ✅ Interface contract
public interface IUserWriteRepository
{
    Task<User> AddAsync(User user, CancellationToken ct);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
}

// ✅ Production implementation
public class UserRepository : IUserWriteRepository
{
    private readonly ApplicationDbContext _context;

    public async Task<User> AddAsync(User user, CancellationToken ct)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
        return user;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }
}

// ✅ Test implementation - FULLY SUBSTITUTABLE
public class InMemoryUserRepository : IUserWriteRepository
{
    private readonly List<User> _users = new();

    public async Task<User> AddAsync(User user, CancellationToken ct)
    {
        _users.Add(user);
        return await Task.FromResult(user);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return await Task.FromResult(user);
    }
}

// ✅ Usage: Both implementations work identically
IUserWriteRepository repo = new UserRepository(context);     // Production
IUserWriteRepository repo = new InMemoryUserRepository();    // Testing
```

#### ❌ Bad Example (Violates LSP):

```csharp
// ❌ Subclass changes behavior unexpectedly
public class ReadOnlyUserRepository : IUserWriteRepository
{
    public async Task<User> AddAsync(User user, CancellationToken ct)
    {
        throw new NotSupportedException("This repository is read-only!");
        // Violates LSP - unexpected behavior!
    }
}
```

---

### 4. Interface Segregation Principle (ISP)

**Definition**: Clients should not be forced to depend on interfaces they don't use.

#### ✅ Good Example: Segregated Repository Interfaces

```csharp
// ✅ Focused interfaces - clients depend ONLY on what they need

// Write operations
public interface IUserWriteRepository
{
    Task<User> AddAsync(User user, CancellationToken ct);
    Task UpdateAsync(User user, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}

// Read operations
public interface IUserReadRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<List<User>> GetAllAsync(CancellationToken ct);
}

// Specialized query
public interface IUserSessionRepository
{
    Task<UserSession?> GetByRefreshTokenAsync(string token, CancellationToken ct);
    Task<List<UserSession>> GetActiveSessionsAsync(Guid userId, CancellationToken ct);
}

// Usage: Inject ONLY what you need
public class SignupUserCommandHandler
{
    private readonly IUserWriteRepository _writeRepo;  // ✅ Needs only writes

    public SignupUserCommandHandler(IUserWriteRepository writeRepo)
    {
        _writeRepo = writeRepo;
    }
}

public class GetUserProfileQueryHandler
{
    private readonly IUserReadRepository _readRepo;  // ✅ Needs only reads

    public GetUserProfileQueryHandler(IUserReadRepository readRepo)
    {
        _readRepo = readRepo;
    }
}
```

#### ❌ Bad Example (Violates ISP):

```csharp
// ❌ Fat interface - forces clients to depend on unused methods
public interface IUserRepository
{
    // Write operations
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);

    // Read operations
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);

    // Session operations
    Task<UserSession?> GetSessionAsync(string token);

    // Bulk operations
    Task BulkInsertAsync(List<User> users);

    // Analytics
    Task<int> GetUserCountAsync();
    Task<List<User>> GetRecentUsersAsync();
}

// ❌ Query handler forced to depend on write methods it doesn't use!
public class GetUserProfileQueryHandler
{
    private readonly IUserRepository _repo;  // ❌ Has write methods it doesn't need
}
```

---

### 5. Dependency Inversion Principle (DIP)

**Definition**: High-level modules should not depend on low-level modules. Both should depend on abstractions.

#### ✅ Good Example: Application Layer Defines Interfaces

```csharp
// APPLICATION LAYER (UserService.Application/Interfaces/ITokenService.cs)
// ✅ High-level module defines the abstraction it needs
namespace UserService.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
}

// APPLICATION LAYER (Commands/SigninUserCommandHandler.cs)
// ✅ Depends on abstraction (interface), NOT concrete implementation
public class SigninUserCommandHandler : IRequestHandler<SigninUserCommand, Result<TokenResponseDto>>
{
    private readonly IUserReadRepository _userRepo;
    private readonly ITokenService _tokenService;  // ✅ Abstraction
    private readonly IPasswordHasher _passwordHasher;  // ✅ Abstraction

    public SigninUserCommandHandler(
        IUserReadRepository userRepo,
        ITokenService tokenService,
        IPasswordHasher passwordHasher)
    {
        _userRepo = userRepo;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<TokenResponseDto>> Handle(/* ... */)
    {
        // Uses abstractions - doesn't care about JWT, BCrypt, EF Core
        var user = await _userRepo.GetByEmailAsync(request.Email, ct);
        var isValid = _passwordHasher.Verify(request.Password, user.PasswordHash.Value);
        var token = _tokenService.GenerateAccessToken(user.Id, user.Email.Value);
    }
}

// INFRASTRUCTURE LAYER (Services/TokenService.cs)
// ✅ Low-level module implements the interface defined in Application layer
namespace UserService.Infrastructure.Services;

public class TokenService : ITokenService  // ✅ Implements Application interface
{
    public string GenerateAccessToken(Guid userId, string email)
    {
        // JWT implementation details - Application layer doesn't know!
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);
        // ...
    }
}

// INFRASTRUCTURE LAYER (Configuration/DependencyInjection.cs)
// ✅ Wire up implementations at runtime
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IUserWriteRepository, UserRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        return services;
    }
}
```

#### ❌ Bad Example (Violates DIP):

```csharp
// ❌ High-level module depends on low-level concrete implementation
public class SigninUserCommandHandler
{
    private readonly UserRepository _userRepo;  // ❌ Concrete class
    private readonly BcryptPasswordHasher _hasher;  // ❌ Concrete class

    public SigninUserCommandHandler()
    {
        _userRepo = new UserRepository();  // ❌ Direct instantiation
        _hasher = new BcryptPasswordHasher();  // ❌ Direct instantiation
    }
}
```

---

## Layer 1: Domain (Enterprise Business Rules)

### ✅ SOLID in Domain Layer

**SRP**: Each entity has ONE business responsibility
**OCP**: Domain events allow extension without modifying entities
**LSP**: All entities inherit from base Entity class consistently
**ISP**: Domain interfaces are focused (IDomainEvent, IEntity)
**DIP**: Domain has ZERO dependencies on outer layers

### Entities

#### User Entity (`Entities/User.cs`)

```csharp
namespace UserService.Domain.Entities;

/// <summary>
/// User Aggregate Root - SRP: Manages user identity and authentication
/// </summary>
public class User : Entity
{
    public Guid Id { get; private set; }
    public Email Email { get; private set; }               // Value Object
    public PasswordHash PasswordHash { get; private set; }  // Value Object
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? EntraExternalId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsEmailVerified { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private User() { } // For EF Core

    public static User Register(
        Email email,
        PasswordHash passwordHash,
        string firstName,
        string lastName,
        string? entraExternalId = null)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            EntraExternalId = entraExternalId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            IsEmailVerified = !string.IsNullOrEmpty(entraExternalId) // Auto-verified if from Entra
        };

        user._domainEvents.Add(new UserRegisteredEvent(user.Id, user.Email.Value));
        return user;
    }

    public void Authenticate()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new UserSignedInEvent(Id, Email.Value));
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new UserProfileUpdatedEvent(Id));
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new UserDeactivatedEvent(Id, Email.Value));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

#### UserSession Entity (`Entities/UserSession.cs`)

```csharp
public class UserSession : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string RefreshTokenHash { get; private set; }
    public string DeviceInfo { get; private set; }
    public string IpAddress { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public bool IsRevoked { get; private set; }

    public User User { get; private set; }

    private UserSession() { }

    public static UserSession Create(
        Guid userId,
        string refreshTokenHash,
        string deviceInfo,
        string ipAddress,
        TimeSpan expiry)
    {
        return new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RefreshTokenHash = refreshTokenHash,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(expiry),
            LastActivityAt = DateTime.UtcNow,
            IsRevoked = false
        };
    }

    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        IsRevoked = true;
    }

    public bool IsValid()
    {
        return !IsRevoked && ExpiresAt > DateTime.UtcNow;
    }
}
```

### Value Objects

#### Email Value Object

```csharp
public class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email cannot be empty");

        if (!IsValidEmail(email))
            throw new DomainException($"Email '{email}' is not valid");

        return new Email(email.ToLowerInvariant());
    }

    private static bool IsValidEmail(string email)
    {
        var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return regex.IsMatch(email);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

#### PasswordHash Value Object

```csharp
public class PasswordHash : ValueObject
{
    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    public static PasswordHash Create(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("Password hash cannot be empty");

        return new PasswordHash(hash);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
```

### Domain Events

```csharp
public record UserRegisteredEvent(Guid UserId, string Email) : IDomainEvent;
public record UserSignedInEvent(Guid UserId, string Email) : IDomainEvent;
public record UserSignedOutEvent(Guid UserId, string Email) : IDomainEvent;
public record UserProfileUpdatedEvent(Guid UserId) : IDomainEvent;
public record UserDeactivatedEvent(Guid UserId, string Email) : IDomainEvent;
```

### Repository Interface

```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByEntraIdAsync(string entraId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<UserSession?> GetSessionByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task AddSessionAsync(UserSession session, CancellationToken cancellationToken = default);
    Task RevokeSessionAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<List<UserSession>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
```

---

## Application Layer (CQRS)

### Commands

#### SignupUserCommand

```csharp
public record SignupUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? EntraExternalId,
    string DeviceInfo,
    string IpAddress
) : IRequest<AuthenticationResult>;

public class SignupUserCommandValidator : AbstractValidator<SignupUserCommand>
{
    public SignupUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}

public class SignupUserCommandHandler : IRequestHandler<SignupUserCommand, AuthenticationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ISessionService _sessionService;
    private readonly IEventPublisher _eventPublisher;

    public async Task<AuthenticationResult> Handle(
        SignupUserCommand request,
        CancellationToken cancellationToken)
    {
        // Check if user exists
        if (await _userRepository.ExistsAsync(request.Email, cancellationToken))
            throw new UserAlreadyExistsException(request.Email);

        // Create domain objects
        var email = Email.Create(request.Email);
        var passwordHash = PasswordHash.Create(_passwordHasher.Hash(request.Password));

        // Create user entity
        var user = User.Register(
            email,
            passwordHash,
            request.FirstName,
            request.LastName,
            request.EntraExternalId);

        // Persist
        await _userRepository.AddAsync(user, cancellationToken);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Create session
        await _sessionService.CreateSessionAsync(
            user.Id,
            refreshToken,
            request.DeviceInfo,
            request.IpAddress,
            TimeSpan.FromDays(7),
            cancellationToken);

        // Publish domain events
        foreach (var domainEvent in user.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }
        user.ClearDomainEvents();

        return new AuthenticationResult(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            accessToken,
            refreshToken);
    }
}
```

#### SigninUserCommand

```csharp
public record SigninUserCommand(
    string Email,
    string Password,
    string DeviceInfo,
    string IpAddress
) : IRequest<AuthenticationResult>;

public class SigninUserCommandHandler : IRequestHandler<SigninUserCommand, AuthenticationResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ISessionService _sessionService;
    private readonly IEventPublisher _eventPublisher;

    public async Task<AuthenticationResult> Handle(
        SigninUserCommand request,
        CancellationToken cancellationToken)
    {
        // Get user
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new InvalidCredentialsException();

        // Verify password
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash.Value))
            throw new InvalidCredentialsException();

        // Check if user is active
        if (!user.IsActive)
            throw new UserInactiveException();

        // Update user
        user.Authenticate();
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Create session
        await _sessionService.CreateSessionAsync(
            user.Id,
            refreshToken,
            request.DeviceInfo,
            request.IpAddress,
            TimeSpan.FromDays(7),
            cancellationToken);

        // Publish events
        foreach (var domainEvent in user.DomainEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);
        }
        user.ClearDomainEvents();

        return new AuthenticationResult(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            accessToken,
            refreshToken);
    }
}
```

### Queries

#### GetUserProfileQuery

```csharp
public record GetUserProfileQuery(Guid UserId) : IRequest<UserProfileDto>;

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;

    public async Task<UserProfileDto> Handle(
        GetUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        // Try cache first
        var cacheKey = $"user:profile:{request.UserId}";
        var cached = await _cacheService.GetAsync<UserProfileDto>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        // Get from database
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new UserNotFoundException(request.UserId);

        var dto = new UserProfileDto(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.CreatedAt,
            user.LastLoginAt,
            user.IsEmailVerified);

        // Cache for 15 minutes
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(15), cancellationToken);

        return dto;
    }
}
```

---

## Infrastructure Layer

### UserRepository Implementation

```csharp
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.Value == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<bool> ExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.Value == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    // Session methods...
}
```

### TokenService Implementation

```csharp
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
```

### SessionService (Redis) Implementation

```csharp
public class SessionService : ISessionService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IUserRepository _userRepository;

    public async Task CreateSessionAsync(
        Guid userId,
        string refreshToken,
        string deviceInfo,
        string ipAddress,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var tokenHash = ComputeHash(refreshToken);

        var session = new SessionData
        {
            UserId = userId,
            DeviceInfo = deviceInfo,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(session);
        await db.StringSetAsync($"session:{tokenHash}", json, expiry);

        // Also store in PostgreSQL for audit
        var userSession = UserSession.Create(userId, tokenHash, deviceInfo, ipAddress, expiry);
        await _userRepository.AddSessionAsync(userSession, cancellationToken);
    }

    public async Task<bool> ValidateSessionAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var tokenHash = ComputeHash(refreshToken);
        return await db.KeyExistsAsync($"session:{tokenHash}");
    }

    public async Task RevokeSessionAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var tokenHash = ComputeHash(refreshToken);
        await db.KeyDeleteAsync($"session:{tokenHash}");
        await _userRepository.RevokeSessionAsync(tokenHash, cancellationToken);
    }

    private string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
```

---

## Presentation Layer

### UsersController

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost("signup")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthenticationResult>> Signup(
        [FromBody] SignupRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SignupUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.EntraExternalId,
            GetDeviceInfo(),
            GetIpAddress());

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProfile), new { id = result.UserId }, result);
    }

    [HttpPost("signin")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResult>> Signin(
        [FromBody] SigninRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SigninUserCommand(
            request.Email,
            request.Password,
            GetDeviceInfo(),
            GetIpAddress());

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("signout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Signout(
        [FromBody] SignoutRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SignoutUserCommand(GetUserId(), request.RefreshToken);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserProfileDto>> GetProfile(CancellationToken cancellationToken)
    {
        var query = new GetUserProfileQuery(GetUserId());
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenRefreshResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenRefreshResult>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string GetDeviceInfo() =>
        Request.Headers.UserAgent.ToString();

    private string GetIpAddress() =>
        HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
}
```

---

## API Endpoints

| Method | Endpoint              | Description              | Auth Required |
| ------ | --------------------- | ------------------------ | ------------- |
| POST   | `/api/users/signup`   | Register new user        | No            |
| POST   | `/api/users/signin`   | Authenticate user        | No            |
| POST   | `/api/users/signout`  | Logout user              | Yes           |
| GET    | `/api/users/me`       | Get current user profile | Yes           |
| PUT    | `/api/users/me`       | Update user profile      | Yes           |
| POST   | `/api/users/refresh`  | Refresh access token     | No            |
| GET    | `/api/users/sessions` | Get active sessions      | Yes           |
| GET    | `/health`             | Health check             | No            |

---

**Next**: See [04-FRONTEND.md](04-FRONTEND.md) for React application details.
