# System Architecture

## Architecture Overview

This document provides detailed architecture diagrams and explanations following the C4 model (Context, Container, Component, Code).

## C4 Model - Level 1: System Context

```
                                   ┌─────────────────┐
                                   │  External User  │
                                   │   (Customer)    │
                                   └────────┬────────┘
                                            │
                                            │ Web Browser
                                            │ (HTTPS)
                                            ▼
                    ┌───────────────────────────────────────┐
                    │                                       │
                    │   Order Processing System             │
                    │   (Kafka + .NET Microservices)        │
                    │                                       │
                    │   - User Management                   │
                    │   - Order Processing                  │
                    │   - Inventory Management              │
                    │   - Notifications                     │
                    │                                       │
                    └────────┬──────────────────────────┬───┘
                             │                          │
                             │                          │
                    ┌────────▼────────┐        ┌────────▼────────┐
                    │  Entra External │        │  Email/SMS      │
                    │  ID (Microsoft) │        │  Provider       │
                    │                 │        │  (Future)       │
                    │  Authentication │        │                 │
                    └─────────────────┘        └─────────────────┘
```

## C4 Model - Level 2: Container Diagram (Phase 1)

```
┌───────────────────────────────────────────────────────────────────────┐
│                           External User                               │
└────────────────────────────────┬──────────────────────────────────────┘
                                 │
                                 │ HTTPS
                                 │
                    ┌────────────▼────────────┐
                    │   React Web App         │
                    │   (SPA)                 │
                    │                         │
                    │   - TypeScript          │
                    │   - MSAL.js             │
                    │   - Material-UI         │
                    │                         │
                    │   Port: 3000/443        │
                    └────────────┬────────────┘
                                 │
                                 │ HTTPS + JWT
                                 │
                    ┌────────────▼────────────┐
                    │  Microsoft Entra        │
                    │  External ID            │
                    │                         │
                    │  OAuth 2.0 / OIDC       │
                    └────────────┬────────────┘
                                 │
                                 │ JWT Token
                                 │
                    ┌────────────▼────────────┐
                    │  Azure API Management   │
                    │  (API Gateway)          │
                    │                         │
                    │  - JWT Validation       │
                    │  - Rate Limiting        │
                    │  - CORS                 │
                    │  - Routing              │
                    │                         │
                    │  Port: 443              │
                    └─────────┬───────────────┘
                              │
              ┌───────────────┼───────────────┐
              │               │               │
              │               │               │
  ┌───────────▼─────┐  ┌──────▼──────┐  ┌────▼──────┐
  │  User Service   │  │   Order     │  │ Inventory │
  │  (Container App)│  │   Service   │  │  Service  │
  │                 │  │ (Phase 2)   │  │ (Phase 2) │
  │  .NET 8 API     │  │             │  │           │
  │  - Clean Arch   │  │             │  │           │
  │  - SOLID        │  │             │  │           │
  │  - CQRS         │  │             │  │           │
  │  - EF Core      │  │             │  │           │
  │                 │  │             │  │           │
  │  Port: 8080     │  │             │  │           │
  └────┬──────┬─────┘  └─────────────┘  └───────────┘
       │      │
       │      │
       │      └─────────────────┬─────────────────┐
       │                        │                 │
       │               ┌────────▼────────┐  ┌─────▼──────┐
       │               │  Apache Kafka   │  │   Redis    │
       │               │  (KRaft Mode)   │  │   Cache    │
       │               │                 │  │            │
       │               │  Event Bus      │  │  Sessions  │
       │               │  Port: 9092     │  │  Port:6379 │
       │               └─────────────────┘  └────────────┘
       │
       │
  ┌────▼─────────────┐
  │   PostgreSQL     │
  │   Flexible       │
  │   Server         │
  │                  │
  │   Database       │
  │   Port: 5432     │
  └──────────────────┘

┌──────────────────────┐
│ Application Insights │
│                      │
│  Monitoring          │
│  (all services)      │
└──────────────────────┘
```

