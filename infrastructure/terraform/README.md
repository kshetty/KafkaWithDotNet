# Terraform Infrastructure - README

This directory contains Terraform Infrastructure as Code (IaC) for deploying the KafkaWithDotNet application to Azure.

## 📋 Prerequisites

- **Terraform** >= 1.9.0
- **Azure CLI** >= 2.50.0
- **Azure Subscription** with appropriate permissions
- **Entra External ID** tenant configured
- **Azure Container Registry** or Docker Hub account

## 🏗️ Architecture

### Technology Versions (Azure-Compatible)

- **Kafka**: Azure Event Hubs (Kafka-compatible API) or Confluent Cloud
- **PostgreSQL**: 17 (Azure Database for PostgreSQL Flexible Server)
- **Redis**: 6.x (Azure Cache for Redis - Redis 8.0 not yet available on Azure)
- **.NET**: 8.0
- **Container Runtime**: Azure Container Apps

**Note**: Local development uses Kafka 3.9.1, PostgreSQL 17.6, and Redis 8.0.5 via Docker Compose. Azure deployment uses managed services with compatible versions.

## 📁 Directory Structure

```
terraform/
├── shared/                    # Shared configuration
│   ├── providers.tf          # Provider configuration
│   ├── variables.tf          # Shared variables
│   ├── locals.tf             # Local values
│   └── outputs.tf            # Shared outputs
├── modules/                   # Reusable modules
│   ├── resource-group/       # Resource group module
│   ├── container-apps/       # Azure Container Apps module
│   ├── postgresql/           # PostgreSQL Flexible Server module
│   ├── redis/                # Azure Cache for Redis module
│   ├── apim/                 # API Management module
│   ├── acr/                  # Container Registry module
│   ├── key-vault/            # Key Vault module
│   └── monitoring/           # Monitoring (Log Analytics + App Insights)
└── environments/              # Environment-specific configurations
    ├── dev/                  # Development environment
    ├── staging/              # Staging environment
    └── prod/                 # Production environment
```

## 🚀 Quick Start

### 1. Setup Azure Authentication

```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "your-subscription-id"

# Verify
az account show
```

### 2. Setup Terraform Backend (Optional but Recommended)

```bash
# Create resource group for Terraform state
az group create --name terraform-state-rg --location eastus

# Create storage account
az storage account create \
  --name tfstatekafkadotnet \
  --resource-group terraform-state-rg \
  --location eastus \
  --sku Standard_LRS \
  --encryption-services blob

# Create container
az storage container create \
  --name tfstate \
  --account-name tfstatekafkadotnet
```

### 3. Configure Environment

```bash
# Navigate to desired environment
cd environments/dev

# Copy example tfvars
cp terraform.tfvars.example terraform.tfvars

# Edit with your values
nano terraform.tfvars
```

### 4. Initialize Terraform

```bash
# Initialize Terraform (downloads providers and modules)
terraform init

# Optional: Configure backend (uncomment backend block in main.tf first)
terraform init -backend-config="resource_group_name=terraform-state-rg" \
               -backend-config="storage_account_name=tfstatekafkadotnet"
```

### 5. Plan and Apply

```bash
# Plan (preview changes)
terraform plan -out=tfplan

# Apply (create resources)
terraform apply tfplan

# Or combine plan and apply
terraform apply -auto-approve
```

## 🌍 Multi-Environment Deployment

### Development

```bash
cd environments/dev
terraform init
terraform plan
terraform apply
```

### Staging

```bash
cd environments/staging
terraform init
terraform plan
terraform apply
```

### Production

```bash
cd environments/prod
terraform init
terraform plan
terraform apply
```

## 📦 Module Details

### Resource Group Module

Creates Azure resource group for organizing resources.

### PostgreSQL Module (Version 17)

- Azure Database for PostgreSQL Flexible Server
- Version 17 (latest)
- Configurable SKU (B_Standard_B1ms for dev)
- Extensions: UUID-OSSP, PGCRYPTO
- UTC timezone
- Firewall rules
- Database creation

### Redis Module

- Azure Cache for Redis
- Version 6.x (Azure managed - Redis 8.0 not yet available)
- Configurable SKU (Basic/Standard/Premium)
- Optional persistence
- Firewall rules

### Container Apps Module

