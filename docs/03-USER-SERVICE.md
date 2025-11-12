# User Service Implementation

## Overview

The User Service is built using Domain-Driven Design (DDD) with Onion Architecture, implementing CQRS pattern with MediatR. This document details the implementation of all four layers.

## Project Structure

```
UserService/
├── UserService.Domain/              # Core business logic - NO dependencies
├── UserService.Application/         # Use cases (CQRS) - Depends on Domain
├── UserService.Infrastructure/      # External concerns - Depends on Application
├── UserService.Api/                 # Presentation - Depends on Application
└── UserService.Tests/               # Unit & Integration tests
```

## Domain Layer

### Entities

#### User Entity (`Entities/User.cs`)

```csharp
public class User : Entity
{
    public Guid Id { get; private set; }
    public Email Email { get; private set; }
    public PasswordHash PasswordHash { get; private set; }
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

## API Layer

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