## C4 Model - Level 3: User Service Component Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        User Service                             │
│                     (Azure Container App)                       │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐     │
│  │              API Layer (Presentation)                   │     │
│  │                                                         │     │
│  │  ┌──────────────────┐  ┌──────────────────┐             │     │
│  │  │ UsersController  │  │  Middleware      │             │     │
│  │  │                  │  │                  │             │     │
│  │  │ - POST /signup   │  │ - Exception      │             │     │
│  │  │ - POST /signin   │  │ - Auth           │             │     │
│  │  │ - POST /signout  │  │ - Logging        │             │     │
│  │  │ - GET /me        │  │                  │             │     │
│  │  │ - POST /refresh  │  │                  │             │     │
│  │  └────────┬─────────┘  └──────────────────┘             │     │
│  │           │                                             │     │
│  └───────────┼─────────────────────────────────────────────┘     │
│              │ MediatR                                           │
│  ┌───────────▼────────────────────────────────────────────┐     │
│  │         Application Layer (Use Cases - CQRS)           │     │
│  │                                                        │     │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │    │
│  │  │   Commands   │  │   Queries    │  │  Validators  │  │    │
│  │  │              │  │              │  │              │  │    │
│  │  │ SignupUser   │  │ GetUserProfile│ │ FluentValid  │  │    │
│  │  │ SigninUser   │  │ GetSessions  │  │              │  │    │
│  │  │ SignoutUser  │  │              │  │              │  │    │
│  │  │ RefreshToken │  │              │  │              │  │    │
│  │  └──────┬───────┘  └──────┬───────┘  └──────────────┘   │    │
│  │         │                  │                            │    │
│  └─────────┼──────────────────┼────────────────────────────┘    │
│            │                  │                                 │
│  ┌─────────▼──────────────────▼────────────────────────────┐    │
│  │           Domain Layer (Business Logic)                 │    │
│  │                                                          │    │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │    │
│  │  │   Entities   │  │ Value Objects│  │    Events    │ │    │
│  │  │              │  │              │  │              │ │    │
│  │  │ User         │  │ Email        │  │UserRegistered│ │    │
│  │  │ UserSession  │  │PasswordHash  │  │UserSignedIn  │ │    │
│  │  │              │  │ RefreshToken │  │UserSignedOut │ │    │
│  │  └──────────────┘  └──────────────┘  └──────┬───────┘ │    │
│  │                                               │         │    │
│  └───────────────────────────────────────────────┼─────────┘    │
│                                                  │               │
│  ┌───────────────────────────────────────────────▼─────────┐    │
│  │         Infrastructure Layer (External)                 │    │
│  │                                                          │    │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │    │
│  │  │ UserRepository│  │ TokenService │  │SessionService│ │    │
│  │  │              │  │              │  │              │ │    │
│  │  │ EF Core      │  │ JWT Gen/Val  │  │ Redis Mgmt   │ │    │
│  │  └──────┬───────┘  └──────────────┘  └──────┬───────┘ │    │
│  │         │                                    │         │    │
│  │  ┌──────▼───────┐  ┌──────────────┐  ┌──────▼───────┐ │    │
│  │  │PasswordHasher│  │KafkaPublisher│  │EntraExternal │ │    │
│  │  │              │  │              │  │   Service    │ │    │
│  │  │ BCrypt       │  │ Domain Events│  │              │ │    │
│  │  └──────────────┘  └──────┬───────┘  └──────────────┘ │    │
│  │                           │                            │    │
│  └───────────────────────────┼────────────────────────────┘    │
│                              │                                 │
└──────────────────────────────┼─────────────────────────────────┘
                               │
                               ▼
                    ┌──────────────────┐
                    │  Apache Kafka    │
                    │  (user-events)   │
                    └──────────────────┘
