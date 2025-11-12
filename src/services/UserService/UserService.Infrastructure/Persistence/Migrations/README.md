# Database Migrations

This directory contains Entity Framework Core migrations for the UserService database schema.

## Overview

The UserService uses PostgreSQL as its primary database and Entity Framework Core for data access. Migrations are generated using the EF Core CLI tools and stored in this directory.

## Current Migrations

### 20251112143402_InitialCreate

The initial database schema migration that creates the foundation for user authentication and session management.

#### Tables Created

**users**

- Primary table for storing user account information
- Schema: `public.users`
- Primary Key: `id` (uuid)

Columns:

- `id` - UUID, primary key
- `email` - VARCHAR(255), unique, indexed - User's email address
- `password_hash` - VARCHAR(255) - Hashed password (never store plain text)
- `full_name` - VARCHAR(200) - User's full name
- `is_email_verified` - BOOLEAN, default false - Email verification status
- `is_active` - BOOLEAN, default true, indexed - Account active status
- `failed_login_attempts` - INTEGER, default 0 - Failed login counter for lockout
- `locked_out_until` - TIMESTAMP WITH TIME ZONE, nullable, indexed - Account lockout expiry
- `last_login_at` - TIMESTAMP WITH TIME ZONE, nullable - Last successful login
- `created_at` - TIMESTAMP WITH TIME ZONE - Account creation timestamp
- `updated_at` - TIMESTAMP WITH TIME ZONE - Last update timestamp

Indexes:

- `ix_users_email` - Unique index on email for fast lookup and uniqueness constraint
- `ix_users_is_active` - Index on is_active for filtering active users
- `ix_users_locked_out_until` - Partial index (WHERE locked_out_until IS NOT NULL) for lockout queries

**user_sessions**

- Table for managing refresh tokens and active user sessions
- Schema: `public.user_sessions`
- Primary Key: `id` (uuid)
- Foreign Key: `user_id` references `users(id)` ON DELETE CASCADE

Columns:

- `id` - UUID, primary key
- `user_id` - UUID, foreign key to users.id, indexed
- `refresh_token` - VARCHAR(255), unique, indexed - Refresh token for JWT rotation
- `expires_at` - TIMESTAMP WITH TIME ZONE, indexed - Token expiration time
- `is_active` - BOOLEAN, default true - Session active status
- `ip_address` - VARCHAR(45), nullable - IPv4/IPv6 address (45 chars for IPv6)
- `user_agent` - VARCHAR(500), nullable - Browser/client user agent
- `revoked_at` - TIMESTAMP WITH TIME ZONE, nullable - When session was revoked
- `last_used_at` - TIMESTAMP WITH TIME ZONE, nullable - Last token usage
- `created_at` - TIMESTAMP WITH TIME ZONE - Session creation timestamp
- `updated_at` - TIMESTAMP WITH TIME ZONE, nullable - Last update timestamp

Indexes:

- `ix_user_sessions_user_id` - Index on user_id for user session queries
- `ix_user_sessions_refresh_token` - Unique index for fast token lookup
- `ix_user_sessions_expires_at` - Index for expiration cleanup queries
- `ix_user_sessions_user_id_is_active` - Composite index for active session queries
- `ix_user_sessions_active` - Composite index (user_id, is_active, expires_at) for optimal active session filtering

#### Relationships

- One-to-Many: `users` → `user_sessions` (one user can have multiple sessions)
- Cascade Delete: When a user is deleted, all their sessions are automatically deleted

## How to Apply Migrations

### Prerequisites

1. PostgreSQL must be running and accessible
2. Connection string configured in `appsettings.json` or environment variables
3. EF Core CLI tools installed: `dotnet tool install --global dotnet-ef`

### Apply Migrations to Database

From the Infrastructure project directory:

```bash
# Apply all pending migrations
dotnet ef database update

# Apply migrations from solution root
dotnet ef database update --project src/services/UserService/UserService.Infrastructure

# Apply migrations using Docker Compose PostgreSQL
# Ensure connection string matches docker-compose setup:
# Host=localhost;Port=5432;Database=userservice;Username=postgres;Password=postgres
```

