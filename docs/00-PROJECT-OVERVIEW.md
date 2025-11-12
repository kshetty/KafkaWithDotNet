# Kafka with .NET - Order Processing System

## Project Overview

This project demonstrates a real-time, event-driven microservices architecture using Apache Kafka with .NET 8, implementing an Order Processing System. The architecture follows Domain-Driven Design (DDD) principles with Clean Architecture and SOLID design principles, deployed on Azure using Infrastructure as Code (Terraform).

## 🎯 Business Scenario

**Real-time Order Processing Platform** that enables:

- User registration and authentication via Microsoft Entra External ID
- Order placement and tracking
- Real-time inventory management
- Automated notifications
- Complete audit trail via event sourcing

## 🏗️ Architecture Principles

### **1. Domain-Driven Design (DDD)**

- Clear bounded contexts for each microservice
- Rich domain models with business logic
- Domain events for inter-service communication
- Ubiquitous language across the codebase

### **2. Clean Architecture**

Clean Architecture enforces separation of concerns with dependency rules flowing inward:

- **Domain (Enterprise Business Rules)**: Core business logic, entities, value objects, domain events (ZERO dependencies)
- **Application (Application Business Rules)**: Use cases, CQRS commands/queries, interfaces, DTOs (depends only on Domain)
- **Infrastructure (Interface Adapters)**: Data access, external services, caching, messaging (depends on Application)
- **Presentation (Frameworks & Drivers)**: API controllers, middleware, web concerns (depends on Application)

**Key Principle**: Dependencies point inward. Inner layers know nothing about outer layers.

### **3. SOLID Principles**

Every layer explicitly follows SOLID design:

- **Single Responsibility Principle (SRP)**: Each class has one reason to change

  - Commands, Queries, Handlers are separate
  - Repositories handle only data access
  - Services have focused responsibilities

- **Open/Closed Principle (OCP)**: Open for extension, closed for modification

  - Use interfaces and abstractions
  - Strategy pattern for algorithms (e.g., PasswordHasher)
  - Decorator pattern for cross-cutting concerns

- **Liskov Substitution Principle (LSP)**: Subtypes must be substitutable

  - Interface implementations are interchangeable
  - Mock implementations for testing
  - Polymorphic behavior without breaking contracts

- **Interface Segregation Principle (ISP)**: Clients shouldn't depend on unused methods

  - Focused, role-based interfaces
  - `IUserRepository` separate from `IUserReadRepository`
  - Granular service interfaces

- **Dependency Inversion Principle (DIP)**: Depend on abstractions, not concretions
  - All dependencies injected via interfaces
  - Infrastructure implements interfaces defined in Application/Domain
  - IoC container manages lifetimes

### **4. Event-Driven Architecture**

- Apache Kafka for event streaming (KRaft mode - over ZooKeeper)
- Domain events published to Kafka topics
- Eventual consistency across services
- Event sourcing for complete audit trails

### **5. CQRS Pattern**

- Separate commands (write) and queries (read)
- Optimized read and write models
- MediatR for command/query handling
- Clear separation of concerns

### **6. Microservices Architecture**

- Independent deployable services
- Each service owns its data
- Communication via events (Kafka)
- Horizontal scalability

## 📐 System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         User Interface                           │
│                   React 18 + TypeScript + MSAL                   │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         │ HTTPS + JWT
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│              Microsoft Entra External ID (OAuth 2.0)             │
│                   Authentication & Authorization                 │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         │ JWT Token Validation
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                  Azure API Management (APIM)                     │
│          API Gateway | Rate Limiting | JWT Validation           │
└────────────────────────┬────────────────────────────────────────┘
                         │
           ┌─────────────┼─────────────┬──────────────┐
           │             │             │              │
           ▼             ▼             ▼              ▼
    ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐
    │   User   │  │  Order   │  │Inventory │  │ Notify   │
    │ Service  │  │ Service  │  │ Service  │  │ Service  │
    │Container │  │Container │  │Container │  │Container │
    │   App    │  │   App    │  │   App    │  │   App    │
    └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘
         │             │             │              │
         │             └─────────────┼──────────────┘
         │                           │
         │                           ▼
         │              ┌────────────────────────┐
         │              │    Apache Kafka        │
         │              │    (KRaft Mode)        │
         │              │  Event Streaming Bus   │
         │              └────────────────────────┘
         │
         ├───────────────────────┬────────────────────┐
         ▼                       ▼                    ▼
