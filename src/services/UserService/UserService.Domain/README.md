# UserService.Domain

The Domain Layer is the core of the application, containing all business logic, domain entities, value objects, domain events, and exceptions. This layer is completely independent of external concerns and frameworks, following Domain-Driven Design (DDD) principles and SOLID design patterns.

## 📁 Structure

```
UserService.Domain/
├── Primitives/           # Base classes and interfaces
│   ├── Entity.cs         # Base entity class with identity
│   ├── AggregateRoot.cs  # Base aggregate root with domain events
│   ├── ValueObject.cs    # Base value object with value equality
│   └── IDomainEvent.cs   # Domain event marker interface
├── Entities/             # Domain entities (aggregate roots)
│   ├── User.cs           # User aggregate root
│   └── UserSession.cs    # User session entity
├── ValueObjects/         # Value objects
│   ├── Email.cs          # Email value object with validation
│   └── Password.cs       # Password value object with hashing
├── Events/               # Domain events
│   ├── UserRegisteredEvent.cs
│   ├── UserSignedInEvent.cs
│   ├── UserSignedOutEvent.cs
│   └── UserUpdatedEvent.cs
└── Exceptions/           # Domain exceptions
    ├── DomainException.cs
    ├── UserNotFoundException.cs
    ├── InvalidEmailException.cs
    ├── WeakPasswordException.cs
    ├── DuplicateEmailException.cs
    ├── InvalidPasswordException.cs
    └── InvalidSessionException.cs
```

## 🏗️ Design Principles

### Clean Architecture

- **Zero external dependencies**: No references to Infrastructure, Application, or Presentation layers
- **Framework independence**: No dependency on Entity Framework, ASP.NET, or any external framework
- **Pure business logic**: All code represents business rules and domain concepts

### Domain-Driven Design (DDD)

- **Ubiquitous language**: Domain terms match business terminology
- **Aggregate roots**: User is an aggregate root managing UserSession entities
- **Value objects**: Email and Password are immutable value objects
- **Domain events**: Rich events communicate what happened in the domain
- **Domain exceptions**: Specific exceptions for business rule violations

### SOLID Principles

#### Single Responsibility Principle (SRP)

- Each entity has one reason to change (User for user management, UserSession for session management)
- Value objects focus solely on validation and equality
- Domain events represent single occurrences

#### Open/Closed Principle (OCP)

- Entities are open for extension (inheritance) but closed for modification
- Domain events allow extending behavior without modifying entities
- Value objects are immutable and sealed

#### Liskov Substitution Principle (LSP)

- Derived entities maintain contracts of base Entity class
- AggregateRoot extends Entity without breaking its contract

#### Interface Segregation Principle (ISP)

- IDomainEvent interface is minimal and focused
- No fat interfaces forcing unnecessary implementations

#### Dependency Inversion Principle (DIP)

- Domain layer defines abstractions (not implemented here, but prepared for repository interfaces in Application layer)
- No dependencies on concrete implementations

## 🔑 Key Components

### Primitives

#### Entity<TId>

Base class for all entities with:

- Identity-based equality
- CreatedAt and UpdatedAt timestamps
- Generic ID type support
- Immutable ID after creation

#### AggregateRoot<TId>

Extends Entity to provide:

- Domain event collection and management
- Clear separation between entities and aggregates
- Event publishing capabilities

#### ValueObject

Base class for value objects with:

- Value-based equality (not identity-based)
- Immutability by design
- GetEqualityComponents pattern

#### IDomainEvent

Marker interface for domain events:

- EventId for tracking
- OccurredAt timestamp
- No behavior, just data

### Entities

#### User (Aggregate Root)

**Purpose**: Manages user identity, authentication, and sessions

**Properties**:

- `Email` (Email value object) - Validated email address
- `Password` (Password value object) - Hashed password
- `FullName` (string) - User's display name
- `IsEmailVerified` (bool) - Email verification status
- `IsActive` (bool) - Account active status
- `FailedLoginAttempts` (int) - Login attempt counter
- `LockedOutUntil` (DateTime?) - Account lockout timestamp
- `LastLoginAt` (DateTime?) - Last successful login
- `Sessions` (IReadOnlyCollection<UserSession>) - Active sessions

**Key Methods**:

- `Create()` - Factory method for creating new users
- `VerifyPassword()` - Verify provided password
- `ChangePassword()` - Update user password
- `UpdateProfile()` - Update user information
- `VerifyEmail()` - Mark email as verified
- `Activate()/Deactivate()` - Enable/disable account
- `RecordSuccessfulLogin()` - Create new session on login
- `RecordFailedLogin()` - Track failed attempts and lockout
- `IsLockedOut()` - Check lockout status
- `TerminateSession()` - End specific session
- `TerminateAllSessions()` - End all user sessions

**Domain Events Raised**:

- UserRegisteredEvent (on creation)
- UserSignedInEvent (on successful login)
- UserSignedOutEvent (on session termination)
- UserUpdatedEvent (on profile update)

**Business Rules**:

- Full name required, max 200 characters
- Email must be valid and unique
- Password must meet strength requirements
- Account locks after 5 failed login attempts for 15 minutes
- Cannot login to inactive or locked accounts
- Sessions belong to the user aggregate

#### UserSession (Entity)

**Purpose**: Manages refresh tokens and active sessions

**Properties**:

- `UserId` (Guid) - Foreign key to User
- `RefreshToken` (string) - Cryptographically secure token
- `ExpiresAt` (DateTime) - Token expiration
- `IsActive` (bool) - Session active status
- `IpAddress` (string?) - Client IP address
- `UserAgent` (string?) - Client user agent
- `RevokedAt` (DateTime?) - Revocation timestamp
- `LastUsedAt` (DateTime?) - Last usage timestamp