### Using Docker Compose

```bash
# Start PostgreSQL container
cd infrastructure/docker
docker-compose up -d postgres

# Wait for PostgreSQL to be ready
docker-compose ps postgres

# Apply migrations
cd ../../src/services/UserService/UserService.Infrastructure
dotnet ef database update

# Verify tables created
docker exec -it postgres psql -U postgres -d userservice -c "\dt public.*"
```

## How to Create New Migrations

### Generate a New Migration

```bash
# From Infrastructure project directory
dotnet ef migrations add <MigrationName> --output-dir Persistence/Migrations

# Example:
dotnet ef migrations add AddUserProfilePicture --output-dir Persistence/Migrations
```

### Migration Naming Conventions

- Use PascalCase
- Use descriptive names: `AddColumnName`, `CreateTableName`, `UpdateIndexName`
- Prefix with action verb: Add, Create, Update, Remove, Alter
- Examples:
  - `AddUserPhoneNumber`
  - `CreateAuditLogTable`
  - `UpdateUserEmailIndex`
  - `RemoveDeprecatedColumns`

## How to Rollback Migrations

### Remove Last Migration (Before Applying)

```bash
# Remove the last migration (if not yet applied to database)
dotnet ef migrations remove

# Force remove (discards changes)
dotnet ef migrations remove --force
```

### Rollback Applied Migration

```bash
# Rollback to specific migration
dotnet ef database update <PreviousMigrationName>

# Rollback to initial state (remove all migrations)
dotnet ef database update 0

# Example: Rollback to before AddUserPhoneNumber
dotnet ef database update InitialCreate
```

## Design-Time DbContext Factory

The `ApplicationDbContextFactory` class enables EF Core tools to create a `DbContext` instance at design time without running the full application. This is required for:

- Generating migrations
- Scaffolding from existing databases
- Running design-time services

**Location**: `Persistence/ApplicationDbContextFactory.cs`

**Default Connection**: `Host=localhost;Port=5432;Database=userservice;Username=postgres;Password=postgres`

⚠️ **Note**: The factory uses a hardcoded connection string for localhost development. In production, migrations are applied using the actual application configuration.

## Troubleshooting

### Error: "Your startup project doesn't reference Microsoft.EntityFrameworkCore.Design"

**Solution**: Ensure the Presentation project has the Design package:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.1">
  <PrivateAssets>all</PrivateAssets>
</PackageReference>
```

### Error: "Unable to create an object of type 'ApplicationDbContext'"

**Solution**: This error occurs when the design-time factory can't find the DbContext. Make sure:

1. `ApplicationDbContextFactory` exists in the Infrastructure project
2. You're running migrations from the Infrastructure project directory
3. The factory's connection string is correct

### Error: "Password authentication failed for user 'postgres'"

**Solution**: Verify:

1. PostgreSQL is running: `docker-compose ps postgres`
2. Connection string credentials match docker-compose.yml
3. Database exists: `docker exec -it postgres psql -U postgres -l`

### Viewing Applied Migrations

```bash
# List all migrations (applied and pending)
dotnet ef migrations list

# Query database directly for applied migrations
docker exec -it postgres psql -U postgres -d userservice -c "SELECT * FROM \"__EFMigrationsHistory\";"
```

### Verify Database Schema

```bash
# Connect to PostgreSQL
docker exec -it postgres psql -U postgres -d userservice

# List all tables
\dt public.*

# Describe users table
\d public.users

# Describe user_sessions table
\d public.user_sessions

# List all indexes
\di public.*

# Check foreign keys
\d+ public.user_sessions