┌─────────────────┐    ┌─────────────────┐  ┌────────────────┐
│   PostgreSQL    │    │  Redis Cache    │  │ Application    │
│   Database      │    │  Sessions &     │  │   Insights     │
│   (Flexible)    │    │  Caching        │  │  Monitoring    │
└─────────────────┘    └─────────────────┘  └────────────────┘
```

## 🔄 Phase 1: User Management Foundation

**Objective**: Build the authentication and user management infrastructure

### Components:

1. **User Service** - User registration, authentication, profile management
2. **React Frontend** - User interface with MSAL authentication
3. **Azure APIM** - API gateway with JWT validation
4. **Infrastructure** - Terraform IaC for all Azure resources
5. **Local Development** - Docker Compose with Kafka (KRaft), PostgreSQL, Redis

### User Journey Flow:

```
┌──────────┐
│  Signup  │
└────┬─────┘
     │
     ▼
┌─────────────────────────────────────────────────────────┐
│ 1. User fills signup form in React                      │
│ 2. MSAL redirects to Entra External ID                  │
│ 3. User completes registration + email verification     │
│ 4. React receives OAuth token                           │
│ 5. React calls POST /api/users/signup via APIM         │
│ 6. User Service validates token                         │
│ 7. Creates user in PostgreSQL                           │
│ 8. Publishes UserRegisteredEvent to Kafka              │
│ 9. Generates access + refresh tokens                    │
│ 10. Stores refresh token + session in Redis            │
│ 11. Returns user profile + tokens                       │
└─────────────────────────────────────────────────────────┘
     │
     ▼
┌──────────┐
│Dashboard │
└──────────┘

┌──────────┐
│  Signin  │
└────┬─────┘
     │
     ▼
┌─────────────────────────────────────────────────────────┐
│ 1. User enters credentials                              │
│ 2. MSAL authenticates with Entra External ID           │
│ 3. React receives OAuth token                           │
│ 4. React calls POST /api/users/signin via APIM         │
│ 5. User Service validates token                         │
│ 6. Verifies user exists in PostgreSQL                   │
│ 7. Publishes UserSignedInEvent to Kafka                │
│ 8. Generates access + refresh tokens                    │
│ 9. Stores session metadata in Redis                     │
│ 10. Updates last_login_at in PostgreSQL                │
│ 11. Returns user profile + tokens                       │
└─────────────────────────────────────────────────────────┘
     │
     ▼
