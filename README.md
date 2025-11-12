# Kafka with .NET - Order Processing System

A production-ready, event-driven microservices platform demonstrating real-time order processing using Apache Kafka, .NET 8, React, and Azure Container Apps.

## 🎯 Overview

This project showcases modern cloud-native architecture with:

- **Domain-Driven Design (DDD)** with **Clean Architecture**
- **SOLID Principles** applied throughout all layers
- **Event-Driven Architecture** using Apache Kafka (KRaft mode)
- **CQRS Pattern** with MediatR
- **Microservices** deployed on Azure Container Apps
- **Infrastructure as Code** with Terraform
- **Multi-environment setup** (dev/staging/prod)

## 🏗️ Architecture

```
React Frontend → Azure APIM → Container Apps (User/Order/Inventory Services)
                                     ↓
                               Apache Kafka (Event Streaming)
                                     ↓
                            PostgreSQL + Redis + App Insights
```

**Phase 1 (Current)**: User Management with authentication via Microsoft Entra External ID
**Phase 2 (Planned)**: Order processing, inventory management, and notifications

## 🚀 Quick Start

### Prerequisites

- Docker Desktop
- .NET 8 SDK
- Node.js 18+
- Azure subscription (for deployment)

### Local Development

1. **Clone and setup:**

```bash
git clone https://github.com/kshetty/KafkaWithDotNet.git
cd KafkaWithDotNet
cp infrastructure/docker/.env.example infrastructure/docker/.env
# Edit .env with your configuration
```

2. **Start infrastructure:**

```bash
cd infrastructure/docker
docker-compose up -d
```

3. **Run User Service:**

```bash
cd src/services/UserService/UserService.Presentation
dotnet run
```

4. **Run Frontend:**

```bash
cd src/frontend/order-app
npm install
npm run dev
```

5. **Access:**

- Frontend: http://localhost:3000
- API: https://localhost:5001
- Swagger: https://localhost:5001/swagger
- Kafka UI: http://localhost:8080

## 📚 Documentation

Comprehensive documentation is available in the [`docs/`](docs/) folder:

- **[00-PROJECT-OVERVIEW.md](docs/00-PROJECT-OVERVIEW.md)** - Project overview, architecture principles, and phases
- **[01-ARCHITECTURE.md](docs/01-ARCHITECTURE.md)** - Detailed architecture with C4 diagrams and sequence flows
- **[02-INFRASTRUCTURE.md](docs/02-INFRASTRUCTURE.md)** - Terraform modules and multi-environment setup
- **[03-USER-SERVICE.md](docs/03-USER-SERVICE.md)** - User Service implementation (DDD/CQRS)
- **[04-FRONTEND.md](docs/04-FRONTEND.md)** - React application with MSAL authentication
- **[05-LOCAL-DEVELOPMENT.md](docs/05-LOCAL-DEVELOPMENT.md)** - Local development guide and troubleshooting

## 🛠️ Technology Stack

### Backend

- **.NET 8** - Web API
- **Entity Framework Core** - ORM
- **MediatR** - CQRS
- **FluentValidation** - Validation
- **KafkaFlow** - Kafka integration
- **BCrypt.Net** - Password hashing

### Frontend

- **React 18** + **TypeScript**
- **Material-UI** - UI components
- **MSAL.js** - Authentication
- **Axios** - HTTP client
- **React Query** - State management

### Infrastructure

- **Apache Kafka 3.7** (KRaft mode)
- **PostgreSQL 15** - Database
- **Redis 7** - Caching
- **Azure Container Apps** - Compute
- **Azure API Management** - API Gateway
- **Terraform** - IaC

## 📊 Project Structure

```
KafkaWithDotNet/
├── docs/                       # Documentation
├── infrastructure/
│   ├── docker/                 # Docker Compose for local dev
│   └── terraform/              # Terraform IaC
│       ├── modules/            # Reusable modules
│       └── environments/       # Dev/Staging/Prod configs
├── src/
│   ├── frontend/
│   │   └── order-app/          # React application
│   ├── services/
│   │   └── UserService/        # Clean Architecture with SOLID
│   │       ├── Domain/         # Enterprise Business Rules (no dependencies)
│   │       ├── Application/    # Application Business Rules (CQRS)
│   │       ├── Infrastructure/ # Interface Adapters (data access)
│   │       └── Presentation/   # Frameworks & Drivers (REST API)
│   └── shared/                 # Shared libraries
└── tests/                      # Tests
```

## 🔐 Authentication Flow

1. User interacts with React app
2. MSAL initiates Entra External ID OAuth flow
3. User authenticates and receives ID token
4. React calls User Service API via APIM
5. User Service validates token and creates session
6. Access token (15 min) + Refresh token (7 days) issued
7. Session metadata stored in Redis
8. Domain events published to Kafka

## 🌍 Multi-Environment Strategy

| Environment | PostgreSQL       | Redis       | APIM        | Est. Cost/Month |
| ----------- | ---------------- | ----------- | ----------- | --------------- |
| **Dev**     | B1ms (Burstable) | Basic C0    | Consumption | ~$50-80         |
| **Staging** | D2s_v3 (GP)      | Standard C1 | Consumption | ~$200-300       |
| **Prod**    | D4s_v3 (GP)      | Premium P1  | StandardV2  | ~$1200-1500     |

## 🚢 Deployment

### Azure Deployment

```bash
# Initialize Terraform
cd infrastructure/terraform/environments/dev
terraform init

# Plan deployment
terraform plan -var-file="terraform.tfvars"

# Apply infrastructure
terraform apply -var-file="terraform.tfvars"
```

### CI/CD

GitHub Actions workflows automatically:

1. Run Terraform plan/apply
2. Build .NET services → push to ACR
3. Deploy to Azure Container Apps
4. Build React app → deploy to Static Web Apps

## 🧪 Testing

```bash
# Unit tests
cd src/services/UserService/UserService.Tests
dotnet test

# Integration tests
cd tests/Integration.Tests
dotnet test
```

## 📈 Future Phases

- **Phase 2**: Order Service with Kafka producers/consumers
- **Phase 3**: Inventory Service with stock management
- **Phase 4**: Notification Service with SignalR
- **Phase 5**: Event sourcing and analytics

## 🤝 Contributing

This is a learning/demo project. Feel free to fork and experiment!

## 📄 License

See [LICENSE](LICENSE) file for details.

## 📞 Support

For issues or questions, please open a GitHub issue.

---

**Built with ❤️ using .NET, React, Kafka, and Azure**
Sample Kafka implementation with DotNet