- Azure Container Apps Environment
- User Service container deployment
- Auto-scaling configuration
- Integration with ACR
- Environment variables injection
- Health probes

### API Management Module

- Azure APIM instance
- User Service API configuration
- Policies (rate limiting, authentication, CORS)
- Integration with Application Insights
- Developer portal configuration

### ACR Module

- Azure Container Registry
- Image scanning
- Retention policies
- Admin access (for dev)

### Key Vault Module

- Azure Key Vault
- Secrets management
- Access policies
- Automatic secret rotation support

### Monitoring Module

- Log Analytics Workspace
- Application Insights
- Diagnostic settings
- Alerts and metrics

## 🔒 Security Best Practices

1. **Never commit secrets** - Use Key Vault or environment variables
2. **Enable soft delete** - Key Vault and APIM configured for recovery
3. **Use managed identities** - Avoid storing credentials
4. **Enable diagnostic logging** - All services log to Log Analytics
5. **Network isolation** - Use private endpoints in production
6. **RBAC** - Use role-based access control

## 🔄 Common Operations

### Update Infrastructure

```bash
# Modify Terraform files
# Plan changes
terraform plan -out=tfplan

# Review and apply
terraform apply tfplan
```

### View Outputs

```bash
terraform output

# Specific output
terraform output user_service_url

# JSON format
terraform output -json
```

### Import Existing Resources

```bash
terraform import module.resource_group.azurerm_resource_group.main /subscriptions/SUB_ID/resourceGroups/RG_NAME
```

### Destroy Resources

```bash
# Preview destroy
terraform plan -destroy

# Destroy (be careful!)
terraform destroy

# Destroy specific resource
terraform destroy -target=module.container_apps
```

## 🐛 Troubleshooting

### State Lock Issues

```bash
# Force unlock (use with caution)
terraform force-unlock LOCK_ID
```

### Provider Version Conflicts

```bash
# Upgrade providers
terraform init -upgrade

# Lock provider versions
terraform providers lock
```

### Module Not Found

```bash
# Re-initialize
terraform init -upgrade
```

### Authentication Errors

```bash
# Re-login
az logout
az login
az account set --subscription "your-subscription-id"
```

## 📊 Cost Estimation

### Development Environment (~$100-150/month)

- PostgreSQL Flexible Server (B_Standard_B1ms): ~$13/month
- Redis (Basic C0): ~$17/month
- Container Apps (Consumption): ~$10-20/month
- APIM (Developer): ~$50/month
- Container Registry (Basic): ~$5/month
- Log Analytics: ~$10/month
- Application Insights: ~$5/month

### Production Environment (~$500-1000+/month)

- PostgreSQL (GP_Standard_D2ds_v5): ~$100/month
- Redis (Premium P1): ~$250/month
- Container Apps (Dedicated): ~$100-200/month
- APIM (Standard): ~$200/month
- Other services: ~$50-100/month

Use [Azure Pricing Calculator](https://azure.microsoft.com/pricing/calculator/) for accurate estimates.

## 🔗 Additional Resources

- [Terraform Azure Provider Docs](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure Container Apps](https://learn.microsoft.com/azure/container-apps/)
- [Azure PostgreSQL Flexible Server](https://learn.microsoft.com/azure/postgresql/flexible-server/)
- [Azure Cache for Redis](https://learn.microsoft.com/azure/azure-cache-for-redis/)
- [Azure API Management](https://learn.microsoft.com/azure/api-management/)

## 📝 Notes

- **Redis Version**: Azure Cache for Redis uses 6.x (8.0.5 not yet available). Local dev uses Redis 8.0.5 for testing newer features.
- **Kafka**: For production, consider Azure Event Hubs (Kafka-compatible) or Confluent Cloud instead of self-hosted Kafka.
- **Modules**: Additional modules (ACR, Key Vault, APIM, Container Apps, Monitoring) need to be fully implemented.
- **Networking**: Production should use VNet integration and private endpoints.
- **CI/CD**: Use GitHub Actions workflows in `.github/workflows/` for automated deployments.

## 🤝 Contributing

When adding new modules:

1. Follow existing module structure
2. Include variables.tf, main.tf, outputs.tf
3. Add module documentation
4. Test in dev environment first
5. Update this README
