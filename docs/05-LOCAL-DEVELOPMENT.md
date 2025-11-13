# Local Development Guide

## Prerequisites

### Required Software:

- **Docker Desktop** (4.25+): For running Kafka, PostgreSQL, Redis
- **.NET 8 SDK**: For running User Service
- **Node.js** (18+) & npm: For React frontend
- **Git**: Version control
- **VS Code** (recommended): With C# and React extensions

### Optional Tools:

- **Azure CLI**: For Azure deployment
- **Terraform CLI**: For infrastructure management
- **Postman/Insomnia**: API testing
- **pgAdmin/DBeaver**: Database management
- **RedisInsight**: Redis management

---

## Quick Start

### 1. Clone Repository

```bash
git clone https://github.com/kshetty/KafkaWithDotNet.git
cd KafkaWithDotNet
```

### 2. Setup Environment Variables

```bash
# Copy example environment file
cp infrastructure/docker/.env.example infrastructure/docker/.env

# Edit .env file with your values
nano infrastructure/docker/.env
```

**Required Environment Variables** (`.env`):

```bash
# Entra External ID (Get from Azure Portal after setup)
ENTRA_TENANT_ID=your-tenant-id
ENTRA_CLIENT_ID=your-client-id
ENTRA_CLIENT_SECRET=your-client-secret
ENTRA_AUTHORITY=https://login.microsoftonline.com/your-tenant-id/v2.0

# Database
POSTGRES_USER=orderadmin
POSTGRES_PASSWORD=YourSecurePassword123!
POSTGRES_DB=orderdb

# Redis
REDIS_PASSWORD=YourRedisPassword123!

# JWT
JWT_SECRET=your-super-secret-jwt-key-min-32-chars
JWT_ISSUER=https://localhost:5001
JWT_AUDIENCE=userservice-api

# Application Insights (optional for local dev)
APPINSIGHTS_CONNECTION_STRING=
```

### 3. Start Infrastructure

```bash
cd infrastructure/docker
docker-compose up -d
```

This starts:

- **Kafka** (KRaft mode) on port 9092
- **PostgreSQL** on port 5432
- **Redis** on port 6379
- **Kafka UI** on port 8080 (for monitoring)

**Verify services:**

```bash
docker-compose ps
```

### 4. Run Database Migrations

```bash
cd src/services/UserService/UserService.Presentation
dotnet ef database update
```

### 5. Run User Service

```bash
cd src/services/UserService/UserService.Presentation
dotnet run
```

Service will be available at: `https://localhost:5001`

Swagger UI: `https://localhost:5001/swagger`

### 6. Run React Frontend

```bash
cd src/frontend/react-web-spa
npm install
npm run dev
```

Frontend will be available at: `http://localhost:3000`

---

## Docker Compose Configuration

### Full docker-compose.yml

```yaml
version: "3.9"

services:
  # Kafka in KRaft mode (No ZooKeeper!)
  kafka:
    image: apache/kafka:3.7.0
    container_name: kafka
    environment:
      # KRaft Configuration
      KAFKA_NODE_ID: 1
      KAFKA_PROCESS_ROLES: "broker,controller"
      KAFKA_CONTROLLER_QUORUM_VOTERS: "1@kafka:9093"
      KAFKA_LISTENERS: "PLAINTEXT://0.0.0.0:9092,CONTROLLER://0.0.0.0:9093,PLAINTEXT_HOST://0.0.0.0:29092"
      KAFKA_ADVERTISED_LISTENERS: "PLAINTEXT://kafka:9092,PLAINTEXT_HOST://localhost:29092"
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: "CONTROLLER:PLAINTEXT,PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT"
      KAFKA_CONTROLLER_LISTENER_NAMES: "CONTROLLER"
      KAFKA_INTER_BROKER_LISTENER_NAME: "PLAINTEXT"

      # Cluster Configuration
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
      KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR: 1
      KAFKA_TRANSACTION_STATE_LOG_MIN_ISR: 1
      KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS: 0

      # Storage
      KAFKA_LOG_DIRS: "/var/lib/kafka/data"
      CLUSTER_ID: "MkU3OEVBNTcwNTJENDM2Qk"
    ports:
      - "9092:9092"
      - "29092:29092"
    volumes:
      - kafka-data:/var/lib/kafka/data
    healthcheck:
      test:
        [
          "CMD-SHELL",
          "kafka-broker-api-versions.sh --bootstrap-server localhost:9092",
        ]
      interval: 10s
      timeout: 5s
      retries: 5

  # Kafka UI for monitoring
  kafka-ui:
    image: provectuslabs/kafka-ui:latest
    container_name: kafka-ui
    ports:
      - "8080:8080"
    environment:
      KAFKA_CLUSTERS_0_NAME: local
      KAFKA_CLUSTERS_0_BOOTSTRAPSERVERS: kafka:9092
    depends_on:
      kafka:
        condition: service_healthy

  # PostgreSQL
  postgres:
    image: postgres:15-alpine
    container_name: postgres
    environment:
      POSTGRES_USER: ${POSTGRES_USER:-orderadmin}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: ${POSTGRES_DB:-orderdb}
    ports:
      - "5432:5432"
    volumes:
      - postgres-data:/var/lib/postgresql/data
      - ./scripts/init-db.sql:/docker-entrypoint-initdb.d/init.sql
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER}"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Redis
  redis:
    image: redis:7-alpine
    container_name: redis
    command: redis-server --requirepass ${REDIS_PASSWORD}
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "--raw", "incr", "ping"]
      interval: 10s
      timeout: 3s
      retries: 5

  # User Service (optional - can run locally with dotnet run)
  user-service:
    build:
      context: ../../src/services/UserService
      dockerfile: UserService.Presentation/Dockerfile
    container_name: user-service
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:8080
      ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"
      ConnectionStrings__Redis: "redis:6379,password=${REDIS_PASSWORD}"
      Kafka__BootstrapServers: "kafka:9092"
      Jwt__Secret: ${JWT_SECRET}
      Jwt__Issuer: ${JWT_ISSUER}
      Jwt__Audience: ${JWT_AUDIENCE}
      EntraExternalId__Authority: ${ENTRA_AUTHORITY}
      EntraExternalId__ClientId: ${ENTRA_CLIENT_ID}
    ports:
      - "5001:8080"
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
      kafka:
        condition: service_healthy

volumes:
  kafka-data:
  postgres-data:
  redis-data:
```

