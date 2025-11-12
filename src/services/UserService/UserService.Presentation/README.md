# User Service - Presentation Layer

The Presentation Layer is the entry point for the User Service API. It contains controllers, middleware, filters, and configuration for handling HTTP requests and responses.

## 📁 Project Structure

```
UserService.Presentation/
├── Controllers/
│   ├── BaseController.cs          # Base controller with common functionality
│   ├── AuthController.cs          # Authentication endpoints (signup, signin, signout, refresh)
│   └── UserController.cs          # User profile and session management
├── Extensions/
│   ├── SwaggerExtensions.cs       # Swagger/OpenAPI configuration
│   ├── AuthenticationExtensions.cs # JWT authentication setup
│   └── CorsExtensions.cs          # CORS policy configuration
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs # Global exception handling
├── Program.cs                     # Application entry point
├── appsettings.json              # Application configuration
└── appsettings.Development.json  # Development-specific settings
```

## 🎯 Key Features

### Controllers

#### AuthController

- **POST /api/auth/signup** - Register a new user account
- **POST /api/auth/signin** - Authenticate and obtain JWT tokens
- **POST /api/auth/signout** - Revoke refresh token and sign out
- **POST /api/auth/refresh** - Refresh access token using refresh token

#### UserController (Requires Authentication)

- **GET /api/user/profile** - Get authenticated user's profile
- **GET /api/user/sessions** - Get all active sessions for the user

### Middleware

#### ExceptionHandlingMiddleware

- Global exception handling for consistent error responses
- Maps domain exceptions to appropriate HTTP status codes
- Returns standardized error response format (RFC 7807-like)
- Handles:
  - `ValidationException` → 400 Bad Request
  - `DomainException` → 400 Bad Request
  - `UnauthorizedAccessException` → 401 Unauthorized
  - `KeyNotFoundException` → 404 Not Found
  - Unhandled exceptions → 500 Internal Server Error

### Extensions

#### SwaggerExtensions

- Configures Swagger/OpenAPI documentation
- Adds JWT bearer authentication to Swagger UI
- Enables annotations for better API documentation

#### AuthenticationExtensions

- Configures JWT bearer authentication
- Sets up token validation parameters
- Custom authentication event handlers

#### CorsExtensions

- Configures CORS policies
- Allows requests from configured origins
- Enables credentials for cross-origin requests

## ⚙️ Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=userservice;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-chars-long-change-in-production",
    "Issuer": "UserService",
    "Audience": "UserServiceAPI",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "ProducerName": "user-service-producer"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:5173"]
  }
}
```

### Environment Variables (Production)

For production deployments, use environment variables or Azure Key Vault:

- `ConnectionStrings__DefaultConnection` - PostgreSQL connection string
- `ConnectionStrings__Redis` - Redis connection string
- `Jwt__SecretKey` - JWT signing key (minimum 32 characters)
- `Jwt__Issuer` - JWT issuer claim
- `Jwt__Audience` - JWT audience claim
- `Kafka__BootstrapServers` - Kafka broker addresses

## 🚀 Running the Application

### Prerequisites

- .NET 8 SDK
- PostgreSQL 17.6+ running
- Redis 8.0+ running
- Kafka 3.9.1+ running

### Development Mode

```bash
# From the Presentation project directory
dotnet run

# Or from the solution root
dotnet run --project src/services/UserService/UserService.Presentation
```

The API will be available at:

- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001` (Development only)
- Health Check: `https://localhost:5001/health`

### Using Docker Compose

```bash
# From the infrastructure/docker directory
docker-compose up -d

# Run the application
dotnet run --project src/services/UserService/UserService.Presentation
```

## 📖 API Documentation

### Swagger/OpenAPI

In development mode, navigate to the application root URL to access Swagger UI:

- URL: `https://localhost:5001`

Swagger provides:

- Interactive API documentation
- Request/response examples
- Authentication testing (JWT bearer tokens)
- Schema definitions

### Testing Authentication

1. **Sign Up**: POST to `/api/auth/signup` to create an account
2. **Sign In**: POST to `/api/auth/signin` to get access token
3. **Authorize**: Click "Authorize" in Swagger UI and enter: `Bearer {your-token}`
4. **Test Protected Endpoints**: Access `/api/user/profile` and `/api/user/sessions`

## 🔒 Security

### JWT Authentication

- Access tokens expire after 60 minutes (configurable)
- Refresh tokens expire after 7 days (configurable)
- Tokens are signed using HMAC-SHA256
- Token validation includes issuer, audience, and lifetime checks

### Password Security

- Passwords are hashed using BCrypt with salt
- Minimum password requirements enforced via FluentValidation
- Account lockout after failed login attempts

### CORS

- Configured to allow specific origins only
- Credentials enabled for cross-origin requests
- Methods and headers are validated

## 📊 Logging

### Serilog Configuration

Logs are written to:

- Console (structured output)
- File: `logs/userservice-{Date}.log` (rolling daily, 30-day retention)

Log levels:

- Development: Debug and above
- Production: Information and above
- Microsoft libraries: Warning and above

### Request Logging

- All HTTP requests are logged with:
  - Method, path, status code
  - Response time
  - Client IP address
  - User identity (if authenticated)

## 🏥 Health Checks

Health check endpoint: `/health`

Checks:

- PostgreSQL database connectivity
- Redis cache connectivity

Response:

- `200 OK` - All systems healthy
- `503 Service Unavailable` - One or more systems unhealthy

## 🧪 Testing

### Manual Testing with curl

```bash
# Sign Up
curl -X POST https://localhost:5001/api/auth/signup \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123!",
    "fullName": "John Doe"
  }'

# Sign In
curl -X POST https://localhost:5001/api/auth/signin \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123!"
  }'

# Get Profile (requires token)
curl -X GET https://localhost:5001/api/user/profile \
  -H "Authorization: Bearer {your-access-token}"

# Health Check
curl https://localhost:5001/health
```

## 🔧 Troubleshooting

### Common Issues

1. **Database Connection Failed**

   - Verify PostgreSQL is running: `docker ps`
   - Check connection string in appsettings.json
   - Ensure database migrations have been applied

2. **Redis Connection Failed**

   - Verify Redis is running: `docker ps`
   - Check Redis connection string
   - Test connection: `redis-cli ping`

3. **Kafka Connection Failed**

   - Verify Kafka is running: `docker ps`
   - Check bootstrap servers configuration
   - View Kafka logs: `docker logs kafka`

4. **JWT Token Invalid**
   - Ensure JWT secret key is at least 32 characters
   - Verify issuer and audience match configuration
   - Check token expiration time

## 📚 Dependencies

- **ASP.NET Core 8.0** - Web framework
- **MediatR** - CQRS command/query handling
- **FluentValidation** - Request validation
- **Swashbuckle** - OpenAPI/Swagger documentation
- **Serilog** - Structured logging
- **HealthChecks** - Health monitoring

## 🔄 Next Steps

After implementing the Presentation Layer:

1. Create and apply EF Core migrations
2. Run integration tests
3. Configure Azure deployment
4. Set up CI/CD pipeline
5. Implement monitoring and alerting