```

## Authentication Flow Sequence Diagram

### User Signup Flow

```
User          React App      Entra ID       APIM         User Service     PostgreSQL    Redis       Kafka
 │               │              │             │                │              │           │           │
 │─Register────▶│              │             │                │              │           │           │
 │               │              │             │                │              │           │           │
 │               │─OAuth Flow─▶│             │                │              │           │           │
 │               │              │             │                │              │           │           │
 │◀────OAuth────│◀─ID Token───│             │                │              │           │           │
 │    Redirect   │              │             │                │              │           │           │
 │               │              │             │                │              │           │           │
 │               │─POST /api/users/signup────▶│                │              │           │           │
 │               │  (with ID token)           │                │              │           │           │
 │               │              │             │─Validate JWT──▶│              │           │           │
 │               │              │             │                │              │           │           │
 │               │              │             │                │─Verify Token─▶│           │           │
 │               │              │             │                │  (Entra ID)   │           │           │
 │               │              │             │                │◀──────────────│           │           │
 │               │              │             │                │              │           │           │
 │               │              │             │                │─Create User──▶│           │           │
 │               │              │             │                │              │           │           │
 │               │              │             │                │◀─User Saved──│           │           │
 │               │              │             │                │              │           │           │
 │               │              │             │                │─Gen Tokens───│           │           │
 │               │              │             │                │  (Access +   │           │           │
 │               │              │             │                │   Refresh)   │           │           │
 │               │              │             │                │              │           │           │
 │               │              │             │                │─Store Session────────────▶│           │
 │               │              │             │                │   (refresh token)         │           │
 │               │              │             │                │◀──────────────────────────│           │
 │               │              │             │                │              │           │           │
 │               │              │             │                │─Publish──────────────────────────────▶│
 │               │              │             │                │  UserRegisteredEvent      │           │
 │               │              │             │                │              │           │           │
 │               │              │             │◀─User Profile──│              │           │           │
 │               │              │             │   + Tokens     │              │           │           │
 │               │◀─Response (200 OK)────────│                │              │           │           │
 │               │   {user, accessToken,      │                │              │           │           │
 │               │    refreshToken}           │                │              │           │           │
 │◀─Show Profile─│              │             │                │              │           │           │
 │               │              │             │                │              │           │           │
```

### User Signin Flow

```
User          React App      Entra ID       APIM         User Service     PostgreSQL    Redis       Kafka
 │               │              │             │                │              │           │           │
 │─Login────────▶│              │             │                │              │           │           │
 │               │              │             │                │              │           │           │
 │               │─OAuth Flow─▶│             │                │              │           │           │
 │               │              │             │                │              │           │           │
 │◀────OAuth────│◀─ID Token───│             │                │              │           │           │
 │    Redirect   │              │             │                │              │           │           │
 │               │              │             │                │              │           │           │
 │               │─POST /api/users/signin─────▶│                │              │           │           │
 │               │  (with ID token)           │                │              │           │           │
 │               │              │             │─Validate JWT──▶│              │           │           │
 │               │              │             │                │              │           │           │
 │               │              │             │                │─Get User─────▶│           │           │
 │               │              │             │                │              │           │           │
 │               │              │             │                │◀─User Found──│           │           │
 │               │              │             │                │              │           │           │
 │               │              │             │                │─Update────────▶│           │           │
 │               │              │             │                │  last_login_at│           │           │
 │               │              │             │                │              │           │           │
 │               │              │             │                │─Gen Tokens───│           │           │
 │               │              │             │                │              │           │           │
 │               │              │             │                │─Store Session────────────▶│           │
 │               │              │             │                │◀──────────────────────────│           │
 │               │              │             │                │              │           │           │
 │               │              │             │                │─Publish──────────────────────────────▶│
 │               │              │             │                │  UserSignedInEvent        │           │
 │               │              │             │                │              │           │           │
 │               │              │             │◀─Response──────│              │           │           │
 │               │◀─Tokens + Profile──────────│                │              │           │           │
 │◀─Dashboard───│              │             │                │              │           │           │
 │               │              │             │                │              │           │           │
```

### API Request with JWT

```
User          React App       APIM         User Service     Redis
 │               │              │                │              │
 │─View Profile─▶│              │                │              │
 │               │              │                │              │
 │               │─GET /api/users/me───────────▶│              │
 │               │  Authorization: Bearer {JWT} │              │
 │               │              │                │              │
 │               │              │─Validate JWT──▶│              │
 │               │              │  (stateless)   │              │
 │               │              │                │              │
 │               │              │                │─Check Cache──▶│
 │               │              │                │              │
 │               │              │                │◀─Cache Hit───│
 │               │              │                │  (user data) │
 │               │              │                │              │
 │               │              │◀─User Profile──│              │
 │               │◀─Response────│                │              │
 │◀─Display─────│              │                │              │
 │               │              │                │              │
```

### Token Refresh Flow

```
React App       APIM         User Service     Redis          PostgreSQL
 │               │                │              │                │
 │─POST /api/users/refresh────────▶│              │                │
 │  {refreshToken}                │              │                │
 │               │                │              │                │
 │               │─Forward────────▶│              │                │
 │               │                │              │                │
 │               │                │─Validate─────▶│                │
 │               │                │  Session     │                │
 │               │                │              │                │
 │               │                │◀─Valid───────│                │
 │               │                │  Session     │                │
 │               │                │              │                │
 │               │                │─Get User─────────────────────▶│
 │               │                │              │                │
 │               │                │◀─User────────────────────────│
 │               │                │              │                │
 │               │                │─Generate New Tokens           │
 │               │                │              │                │
 │               │                │─Rotate Refresh Token──────────▶│
 │               │                │  (invalidate old,              │
 │               │                │   store new)                   │
 │               │                │              │                │
 │               │                │◀──────────────│                │
 │               │                │              │                │
 │               │◀─New Tokens────│              │                │
 │◀─Response─────│                │              │                │
 │  {accessToken, refreshToken}   │              │                │
 │               │                │              │                │