┌──────────┐
│Dashboard │
└──────────┘
```

## 🔐 Authentication & Authorization Strategy

### Token Strategy:

- **Access Token (JWT)**: Short-lived (15 minutes), stateless validation
- **Refresh Token**: Long-lived (7 days), stored in Redis, rotated on refresh
- **Session Metadata**: Stored in Redis with device info, IP, last activity

### Security Layers:

1. **Entra External ID**: OAuth 2.0 authentication
2. **APIM JWT Validation**: Validates token signature and claims
3. **Service Authentication**: Additional validation in User Service
4. **Redis Session Check**: For sensitive operations and logout

### Why Hybrid Approach (Token + Session)?

- ✅ Instant token revocation on logout/password change
- ✅ Track active sessions per user
- ✅ Detect suspicious activity (multiple devices, locations)
- ✅ Force logout capability
- ✅ Audit trail of user sessions

## 🌍 Multi-Environment Strategy

### Environment Configuration:

| Resource           | Dev                       | Staging                      | Production                    |
| ------------------ | ------------------------- | ---------------------------- | ----------------------------- |
| **PostgreSQL**     | B_Standard_B1ms (~$12/mo) | GP_Standard_D2s_v3 (~$80/mo) | GP_Standard_D4s_v3 (~$160/mo) |
| **Redis**          | Basic C0 (~$16/mo)        | Standard C1 (~$75/mo)        | Premium P1 (~$250/mo)         |
| **APIM**           | Consumption (~$35/mo)     | Consumption (~$35/mo)        | StandardV2 (~$700/mo)         |
| **Container Apps** | Consumption (scale to 0)  | Consumption (min 1)          | Consumption (min 2)           |
| **Replicas**       | 0-2                       | 1-5                          | 2-10                          |
| **Total Est.**     | ~$50-80/month             | ~$200-300/month              | ~$1200-1500/month             |

### Infrastructure Separation:

- Separate Azure subscriptions (recommended) or resource groups
- Separate Terraform state files per environment
- Separate Key Vault instances
- Separate Entra External ID tenants (or separate app registrations)

## 📊 Data Architecture

### PostgreSQL Schema:

```
Users Table:
├── id (UUID, PK)
├── email (VARCHAR, UNIQUE)
├── password_hash (VARCHAR)
├── first_name (VARCHAR)
├── last_name (VARCHAR)
├── entra_external_id (VARCHAR, UNIQUE)
├── created_at (TIMESTAMP)
├── updated_at (TIMESTAMP)
├── last_login_at (TIMESTAMP)
├── is_active (BOOLEAN)
└── is_email_verified (BOOLEAN)

UserSessions Table:
├── id (UUID, PK)
├── user_id (UUID, FK -> Users)
├── refresh_token_hash (VARCHAR, UNIQUE)
├── device_info (JSONB)
├── ip_address (VARCHAR)
├── created_at (TIMESTAMP)
├── expires_at (TIMESTAMP)
├── last_activity_at (TIMESTAMP)
└── is_revoked (BOOLEAN)
```

### Redis Cache Structure:

```
Key Pattern: session:{refresh_token_hash}
Value: JSON {
  userId: UUID,
  email: string,
  deviceInfo: object,
  ipAddress: string,
  createdAt: timestamp,
  lastActivity: timestamp
}
TTL: 7 days (auto-cleanup)

Key Pattern: user:profile:{userId}
Value: JSON { user profile data }
TTL: 15 minutes (cache frequently accessed data)
```

### Kafka Topics (Phase 1):

```
user-events:
  - UserRegisteredEvent
  - UserSignedInEvent
  - UserSignedOutEvent
  - UserProfileUpdatedEvent
  - UserDeactivatedEvent