**Key Methods**:

- `Create()` - Factory method for new sessions
- `IsExpired()` - Check if expired
- `IsValid()` - Check if active and not expired
- `Revoke()` - Invalidate session
- `MarkAsUsed()` - Update last used timestamp
- `Refresh()` - Generate new refresh token

**Business Rules**:

- Refresh tokens expire after 7 days by default
- Tokens are 32-byte random values (Base64 encoded)
- Cannot use revoked or expired sessions
- IP address max 45 chars (IPv6 support)
- User agent max 500 chars

### Value Objects

#### Email

**Purpose**: Encapsulate email validation and normalization

**Features**:

- RFC 5322 compliant validation
- Automatic normalization (lowercase, trim)
- Max length 255 characters
- Domain validation
- TryCreate pattern for non-throwing validation
- Immutable

**Usage**:

```csharp
var email = Email.Create("user@example.com");
bool isValid = Email.IsValid("test@test.com");
if (Email.TryCreate("maybe@email.com", out var result)) { }
```

#### Password

**Purpose**: Handle password validation, hashing, and verification

**Features**:

- PBKDF2 hashing with SHA-256
- 100,000 iterations (OWASP recommended)
- 16-byte salt, 32-byte hash
- Strength requirements:
  - Min 8 characters, max 128
  - At least one digit
  - At least one lowercase letter
  - At least one uppercase letter
  - At least one special character
- Timing-attack resistant verification
- Immutable

**Storage Format**: `{iterations}.{base64_salt}.{base64_hash}`

**Usage**:

```csharp
var password = Password.Create("MyP@ssw0rd123");
bool isValid = password.Verify("MyP@ssw0rd123");
var fromDb = Password.FromHash(storedHash);
```

### Domain Events

#### UserRegisteredEvent

Raised when a new user registers

- UserId
- Email
- FullName

#### UserSignedInEvent

Raised when a user successfully signs in

- UserId
- Email
- SessionId
- IpAddress
- UserAgent

#### UserSignedOutEvent

Raised when a user signs out

- UserId
- Email
- SessionId

#### UserUpdatedEvent

Raised when user profile is updated

- UserId
- Email
- FullName

### Exceptions

#### DomainException (Base)

Base class for all domain exceptions

- ErrorCode property
- Consistent error handling

#### UserNotFoundException

User not found by ID or email

#### InvalidEmailException

Email format or validation failed

#### WeakPasswordException

Password doesn't meet strength requirements

#### DuplicateEmailException

Email already exists in system

#### InvalidPasswordException

Incorrect password during authentication

#### InvalidSessionException

Session not found or expired

## 🔐 Security Features

### Password Security

- **PBKDF2-SHA256**: Industry standard hashing
- **100,000 iterations**: Protection against brute force
- **Random salts**: Each password has unique salt
- **Timing-attack resistant**: Constant-time comparison
- **Never store plain text**: Only hashed values

### Session Security

- **Cryptographically secure tokens**: 32-byte random values
- **Token expiration**: Time-limited sessions
- **Revocation support**: Immediate session invalidation
- **IP and User Agent tracking**: Audit trail
- **Multiple session management**: Track all active sessions

### Account Protection

- **Failed login tracking**: Monitor suspicious activity
- **Automatic lockout**: Protection against brute force (5 attempts)
- **Time-based lockout**: Auto-unlock after 15 minutes
- **Account deactivation**: Admin can disable accounts

## 📊 Domain Model Relationships

```
User (Aggregate Root)
├── Email (Value Object)
├── Password (Value Object)
└── Sessions (Collection)
    └── UserSession (Entity)
        └── RefreshToken (string)
```

## ✅ Best Practices Implemented

1. **Immutability**: Value objects and entity IDs are immutable
2. **Factory methods**: `Create()` methods ensure valid construction
3. **Private setters**: Encapsulation prevents invalid state
4. **Rich domain model**: Business logic in entities, not anemic models
5. **Self-validating**: Entities validate their own invariants
6. **Domain events**: Communicate changes without tight coupling
7. **Explicit exceptions**: Clear, specific exceptions for each error case
8. **UTC timestamps**: All dates in UTC to avoid timezone issues
9. **Guard clauses**: Early validation and error handling
10. **Separation of concerns**: Each class has one clear responsibility

## 🧪 Testing Strategy

When testing the Domain Layer:

1. **Unit test entities**: Test all business logic methods
2. **Test value object validation**: Ensure all validation rules work
3. **Test domain events**: Verify events are raised correctly
4. **Test exceptions**: Verify proper exception throwing
5. **Test edge cases**: Boundary values, null handling, etc.
6. **No mocking needed**: Pure domain logic, no external dependencies

Example test:

```csharp
[Fact]
public void User_Create_ShouldRaiseUserRegisteredEvent()
{
    // Arrange
    var email = Email.Create("test@example.com");
    var password = Password.Create("P@ssw0rd123");

    // Act
    var user = User.Create(email, password, "Test User");

    // Assert
    user.DomainEvents.Should().ContainSingle();
    user.DomainEvents.First().Should().BeOfType<UserRegisteredEvent>();
}
```

## 🚀 Next Steps

The Domain Layer is now complete. Next steps:

1. **Application Layer**: Create CQRS commands, queries, handlers, and validators
2. **Repository Interfaces**: Define IUserRepository and IUserSessionRepository in Application layer
3. **Infrastructure Layer**: Implement EF Core configuration and repositories
4. **Integration Tests**: Test domain logic with real database

## 📚 References

- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [PBKDF2 (RFC 8018)](https://tools.ietf.org/html/rfc8018)
- [Email Validation (RFC 5322)](https://tools.ietf.org/html/rfc5322)
