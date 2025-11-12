# Project Structure

This document describes the complete folder structure for the KafkaWithDotNet project, following Clean Architecture principles with explicit SOLID design patterns.

## Root Structure

```
KafkaWithDotNet/
├── src/                          # Source code
├── infrastructure/               # Infrastructure as Code and Docker
├── tests/                        # Cross-cutting integration and E2E tests
├── scripts/                      # Utility scripts
├── docs/                         # Documentation
├── .github/                      # GitHub Actions workflows
├── .gitignore                    # Git ignore rules
├── .editorconfig                 # Editor configuration
├── global.json                   # .NET SDK version
├── Directory.Build.props         # Shared MSBuild properties
├── KafkaWithDotNet.sln          # Visual Studio solution file
├── LICENSE                       # Project license
└── README.md                     # Project overview
```

## Source Structure (`src/`)

### Services (`src/services/`)

#### User Service (`src/services/UserService/`)

Following Clean Architecture with 4 layers:

**1. Domain Layer (`UserService.Domain/`)**

- **Zero dependencies** - Pure business logic
- `Entities/` - Domain entities (User, UserSession)
- `ValueObjects/` - Value objects (Email, Password)
- `Events/` - Domain events
- `Exceptions/` - Domain-specific exceptions
- `Common/` - Base classes and interfaces

**2. Application Layer (`UserService.Application/`)**

- **Depends only on Domain**
- `Commands/` - Write operations (SignupUser, SigninUser, etc.)
  - Each command folder contains: Command.cs, CommandHandler.cs, CommandValidator.cs
- `Queries/` - Read operations (GetUserProfile)
- `DTOs/` - Data Transfer Objects
- `Interfaces/` - Repository and service contracts
  - `Repositories/` - Repository interfaces
  - `Services/` - Application service interfaces
  - `Infrastructure/` - Infrastructure abstraction interfaces
- `Behaviors/` - MediatR pipeline behaviors (validation, logging, etc.)

**3. Infrastructure Layer (`UserService.Infrastructure/`)**

- **Implements Application interfaces**
- `Persistence/` - Database related
  - `Configurations/` - EF Core entity configurations
  - `Migrations/` - Database migrations
  - `ApplicationDbContext.cs` - EF Core DbContext
- `Repositories/` - Repository implementations
- `Services/` - Service implementations (PasswordHasher, TokenService, etc.)
- `Kafka/` - Kafka producers and consumers
- `Configuration/` - Infrastructure service registration

**4. Presentation Layer (`UserService.Presentation/`)**

- **Web API layer**
- `Controllers/` - API controllers
- `Middleware/` - Custom middleware
- `Filters/` - Action filters and exception filters
- `Extensions/` - Service registration extensions
- `Program.cs` - Application entry point
- `appsettings.json` - Application configuration

**5. Tests (`UserService.Tests/`)**

- `Unit/` - Unit tests
  - `Domain/` - Domain logic tests
  - `Application/` - Application logic tests
- `Integration/` - Integration tests
  - `Controllers/` - API integration tests
  - `Repositories/` - Repository integration tests
- `Architecture/` - Architecture tests (NetArchTest)

### Shared Libraries (`src/shared/`)

**Common (`src/shared/Common/`)**

- `Exceptions/` - Shared exception types
- `Extensions/` - Extension methods
- `Helpers/` - Utility helpers
- `Middleware/` - Reusable middleware

**Contracts (`src/shared/Contracts/`)**

- `Events/` - Event schemas for Kafka
- `DTOs/` - Shared DTOs

### Frontend (`src/frontend/`)

**Order App (`src/frontend/order-app/`)**

- `src/`
  - `components/` - React components
    - `layout/` - Layout components
    - `auth/` - Authentication components
  - `pages/` - Page components
  - `services/` - API service clients
  - `hooks/` - Custom React hooks
  - `utils/` - Utility functions
  - `types/` - TypeScript type definitions
  - `config/` - Configuration (MSAL, API endpoints)
- `public/` - Static assets

## Infrastructure Structure (`infrastructure/`)

### Terraform (`infrastructure/terraform/`)

**Modules (`infrastructure/terraform/modules/`)**

- `resource-group/` - Azure resource group module
- `container-apps/` - Azure Container Apps module
- `postgresql/` - Azure Database for PostgreSQL module
- `redis/` - Azure Cache for Redis module
- `apim/` - Azure API Management module
- `acr/` - Azure Container Registry module
- `key-vault/` - Azure Key Vault module
- `monitoring/` - Azure Monitor and Application Insights module

**Environments (`infrastructure/terraform/environments/`)**

- `dev/` - Development environment
- `staging/` - Staging environment
- `prod/` - Production environment

**Shared (`infrastructure/terraform/shared/`)**

- Shared variables and configurations

### Docker (`infrastructure/docker/`)

- `docker-compose.yml` - Multi-container application definition
- `Dockerfile.userservice` - User Service container
- `Dockerfile.frontend` - Frontend container
- `scripts/` - Helper scripts

## Test Structure (`tests/`)

- `Integration.Tests/` - Cross-service integration tests
- `E2E.Tests/` - End-to-end tests

## Scripts (`scripts/`)

- Development and deployment automation scripts

## CI/CD (`.github/workflows/`)

- GitHub Actions workflow definitions

## SOLID Principles Applied

This structure demonstrates all SOLID principles:

1. **SRP (Single Responsibility)**

   - Each folder has one clear responsibility
   - Commands, Handlers, and Validators are separate classes

2. **OCP (Open-Closed)**

   - Strategy pattern for services (e.g., IPasswordHasher)
   - Pipeline behaviors for cross-cutting concerns

3. **LSP (Liskov Substitution)**

   - Repository implementations are fully substitutable
   - In-memory implementations for testing

4. **ISP (Interface Segregation)**

   - Fine-grained repository interfaces
   - Role-based service interfaces

5. **DIP (Dependency Inversion)**
   - Application layer defines interfaces
   - Infrastructure layer implements them
   - Domain has zero dependencies

## Clean Architecture Dependency Flow

```
Presentation → Application → Domain
     ↓              ↓
Infrastructure → Application
```

- **Domain**: No dependencies
- **Application**: Depends only on Domain
- **Infrastructure**: Implements Application interfaces
- **Presentation**: Orchestrates Application and Infrastructure

---

For detailed implementation guidance, see the documentation in the `docs/` folder.