# Exit psql
\q
```

## Best Practices

1. **Always Review Generated Migrations**: Check the `Up()` and `Down()` methods before applying
2. **Test Rollbacks**: Ensure `Down()` properly reverses `Up()` changes
3. **Backup Before Production Migrations**: Always backup production databases
4. **Use Transactions**: EF Core wraps migrations in transactions by default
5. **Avoid Manual Schema Changes**: Always use migrations, never modify schema directly
6. **Version Control**: Commit migrations immediately after generation
7. **One Change Per Migration**: Keep migrations focused and atomic
8. **Index Strategy**: Add indexes for foreign keys and frequently queried columns
9. **Nullable vs Required**: Explicitly specify with `.IsRequired()` or `.IsRequired(false)`
10. **Data Migration Separation**: Separate schema changes from data migrations

## Connection String Configuration

### Development (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=userservice;Username=postgres;Password=postgres"
  }
}
```

### Production (Environment Variables)

```bash
export ConnectionStrings__DefaultConnection="Host=production-host;Port=5432;Database=userservice;Username=produser;Password=<secure-password>;SSL Mode=Require"
```

### Docker Compose

```yaml
environment:
  ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=userservice;Username=postgres;Password=postgres"
```

## Migration History

| Migration                    | Date       | Description                                        |
| ---------------------------- | ---------- | -------------------------------------------------- |
| 20251112143402_InitialCreate | 2025-01-12 | Initial schema with users and user_sessions tables |

## Schema Diagram

```
┌─────────────────────────────┐
│         users               │
├─────────────────────────────┤
│ id (PK)                     │
│ email (UNIQUE)              │
│ password_hash               │
│ full_name                   │
│ is_email_verified           │
│ is_active                   │
│ failed_login_attempts       │
│ locked_out_until            │
│ last_login_at               │
│ created_at                  │
│ updated_at                  │
└─────────────────────────────┘
         │
         │ 1:N
         │
┌────────▼────────────────────┐
│     user_sessions           │
├─────────────────────────────┤
│ id (PK)                     │
│ user_id (FK)                │
│ refresh_token (UNIQUE)      │
│ expires_at                  │
│ is_active                   │
│ ip_address                  │
│ user_agent                  │
│ revoked_at                  │
│ last_used_at                │
│ created_at                  │
│ updated_at                  │
└─────────────────────────────┘
```

## Security Considerations

1. **Password Storage**: Only `password_hash` is stored, never plain text passwords
2. **Token Security**: Refresh tokens are 32-byte cryptographically secure random values
3. **Session Tracking**: IP address and user agent logged for security audit
4. **Account Lockout**: Failed login attempts trigger temporary account lockout
5. **Token Rotation**: Refresh tokens have expiration and can be revoked
6. **Cascade Deletion**: User deletion automatically removes all sessions
7. **Email Verification**: `is_email_verified` tracks email confirmation status

## Performance Optimization

- **Unique Indexes**: Email and refresh_token have unique indexes for O(1) lookups
- **Composite Indexes**: Multi-column indexes optimize common query patterns
- **Partial Indexes**: `locked_out_until` uses partial index (only rows with values)
- **Foreign Key Indexes**: All FKs automatically indexed for JOIN performance
- **Timestamp Indexes**: `expires_at` indexed for expiration cleanup jobs

## Maintenance Tasks

### Clean Up Expired Sessions

```sql
-- Delete expired sessions (run periodically)
DELETE FROM public.user_sessions
WHERE expires_at < NOW()
   OR (is_active = false AND revoked_at < NOW() - INTERVAL '30 days');
```

### Monitor Session Growth

```sql
-- Check total sessions per user
SELECT u.email, COUNT(s.id) as session_count
FROM public.users u
LEFT JOIN public.user_sessions s ON u.id = s.user_id
GROUP BY u.id, u.email
ORDER BY session_count DESC;
```

### Identify Locked Accounts

```sql
-- Find currently locked accounts
SELECT id, email, locked_out_until, failed_login_attempts
FROM public.users
WHERE locked_out_until > NOW()
ORDER BY locked_out_until DESC;
```

## Additional Resources

- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Npgsql EF Core Provider](https://www.npgsql.org/efcore/)
- [PostgreSQL Data Types](https://www.postgresql.org/docs/current/datatype.html)
- [PostgreSQL Indexes](https://www.postgresql.org/docs/current/indexes.html)