```

### Signout Flow

```
User          React App       APIM         User Service     Redis       Kafka
 │               │              │                │              │           │
 │─Signout──────▶│              │                │              │           │
 │               │              │                │              │           │
 │               │─POST /api/users/signout──────▶│              │           │
 │               │  {refreshToken}               │              │           │
 │               │              │                │              │           │
 │               │              │─Forward────────▶│              │           │
 │               │              │                │              │           │
 │               │              │                │─Revoke───────▶│           │
 │               │              │                │  Session     │           │
 │               │              │                │  (delete     │           │
 │               │              │                │   from Redis)│           │
 │               │              │                │              │           │
 │               │              │                │◀─────────────│           │
 │               │              │                │              │           │
 │               │              │                │─Publish──────────────────▶│
 │               │              │                │  UserSignedOutEvent       │
 │               │              │                │              │           │
 │               │              │◀─Success───────│              │           │
 │               │◀─Response────│                │              │           │
 │               │  (204 No Content)             │              │           │
 │               │              │                │              │           │
 │               │─Clear Local Storage           │              │           │
 │               │  (tokens)                     │              │           │
 │               │              │                │              │           │
 │               │─MSAL Logout─▶│                │              │           │
 │               │  (Entra ID)  │                │              │           │
 │               │              │                │              │           │
 │◀─Redirect─────│              │                │              │           │
 │  to Home      │              │                │              │           │
 │               │              │                │              │           │
```

## Data Flow Architecture

### Write Path (Command):

```
API Request → APIM → User Service → Command Handler → Domain Entity → Repository → PostgreSQL
                                                            │
                                                            └──▶ Domain Event → Kafka Publisher → Kafka Topic
```

### Read Path (Query):

```
API Request → APIM → User Service → Query Handler → Check Redis Cache
                                                            │
                                                            ├─ Cache Hit → Return Data
                                                            │
                                                            └─ Cache Miss → Repository → PostgreSQL → Update Cache → Return Data
```

## Scalability & Performance

### Horizontal Scaling:

- **Container Apps**: Auto-scale based on HTTP requests or CPU/memory
- **Kafka**: Add partitions for increased throughput
- **PostgreSQL**: Read replicas for read-heavy workloads
- **Redis**: Redis Cluster for high availability

### Caching Strategy:

- **User Profiles**: 15-minute TTL
- **Session Data**: Match token expiry (7 days)
- **Frequently Accessed Data**: Implement cache-aside pattern

### Performance Optimizations:

- **Database**: Indexes on frequently queried columns
- **API**: Response compression (gzip)
- **Frontend**: Code splitting, lazy loading
- **CDN**: Static assets served from Azure CDN

## Resilience & Fault Tolerance

### Circuit Breaker Pattern:

- Implement Polly for transient fault handling
- Open circuit after consecutive failures
- Half-open state for testing recovery

### Retry Policies:

- **Kafka**: Automatic retry with exponential backoff
- **Database**: Retry on transient errors
- **External APIs**: Polly retry policies

### Fallback Strategies:

- **Cache**: Serve stale data if database unavailable
- **Kafka**: Dead letter queue for failed messages
- **API**: Return cached responses during outages

## Security Architecture

### Defense in Depth:

1. **Network**: Virtual Network with NSGs
2. **API Gateway**: APIM with JWT validation
3. **Application**: Input validation, output encoding
4. **Data**: Encryption at rest and in transit
5. **Secrets**: Azure Key Vault for credentials

### Authentication Layers:

```
User Browser → SSL/TLS → APIM (JWT Validation) → Container Apps (Additional Validation) → Service Logic
```

### Authorization:

- **Role-Based Access Control (RBAC)**: User, Admin roles
- **Claims-Based**: JWT claims for user identity
- **Policy-Based**: Define policies in User Service

---

**Next**: See [02-INFRASTRUCTURE.md](02-INFRASTRUCTURE.md) for Terraform implementation details.
