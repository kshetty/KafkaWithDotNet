# UserService.Infrastructure

## Overview

The Infrastructure Layer implements all the abstractions defined in the Application Layer. It contains the concrete implementations for data persistence, external services, caching, and messaging.

## Architecture

This layer follows **Clean Architecture** principles:

- **Implements interfaces** from the Application layer
- **Depends on** Domain and Application layers
- Contains **implementation details** (EF Core, Redis, Kafka, JWT)
- **Should not** be referenced by the Domain layer

## Project Structure

```
UserService.Infrastructure/
├── Kafka/
│   └── KafkaEventPublisher.cs       # Kafka event publishing
├── Persistence/
│   ├── ApplicationDbContext.cs      # EF Core DbContext
│   └── Configurations/
│       ├── UserConfiguration.cs     # User entity configuration
│       └── UserSessionConfiguration.cs
├── Repositories/
│   ├── Repository.cs                # Base repository
│   ├── UserRepository.cs            # User repository
│   ├── UserSessionRepository.cs     # Session repository
│   └── UnitOfWork.cs                # Unit of Work
├── Services/
│   ├── CacheService.cs              # Redis caching
│   ├── DateTimeProvider.cs          # Time abstraction
│   └── TokenService.cs              # JWT generation/validation
└── DependencyInjection.cs           # Service registration
```

## Key Components

### 1. Database - Entity Framework Core

**ApplicationDbContext** manages database operations with PostgreSQL:

```csharp
public sealed class ApplicationDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
}
```

**Features:**

- PostgreSQL with Npgsql provider
- Automatic retry on failure (3 attempts)
- Value object conversions (Email, Password)
- Optimized indexes for performance
- Schema conventions (VARCHAR instead of TEXT)

**Entity Configurations:**

```csharp
// UserConfiguration.cs
- Maps Email value object to email column
- Maps Password value object to password_hash
- Unique index on email
- Indexes on lockout_end, is_active
- Cascade delete for sessions

// UserSessionConfiguration.cs
- Maps refresh_token with unique index
- Composite indexes for active session queries
- Foreign key to users table
```

### 2. Repositories

**Base Repository** provides common CRUD operations:

```csharp
public abstract class Repository<TEntity, TId>
{
    Task<TEntity?> GetByIdAsync(TId id);
    Task AddAsync(TEntity entity);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}
```

**UserRepository** implements user-specific queries:

```csharp
Task<User?> GetByEmailAsync(string email);
Task<bool> ExistsAsync(Guid id);
Task<User?> GetWithSessionsAsync(Guid id); // Eager loading
```

**UserSessionRepository** manages sessions:

```csharp
Task<UserSession?> GetByRefreshTokenAsync(string refreshToken);
Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId);
Task RevokeAllUserSessionsAsync(Guid userId);
Task<int> RemoveExpiredSessionsAsync(); // Cleanup task
```

**UnitOfWork** coordinates transactions:

```csharp
IUserRepository Users { get; }
IUserSessionRepository UserSessions { get; }

Task<int> SaveChangesAsync();
Task BeginTransactionAsync();
Task CommitTransactionAsync();
Task RollbackTransactionAsync();
```

### 3. Services

#### TokenService (JWT)

Generates and validates JWT access tokens:

```csharp
string GenerateAccessToken(Guid userId, string email);
ClaimsPrincipal? ValidateToken(string token);
Guid? GetUserIdFromToken(string token);
```

**Configuration:**

- Algorithm: HMAC-SHA256
- Claims: Sub (userId), Email, Jti (token ID), NameIdentifier
- Configurable expiry (default: 60 minutes)
- Clock skew: 5 minutes

**Required Settings:**

```json
{
  "Jwt": {
    "SecretKey": "your-256-bit-secret",
    "Issuer": "UserService",
    "Audience": "UserServiceClient",
    "ExpiryMinutes": 60
  }
}
```

#### CacheService (Redis)

Provides distributed caching with Redis:

```csharp
Task<T?> GetAsync<T>(string key);
Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
Task RemoveAsync(string key);
Task RemoveByPatternAsync(string pattern);
Task<bool> ExistsAsync(string key);
```

**Features:**

- JSON serialization
- Configurable expiry
- Pattern-based deletion
- Connection retry (3 attempts)

#### DateTimeProvider

Provides current UTC time for deterministic testing:

```csharp
DateTime UtcNow { get; }
```

#### KafkaEventPublisher

Publishes domain events to Kafka topics:

```csharp
Task PublishAsync<TEvent>(TEvent @event);
Task PublishManyAsync<TEvent>(IEnumerable<TEvent> events);
```

**Features:**

- Automatic topic naming (UserRegisteredEvent → user-registered)
- Event ID as partition key
- GZIP compression
- Acks.All for durability
- JSON serialization

**Kafka Topics:**

- `user-registered` (3 partitions, replication: 1)
- `user-signed-in` (3 partitions, replication: 1)
- `user-signed-out` (3 partitions, replication: 1)
- `email-verified` (3 partitions, replication: 1)

## Configuration

### Database Connection

```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=userservice;Username=admin;Password=your-password"
  }
}
```