---

## Development Workflow

### Daily Development

1. **Start infrastructure:**

   ```bash
   docker-compose up -d
   ```

2. **Run services:**

   ```bash
   # Terminal 1: User Service
   cd src/services/UserService/UserService.Presentation
   dotnet watch run

   # Terminal 2: Frontend
   cd src/frontend/react-web-spa
   npm run dev
   ```

3. **Make changes and test**

4. **Stop infrastructure:**
   ```bash
   docker-compose down
   ```

### Database Operations

**Run migrations:**

```bash
cd src/services/UserService/UserService.Presentation
dotnet ef migrations add MigrationName
dotnet ef database update
```

**Reset database:**

```bash
docker-compose down -v  # Removes volumes
docker-compose up -d postgres
dotnet ef database update
```

**Access PostgreSQL:**

```bash
docker exec -it postgres psql -U orderadmin -d orderdb
```

### Kafka Operations

**Create topic:**

```bash
docker exec -it kafka kafka-topics.sh \
  --create \
  --bootstrap-server localhost:9092 \
  --topic user-events \
  --partitions 3 \
  --replication-factor 1
```

**List topics:**

```bash
docker exec -it kafka kafka-topics.sh \
  --list \
  --bootstrap-server localhost:9092
```

**Consume messages:**

```bash
docker exec -it kafka kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic user-events \
  --from-beginning
```

**Produce test message:**

```bash
docker exec -it kafka kafka-console-producer.sh \
  --bootstrap-server localhost:9092 \
  --topic user-events
```

### Redis Operations

**Access Redis CLI:**

```bash
docker exec -it redis redis-cli -a YourRedisPassword123!
```

**View all keys:**

```redis
KEYS *
```

**View session:**

```redis
GET session:someTokenHash
```

**Flush all data:**

```redis
FLUSHALL
```

---

## Testing

### Unit Tests

```bash
cd src/services/UserService/UserService.Tests
dotnet test
```

### Integration Tests (with TestContainers)

```bash
cd tests/Integration.Tests
dotnet test
```

### API Testing with curl

**Signup:**

```bash
curl -X POST https://localhost:5001/api/users/signup \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "SecurePass123!",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

**Signin:**

```bash
curl -X POST https://localhost:5001/api/users/signin \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "SecurePass123!"
  }'
```

**Get Profile (with JWT):**

```bash
curl -X GET https://localhost:5001/api/users/me \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

---

## Troubleshooting

### Kafka Issues

**Problem**: Kafka not starting

```bash
# Check logs
docker logs kafka

# Reset Kafka data
docker-compose down -v
docker-compose up -d kafka
```

**Problem**: Can't connect to Kafka

- Ensure port 9092 is not blocked
- Check `KAFKA_ADVERTISED_LISTENERS` configuration
- Use `localhost:29092` from host machine

### Database Issues

**Problem**: Connection refused

```bash
# Check if PostgreSQL is running
docker ps | grep postgres

# Check logs
docker logs postgres

# Verify connection string
```

**Problem**: Migrations fail

```bash
# Drop and recreate database
docker exec -it postgres psql -U orderadmin -c "DROP DATABASE orderdb;"
docker exec -it postgres psql -U orderadmin -c "CREATE DATABASE orderdb;"
dotnet ef database update
```

### Redis Issues

**Problem**: Authentication failed

- Verify `REDIS_PASSWORD` in `.env`
- Update connection string to include password

### .NET Service Issues

**Problem**: Port already in use

```bash
# Find process using port 5001
lsof -i :5001

# Kill process
kill -9 PID
```

**Problem**: SSL certificate issues

```bash
# Trust dev certificate
dotnet dev-certs https --trust
```

---

## Debugging

### VS Code Configuration

**launch.json** (User Service):

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "User Service (Presentation)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/src/services/UserService/UserService.Presentation/bin/Debug/net8.0/UserService.Presentation.dll",
      "args": [],
      "cwd": "${workspaceFolder}/src/services/UserService/UserService.Presentation",
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      }
    }
  ]
}
```

### React Debugging

Enable React DevTools in browser, add breakpoints in browser dev tools.

---

**Next**: See [06-DEPLOYMENT.md](06-DEPLOYMENT.md) for Azure deployment guide.
