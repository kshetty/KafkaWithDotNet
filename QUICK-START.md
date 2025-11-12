# Quick Start Guide - Local Development

## 🚀 Start Development Environment (5 minutes)

### Step 1: Start Services

```bash
cd infrastructure/docker
cp .env.example .env
docker-compose up -d
```

### Step 2: Verify Services

```bash
docker-compose ps
# All services should show "healthy" or "running"
```

### Step 3: Access Interfaces

- **Kafka UI**: http://localhost:8080
- **pgAdmin**: http://localhost:5050 (admin@example.com / admin_password)
- **Redis Commander**: http://localhost:8081
- **User Service**: http://localhost:5001/swagger
- **User Service Health**: http://localhost:5001/health

## 📦 Services Running

| Service          | Port | Version | Purpose                      |
| ---------------- | ---- | ------- | ---------------------------- |
| Kafka            | 9092 | 3.9.1   | Event streaming (KRaft mode) |
| Kafka Controller | 9093 | 3.9.1   | Kafka metadata management    |
| PostgreSQL       | 5432 | 17.6    | Primary database             |
| Redis            | 6379 | 8.0.5   | Cache & sessions             |
| Kafka UI         | 8080 | latest  | Kafka management             |
| pgAdmin          | 5050 | latest  | PostgreSQL management        |
| Redis Commander  | 8081 | latest  | Redis management             |
| User Service     | 5001 | .NET 8  | REST API                     |

## 🔧 Common Commands

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f kafka
docker-compose logs -f userservice
```

### Restart Service

```bash
docker-compose restart userservice
```

### Stop All Services

```bash
docker-compose down
```

### Clean Restart (removes data)

```bash
docker-compose down -v
docker-compose up -d
```

### Rebuild User Service

```bash
docker-compose build userservice
docker-compose up -d userservice
```

## 🗄️ Database Access

### PostgreSQL via psql

```bash
docker-compose exec postgres psql -U postgres -d userservice
```

### Common SQL Commands

```sql
-- List tables
\dt

-- Describe table
\d users

-- Query users
SELECT * FROM users;

-- Exit
\q
```

### PostgreSQL Connection String

```
Host: localhost
Port: 5432
Database: userservice
Username: postgres
Password: postgres_password
```

## 🔴 Redis Access

### Redis CLI

```bash
docker-compose exec redis redis-cli -a redis_password
```

### Common Redis Commands

```redis
# List all keys
KEYS *

# Get value
GET key_name

# Delete key
DEL key_name

# Exit
EXIT
```

## 📨 Kafka Operations

### List Topics

```bash
docker-compose exec kafka kafka-topics.sh \
  --bootstrap-server localhost:9092 \
  --list
```

### Create Topic

```bash
docker-compose exec kafka kafka-topics.sh \
  --bootstrap-server localhost:9092 \
  --create \
  --topic test.topic \
  --partitions 3 \
  --replication-factor 1
```

### Produce Messages

```bash
docker-compose exec kafka kafka-console-producer.sh \
  --bootstrap-server localhost:9092 \
  --topic test.topic
```

### Consume Messages

```bash
docker-compose exec kafka kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic test.topic \
  --from-beginning
```

## 🏗️ Azure Deployment (Dev Environment)

### Prerequisites

```bash
az login
az account set --subscription "your-subscription-id"
```

### Deploy Infrastructure

```bash
cd infrastructure/terraform/environments/dev
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values
terraform init
terraform plan
terraform apply
```

### View Outputs

```bash
terraform output
terraform output user_service_url
```

## 🐛 Troubleshooting

### Services Not Starting

```bash
# Check logs
docker-compose logs

# Check ports
lsof -i :9092
lsof -i :5432
lsof -i :6379

# Restart Docker Desktop
```

### Kafka Connection Issues

```bash
# Remove volumes and restart
docker-compose down -v
docker-compose up -d kafka
```

### User Service Build Failure

```bash
# Build locally to debug
cd src/services/UserService/UserService.Presentation
dotnet restore
dotnet build
```

## 📚 Documentation

- **Docker Compose**: `infrastructure/docker/README.md`
- **Terraform**: `infrastructure/terraform/README.md`
- **Architecture**: `docs/00-PROJECT-OVERVIEW.md`
- **Infrastructure Summary**: `INFRASTRUCTURE-SETUP-SUMMARY.md`

## ⚙️ Configuration Files

- **Environment Variables**: `infrastructure/docker/.env`
- **Docker Compose**: `infrastructure/docker/docker-compose.yml`
- **Terraform Variables**: `infrastructure/terraform/environments/dev/terraform.tfvars`

## 🎯 Next Steps

1. ✅ Services running? → Start implementing Domain Layer
2. ❌ Issues? → Check `infrastructure/docker/README.md` troubleshooting section
3. 🚀 Ready to deploy? → Follow Terraform guide in `infrastructure/terraform/README.md`

---

**Need Help?** Check the comprehensive documentation in:

- `infrastructure/docker/README.md` (450+ lines)
- `infrastructure/terraform/README.md` (400+ lines)