### Redis Connection

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,password=your-redis-password,abortConnect=false"
  }
}
```

### Kafka Configuration

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

## Usage

### Registration

Add to `Program.cs`:

```csharp
builder.Services.AddInfrastructure(builder.Configuration);
```

This registers:

- ✅ DbContext with PostgreSQL
- ✅ All repositories (scoped)
- ✅ UnitOfWork (scoped)
- ✅ TokenService (singleton)
- ✅ CacheService (singleton)
- ✅ DateTimeProvider (singleton)
- ✅ EventPublisher (singleton)
- ✅ Redis connection (singleton)
- ✅ Kafka producer (singleton)

### Database Migrations

Create migration:

```bash
dotnet ef migrations add InitialCreate --project UserService.Infrastructure --startup-project UserService.Presentation
```

Apply migration:

```bash
dotnet ef database update --project UserService.Infrastructure --startup-project UserService.Presentation
```

## Dependencies

Required NuGet packages:

```xml
<!-- Database -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.1" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />

<!-- Caching -->
<PackageReference Include="StackExchange.Redis" Version="2.7.10" />

<!-- Messaging -->
<PackageReference Include="KafkaFlow" Version="3.0.9" />
<PackageReference Include="KafkaFlow.Serializer.JsonCore" Version="3.0.9" />
<PackageReference Include="KafkaFlow.Microsoft.DependencyInjection" Version="3.0.9" />

<!-- Security -->
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.1.2" />
```

## Database Schema

### users table

| Column                | Type         | Constraints             |
| --------------------- | ------------ | ----------------------- |
| id                    | uuid         | PRIMARY KEY             |
| email                 | varchar(255) | UNIQUE, NOT NULL        |
| password_hash         | varchar(255) | NOT NULL                |
| full_name             | varchar(200) | NOT NULL                |
| is_email_verified     | boolean      | NOT NULL, DEFAULT false |
| is_active             | boolean      | NOT NULL, DEFAULT true  |
| failed_login_attempts | int          | NOT NULL, DEFAULT 0     |
| lockout_end           | timestamp    | NULL                    |
| last_login_at         | timestamp    | NULL                    |
| created_at            | timestamp    | NOT NULL                |
| updated_at            | timestamp    | NOT NULL                |

**Indexes:**

- `ix_users_email` (UNIQUE)
- `ix_users_lockout_end` (filtered)
- `ix_users_is_active`

### user_sessions table

| Column        | Type         | Constraints            |
| ------------- | ------------ | ---------------------- |
| id            | uuid         | PRIMARY KEY            |
| user_id       | uuid         | FOREIGN KEY, NOT NULL  |
| refresh_token | varchar(255) | UNIQUE, NOT NULL       |
| ip_address    | varchar(45)  | NULL                   |
| user_agent    | varchar(500) | NULL                   |
| is_active     | boolean      | NOT NULL, DEFAULT true |
| revoked_at    | timestamp    | NULL                   |
| created_at    | timestamp    | NOT NULL               |
| expires_at    | timestamp    | NOT NULL               |
| last_used_at  | timestamp    | NULL                   |

**Indexes:**

- `ix_user_sessions_refresh_token` (UNIQUE)
- `ix_user_sessions_user_id`
- `ix_user_sessions_expires_at`
- `ix_user_sessions_user_id_is_active` (composite)
- `ix_user_sessions_active` (composite: user_id, is_active, expires_at)

## Performance Considerations

### Database

- Use `.AsNoTracking()` for read-only queries
- Eager load relationships with `.Include()` when needed
- Indexes optimized for common query patterns
- Connection pooling enabled by default

### Caching

- Redis used for user profiles (5-minute TTL)
- Reduces database load for frequently accessed data
- Cache-aside pattern implemented

### Kafka

- GZIP compression reduces network bandwidth
- Batching with `PublishManyAsync`
- Event ID as partition key ensures ordering per entity

## Testing

Mock repositories and services in tests:

```csharp
var mockUnitOfWork = Substitute.For<IUnitOfWork>();
var mockTokenService = Substitute.For<ITokenService>();
var mockCacheService = Substitute.For<ICacheService>();

mockTokenService.GenerateAccessToken(userId, email)
    .Returns("mock-jwt-token");
```

Use in-memory database for integration tests:

```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));
```

## Security Best Practices

1. **Connection Strings**: Store in Azure Key Vault or user secrets
2. **JWT Secret**: Use strong 256-bit key, rotate regularly
3. **Redis Password**: Always use authentication in production
4. **SSL/TLS**: Enable for PostgreSQL and Redis connections
5. **Password Hashing**: BCrypt with automatic salt (handled in Domain)

## Monitoring

Enable logging in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore": "Warning",
      "KafkaFlow": "Information"
    },
    "EnableSensitiveDataLogging": false
  }
}
```

## Next Steps

To complete the application:

1. Implement Presentation layer (controllers, middleware)
2. Create database migrations
3. Configure authentication in `Program.cs`
4. Add health checks for PostgreSQL, Redis, Kafka
5. Set up distributed tracing
