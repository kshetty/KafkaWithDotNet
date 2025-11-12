# Docker Compose Setup

This directory contains Docker Compose configuration for local development with:

- **Kafka 3.9.1** (KRaft mode - no Zookeeper)
- **PostgreSQL 17.6**
- **Redis 8.0.5**
- **User Service** (.NET 8)
- Management UIs (Kafka UI, pgAdmin, Redis Commander)

## Prerequisites

- Docker Desktop 4.x or later
- Docker Compose V2
- At least 4GB RAM allocated to Docker
- Ports 5001, 5050, 5432, 6379, 8080, 8081, 9092, 9093 available

## Quick Start

### 1. Setup Environment Variables

```bash
# Copy example environment file
cp .env.example .env

# Edit .env with your actual values
nano .env
```

### 2. Start All Services

```bash
# From the infrastructure/docker directory
docker-compose up -d

# View logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f kafka
docker-compose logs -f userservice
```

### 3. Verify Services

```bash
# Check all services are running
docker-compose ps

# Expected output shows all services as "healthy" or "running"
```

### 4. Access Management UIs

- **Kafka UI**: http://localhost:8080
- **pgAdmin**: http://localhost:5050 (admin@example.com / admin_password)
- **Redis Commander**: http://localhost:8081
- **User Service API**: http://localhost:5001/swagger
- **Health Check**: http://localhost:5001/health

## Service Details

### Kafka 3.9.1 (KRaft Mode)

**KRaft Benefits:**

- No Zookeeper dependency (simplified architecture)
- Faster metadata operations
- Better scalability
- Production-ready since Kafka 3.3+

**Bootstrap Server:** kafka:9092 (internal) / localhost:9092 (external)

**Topics Auto-Created:**

- `user.registered` (3 partitions)
- `user.signedin` (3 partitions)

**Configuration:**

- Compression: Snappy
- Retention: 7 days
- Replication Factor: 1 (single node)

### PostgreSQL 17.6

**Connection:**

```
Host: localhost
Port: 5432
Database: userservice
Username: postgres
Password: postgres_password
```

**Features:**

- UUID and crypto extensions enabled
- UTC timezone
- pgAdmin for management

### Redis 8.0.5

**Connection:**

```
Host: localhost
Port: 6379
Password: redis_password
```

**Configuration:**

- Max Memory: 256MB
- Eviction Policy: allkeys-lru
- Persistence: RDB snapshots

### User Service (.NET 8)

**Endpoints:**

```
API: http://localhost:5001
Swagger: http://localhost:5001/swagger
Health: http://localhost:5001/health
```

## Common Commands

### Start/Stop Services

```bash
# Start all services
docker-compose up -d

# Stop all services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v

# Restart specific service
docker-compose restart userservice
```

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f kafka
docker-compose logs -f postgres
docker-compose logs -f userservice

# Last 100 lines
docker-compose logs --tail=100 userservice
```

### Execute Commands in Containers

```bash
# PostgreSQL shell
docker-compose exec postgres psql -U postgres -d userservice

# Redis CLI
docker-compose exec redis redis-cli -a redis_password

# Kafka topics list
docker-compose exec kafka kafka-topics.sh --bootstrap-server localhost:9092 --list

# Create Kafka topic manually
docker-compose exec kafka kafka-topics.sh \
  --bootstrap-server localhost:9092 \
  --create \
  --topic test.topic \
  --partitions 3 \
  --replication-factor 1
```

### Rebuild Services

```bash
# Rebuild User Service after code changes
docker-compose build userservice
docker-compose up -d userservice

# Rebuild and restart
docker-compose up -d --build userservice

# Force rebuild (no cache)
docker-compose build --no-cache userservice
```

## Troubleshooting

### Kafka Not Starting

```bash
# Check logs
docker-compose logs kafka

# Common issues:
# 1. Cluster ID mismatch - Remove volumes and restart
docker-compose down -v
docker-compose up -d

# 2. Port already in use - Check ports
lsof -i :9092
lsof -i :9093
```

### PostgreSQL Connection Issues

```bash
# Check PostgreSQL logs
docker-compose logs postgres

# Test connection
docker-compose exec postgres pg_isready -U postgres

# Recreate database
docker-compose down postgres
docker volume rm kafka-dotnet-postgres_data
docker-compose up -d postgres
```

### User Service Build Failures

```bash
# Check build logs
docker-compose logs --tail=100 userservice

# Rebuild without cache
docker-compose build --no-cache userservice

# Run restore locally first to debug
cd ../../src/services/UserService/UserService.Presentation
dotnet restore
dotnet build
```

### Insufficient Memory

```bash
# Check Docker memory allocation
docker stats

# Increase Docker Desktop memory:
# Docker Desktop → Settings → Resources → Memory (min 4GB)
```

### Port Conflicts

```bash
# Check what's using ports
lsof -i :5001
lsof -i :9092

# Kill process using port
kill -9 <PID>

# Or change ports in docker-compose.yml
```

## Health Checks

All services have health checks configured:

```bash
# Check service health
docker-compose ps

# Healthy services show: Up (healthy)
# Unhealthy services show: Up (unhealthy)
```

## Data Persistence

Data is persisted in Docker volumes:

```bash
# List volumes
docker volume ls | grep kafka-dotnet

# Inspect volume
docker volume inspect kafka-dotnet-postgres_data

# Backup PostgreSQL
docker-compose exec postgres pg_dump -U postgres userservice > backup.sql

# Restore PostgreSQL
docker-compose exec -T postgres psql -U postgres userservice < backup.sql
```

## Cleanup

```bash
# Stop and remove containers
docker-compose down

# Remove volumes (deletes all data)
docker-compose down -v

# Remove images
docker-compose down --rmi all

# Complete cleanup
docker-compose down -v --rmi all --remove-orphans
docker volume prune -f
docker network prune -f
```

## Production Deployment Notes

**This configuration is for LOCAL DEVELOPMENT only.**

For production on Azure:

- Use Azure Container Apps for User Service
- Use Azure Database for PostgreSQL Flexible Server
- Use Azure Cache for Redis
- Use Azure Event Hubs (Kafka-compatible) or Confluent Cloud
- Use Azure Key Vault for secrets
- Enable TLS/SSL for all connections
- Implement proper authentication and authorization
- Configure monitoring and alerting

See `infrastructure/terraform/` for production deployment configurations.

## Additional Resources

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Kafka KRaft Mode](https://kafka.apache.org/documentation/#kraft)
- [PostgreSQL 17 Release Notes](https://www.postgresql.org/docs/17/release-17.html)
- [Redis 8.0 Documentation](https://redis.io/docs/)
- [.NET 8 Docker Guide](https://learn.microsoft.com/en-us/dotnet/core/docker/)