Partitioning: By userId (ensures ordering per user)
Retention: 30 days
Replication Factor: 3 (production)
```

## 🛠️ Technology Stack

### Backend:

- **.NET 8.0** - Latest LTS version
- **ASP.NET Core** - Web API framework
- **Entity Framework Core 8.0** - ORM for PostgreSQL
- **MediatR 12.x** - CQRS command/query handling
- **FluentValidation 11.x** - Input validation
- **KafkaFlow 3.x** - Kafka integration
- **StackExchange.Redis** - Redis client
- **BCrypt.Net** - Password hashing
- **Microsoft.Identity.Web** - Entra External ID integration
- **Serilog** - Structured logging
- **xUnit** - Unit testing
- **Moq** - Mocking framework
- **Testcontainers** - Integration testing

### Frontend:

- **React 18** - UI library
- **TypeScript 5.x** - Type safety
- **@azure/msal-browser** - Entra External ID auth
- **@azure/msal-react** - React integration
- **React Router v6** - Client-side routing
- **Axios** - HTTP client
- **Material-UI v5** OR **Tailwind CSS** - UI framework
- **Vite** - Build tool
- **Vitest** - Unit testing
- **React Testing Library** - Component testing

### Infrastructure:

- **Terraform 1.6+** - Infrastructure as Code
- **Docker & Docker Compose** - Containerization
- **Azure Container Registry** - Container images
- **Azure Container Apps** - Serverless containers
- **Azure Database for PostgreSQL** - Managed database
- **Azure Cache for Redis** - Managed cache
- **Azure API Management** - API gateway
- **Azure Key Vault** - Secrets management
- **Azure Application Insights** - Monitoring
- **Apache Kafka 3.9.1** (KRaft mode) - Event streaming

### DevOps:

- **GitHub Actions** - CI/CD pipelines
- **GitHub Container Registry** OR **Azure Container Registry**
- **Terraform Cloud** OR **Azure Storage** (state backend)

## 📁 Project Structure

```
KafkaWithDotNet/
├── .github/
│   └── workflows/                      # CI/CD pipelines
│       ├── terraform-dev.yml
│       ├── terraform-staging.yml
│       ├── terraform-prod.yml
│       ├── user-service-build.yml
│       └── frontend-build.yml
│
├── docs/                               # Documentation
│   ├── 00-PROJECT-OVERVIEW.md          # This file
│   ├── 01-ARCHITECTURE.md              # Detailed architecture
│   ├── 02-INFRASTRUCTURE.md            # Terraform guide
│   ├── 03-USER-SERVICE.md              # User service details
│   ├── 04-FRONTEND.md                  # Frontend guide
│   ├── 05-LOCAL-DEVELOPMENT.md         # Local setup
│   ├── 06-DEPLOYMENT.md                # Deployment guide
│   ├── 07-API-REFERENCE.md             # API documentation
│   └── 08-TROUBLESHOOTING.md           # Common issues
│
├── infrastructure/
│   ├── docker/
│   │   ├── docker-compose.yml          # Local development
│   │   ├── docker-compose.override.yml
│   │   └── .env.example
│   │
│   └── terraform/
│       ├── modules/                    # Reusable modules
│       │   ├── resource-group/
│       │   ├── container-apps/
│       │   ├── postgresql/
│       │   ├── redis/
│       │   ├── apim/
│       │   ├── acr/
│       │   ├── key-vault/
│       │   ├── entra-external-id/
│       │   └── monitoring/
│       │
│       ├── environments/               # Environment configs
│       │   ├── dev/
│       │   │   ├── main.tf
│       │   │   ├── variables.tf
│       │   │   ├── terraform.tfvars
│       │   │   └── backend.tf
│       │   ├── staging/
│       │   │   └── ...
│       │   └── prod/
│       │       └── ...
│       │
│       └── shared/
│           └── common-variables.tf
│
├── src/
│   ├── frontend/
│   │   └── order-app/                  # React application
│   │       ├── public/
│   │       ├── src/
│   │       │   ├── components/
│   │       │   ├── contexts/
│   │       │   ├── hooks/
│   │       │   ├── pages/
│   │       │   ├── services/
│   │       │   ├── utils/
│   │       │   └── config/
│   │       ├── package.json
│   │       ├── tsconfig.json
│   │       ├── vite.config.ts
│   │       └── Dockerfile
│   │
│   ├── services/
│   │   ├── UserService/                # Phase 1 - Clean Architecture
│   │   │   ├── UserService.Domain/           # Enterprise Business Rules
│   │   │   │   ├── Entities/
│   │   │   │   ├── ValueObjects/
│   │   │   │   ├── Events/
│   │   │   │   ├── Exceptions/
│   │   │   │   └── Common/
│   │   │   │
│   │   │   ├── UserService.Application/      # Application Business Rules
│   │   │   │   ├── Commands/
│   │   │   │   ├── Queries/
│   │   │   │   ├── DTOs/
│   │   │   │   ├── Interfaces/
│   │   │   │   ├── Validators/
│   │   │   │   └── Behaviors/
│   │   │   │
│   │   │   ├── UserService.Infrastructure/   # Interface Adapters
│   │   │   │   ├── Persistence/
│   │   │   │   ├── Repositories/
│   │   │   │   ├── Services/
│   │   │   │   ├── Kafka/
│   │   │   │   └── Configuration/
│   │   │   │
│   │   │   ├── UserService.Presentation/     # Frameworks & Drivers
│   │   │   │   ├── Controllers/
│   │   │   │   ├── Middleware/
│   │   │   │   ├── Filters/
│   │   │   │   ├── Extensions/
│   │   │   │   └── Program.cs
│   │   │   │
│   │   │   └── UserService.Tests/
│   │   │       ├── Unit/
│   │   │       ├── Integration/
│   │   │       └── Architecture/
│   │   │
│   │   ├── OrderService/               # Phase 2 (future)
│   │   ├── OrderProcessor/             # Phase 2 (future)
│   │   ├── InventoryService/           # Phase 2 (future)
│   │   └── NotificationService/        # Phase 2 (future)
│   │
│   └── shared/
│       ├── Common/                     # Shared libraries
│       │   ├── Exceptions/
│       │   ├── Extensions/
│       │   ├── Helpers/
│       │   └── Middleware/
│       │
│       └── Contracts/                  # Event contracts
│           ├── Events/
│           └── DTOs/
│
├── tests/
│   ├── Integration.Tests/              # Integration tests
│   └── E2E.Tests/                      # End-to-end tests
│
├── scripts/                            # Utility scripts
│   ├── setup-local-env.sh
│   ├── seed-database.sh
│   └── generate-certs.sh
│
├── .gitignore
├── .editorconfig
├── LICENSE
├── README.md                           # Quick start guide
└── global.json                         # .NET SDK version
```

## 🚀 Development Workflow

### Local Development:

1. Clone repository
2. Run `docker-compose up -d` (Kafka, PostgreSQL, Redis)
3. Run User Service: `dotnet run --project src/services/UserService/UserService.Presentation`
4. Run Frontend: `npm run dev` in `src/frontend/order-app`
5. Access: http://localhost:3000

### Deployment:

1. Push code to GitHub
2. GitHub Actions triggers:
   - Terraform plan/apply (infrastructure)
   - Build .NET service → push to ACR
   - Deploy to Azure Container Apps
   - Build React app → deploy to Azure Static Web Apps
3. Smoke tests run automatically

## 📈 Future Phases

### Phase 2: Order Processing

- Order Service (API)
- Order Processor (consumer)
- Kafka integration for order events
- Order status tracking

### Phase 3: Inventory Management

- Inventory Service
- Stock checking and reservation
- Low stock alerts

### Phase 4: Notifications

- Notification Service
- Email/SMS integration
- Real-time notifications via SignalR

### Phase 5: Advanced Features

- Event sourcing implementation
- CQRS read models
- Analytics and reporting
- Admin dashboard

## 📝 Documentation Index

- **[01-ARCHITECTURE.md](01-ARCHITECTURE.md)**: Detailed system architecture, C4 diagrams, component interactions
- **[02-INFRASTRUCTURE.md](02-INFRASTRUCTURE.md)**: Terraform modules, multi-environment setup, Azure resources
- **[03-USER-SERVICE.md](03-USER-SERVICE.md)**: User Service implementation, DDD layers, API endpoints
- **[04-FRONTEND.md](04-FRONTEND.md)**: React application, MSAL setup, component structure
- **[05-LOCAL-DEVELOPMENT.md](05-LOCAL-DEVELOPMENT.md)**: Docker Compose setup, local configuration
- **[06-DEPLOYMENT.md](06-DEPLOYMENT.md)**: CI/CD pipelines, Azure deployment steps
- **[07-API-REFERENCE.md](07-API-REFERENCE.md)**: Complete API documentation, request/response examples
- **[08-TROUBLESHOOTING.md](08-TROUBLESHOOTING.md)**: Common issues, debugging guide

## ✅ Success Criteria (Phase 1)

- [ ] User can register via Entra External ID
- [ ] User can sign in and receive JWT tokens
- [ ] User can view and update profile
- [ ] User can sign out (token revocation works)
- [ ] All API calls are authenticated via APIM
- [ ] Domain events are published to Kafka
- [ ] Session management works with Redis
- [ ] Infrastructure can be deployed via Terraform to dev/staging/prod
- [ ] CI/CD pipeline successfully deploys all components
- [ ] Health checks and monitoring are operational
- [ ] All unit and integration tests pass

---

**Next Steps**: Review remaining documentation files to understand implementation details for each component.
