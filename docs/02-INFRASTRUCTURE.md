# Infrastructure as Code - Terraform

## Overview

This document describes the Terraform infrastructure setup for multi-environment deployment (dev/staging/prod) with reusable modules.

## Design Principles

1. **DRY (Don't Repeat Yourself)**: Reusable modules for all Azure resources
2. **Environment Separation**: Separate state files and configurations per environment
3. **Cost Optimization**: Environment-specific SKUs and scaling configurations
4. **Security**: Key Vault for secrets, managed identities, network isolation
5. **Scalability**: Consumption-based resources where possible

## Directory Structure

```
infrastructure/terraform/
├── modules/                          # Reusable modules
│   ├── resource-group/
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   └── outputs.tf
│   │
│   ├── container-apps/
│   │   ├── environment.tf           # Container Apps Environment
│   │   ├── user-service.tf          # User Service app
│   │   ├── variables.tf
│   │   └── outputs.tf
│   │
│   ├── postgresql/
│   │   ├── main.tf                  # Flexible Server
│   │   ├── firewall-rules.tf
│   │   ├── variables.tf
│   │   └── outputs.tf
│   │
│   ├── redis/
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   └── outputs.tf
│   │
│   ├── apim/
│   │   ├── apim.tf                  # API Management instance
│   │   ├── apis.tf                  # API definitions
│   │   ├── policies.tf              # Global and API policies
│   │   ├── backends.tf              # Backend services
│   │   ├── variables.tf
│   │   └── outputs.tf
│   │
│   ├── acr/
│   │   ├── main.tf                  # Container Registry
│   │   ├── variables.tf
│   │   └── outputs.tf
│   │
│   ├── key-vault/
│   │   ├── main.tf
│   │   ├── secrets.tf
│   │   ├── access-policies.tf
│   │   ├── variables.tf
│   │   └── outputs.tf
│   │
│   ├── entra-external-id/
│   │   ├── app-registrations.tf    # SPA and API registrations
│   │   ├── variables.tf
│   │   └── outputs.tf
│   │
│   └── monitoring/
│       ├── app-insights.tf
│       ├── log-analytics.tf
│       ├── variables.tf
│       └── outputs.tf
│
├── environments/
│   ├── dev/
│   │   ├── main.tf                  # Imports modules
│   │   ├── variables.tf
│   │   ├── terraform.tfvars         # Dev-specific values
│   │   ├── backend.tf               # State backend config
│   │   └── providers.tf
│   │
│   ├── staging/
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   ├── terraform.tfvars
│   │   ├── backend.tf
│   │   └── providers.tf
│   │
│   └── prod/
│       ├── main.tf
│       ├── variables.tf
│       ├── terraform.tfvars
│       ├── backend.tf
│       └── providers.tf
│
└── shared/
    ├── common-variables.tf          # Common defaults
    └── naming-conventions.tf        # Resource naming
```

## Module Descriptions

### 1. Resource Group Module

**Purpose**: Create and manage Azure Resource Group

**Inputs**:

- `name`: Resource group name
- `location`: Azure region
- `tags`: Resource tags

**Outputs**:

- `id`: Resource group ID
- `name`: Resource group name
- `location`: Resource group location

**Example Usage**:

```hcl
module "resource_group" {
  source   = "../../modules/resource-group"
  name     = "rg-orderapp-dev-eastus"
  location = "eastus"
  tags = {
    Environment = "dev"
    Project     = "OrderProcessing"
    ManagedBy   = "Terraform"
  }
}
```

---

### 2. Container Apps Module

**Purpose**: Azure Container Apps Environment and individual container apps

**Inputs**:

- `name`: Container Apps Environment name
- `resource_group_name`: Parent resource group
- `location`: Azure region
- `log_analytics_workspace_id`: Workspace for logs
- `apps`: Map of container app configurations
  - `name`: App name
  - `container_image`: Docker image
  - `min_replicas`: Minimum replica count
  - `max_replicas`: Maximum replica count
  - `cpu`: CPU allocation (e.g., "0.5")
  - `memory`: Memory allocation (e.g., "1Gi")
  - `env_vars`: Environment variables
  - `secrets`: Secret values

**Outputs**:

- `environment_id`: Container Apps Environment ID
- `app_urls`: Map of app names to URLs

**Example**:

```hcl
module "container_apps" {
  source                      = "../../modules/container-apps"
  name                        = "cae-orderapp-dev"
  resource_group_name         = module.resource_group.name
  location                    = module.resource_group.location
  log_analytics_workspace_id  = module.monitoring.workspace_id

  apps = {
    user-service = {
      name            = "user-service"
      container_image = "${module.acr.login_server}/user-service:latest"
      min_replicas    = 0  # Scale to zero in dev
      max_replicas    = 2
      cpu             = "0.5"
      memory          = "1Gi"
      env_vars = {
        ASPNETCORE_ENVIRONMENT = "Development"
        ConnectionStrings__DefaultConnection = module.postgresql.connection_string
        ConnectionStrings__Redis = module.redis.connection_string
      }
      secrets = {
        JWT_SECRET = module.key_vault.jwt_secret
      }
    }
  }
}
```

---

### 3. PostgreSQL Module

**Purpose**: Azure Database for PostgreSQL Flexible Server

**Inputs**:

- `name`: Server name
- `resource_group_name`: Parent resource group
- `location`: Azure region
- `sku_name`: SKU (e.g., "B_Standard_B1ms", "GP_Standard_D2s_v3")
- `storage_mb`: Storage in MB
- `backup_retention_days`: Backup retention
- `admin_username`: Administrator username
- `admin_password`: Administrator password (from Key Vault)
- `databases`: List of database names to create
- `firewall_rules`: List of firewall rules

**Outputs**:

- `server_fqdn`: Fully qualified domain name
- `connection_string`: Connection string
- `admin_username`: Admin username

**SKU Recommendations**:

- **Dev**: `B_Standard_B1ms` (1 vCore, 2 GB RAM) ~$12/month
- **Staging**: `GP_Standard_D2s_v3` (2 vCore, 8 GB RAM) ~$80/month
- **Prod**: `GP_Standard_D4s_v3` (4 vCore, 16 GB RAM) ~$160/month

**Example**:

```hcl
module "postgresql" {
  source              = "../../modules/postgresql"
  name                = "psql-orderapp-dev"
  resource_group_name = module.resource_group.name
  location            = module.resource_group.location
  sku_name            = "B_Standard_B1ms"
  storage_mb          = 32768  # 32 GB
  backup_retention_days = 7
  admin_username      = "orderadmin"
  admin_password      = module.key_vault.postgresql_password

  databases = ["orderdb", "auditdb"]

  firewall_rules = [
    {
      name             = "AllowAzureServices"
      start_ip_address = "0.0.0.0"
      end_ip_address   = "0.0.0.0"
    }
  ]
}
```

---

### 4. Redis Cache Module

**Purpose**: Azure Cache for Redis

**Inputs**:

- `name`: Cache name
- `resource_group_name`: Parent resource group
- `location`: Azure region
- `sku_name`: SKU (Basic, Standard, Premium)
- `capacity`: Cache size (0-6 depending on SKU)
- `enable_non_ssl_port`: Allow non-SSL connections

**Outputs**:

- `hostname`: Redis hostname
- `ssl_port`: SSL port (6380)
- `connection_string`: Connection string
- `primary_access_key`: Primary key

**SKU Recommendations**:

- **Dev**: Basic C0 (250 MB) ~$16/month
- **Staging**: Standard C1 (1 GB) ~$75/month
- **Prod**: Premium P1 (6 GB, HA) ~$250/month

**Example**:

```hcl
module "redis" {
  source              = "../../modules/redis"
  name                = "redis-orderapp-dev"
  resource_group_name = module.resource_group.name
  location            = module.resource_group.location
  sku_name            = "Basic"
  capacity            = 0
  enable_non_ssl_port = false
}
```

---

### 5. API Management Module

**Purpose**: Azure API Management (API Gateway)

**Inputs**:

- `name`: APIM instance name
- `resource_group_name`: Parent resource group
- `location`: Azure region
- `sku_name`: SKU (Consumption, Developer, Standard, Premium)
- `publisher_name`: Organization name
- `publisher_email`: Contact email
- `apis`: Map of API definitions
- `backend_services`: Map of backend services
- `jwt_validation_policy`: JWT validation configuration

**Outputs**:

- `gateway_url`: API gateway URL
- `portal_url`: Developer portal URL

**SKU Recommendations**:

- **Dev/Staging**: Consumption (~$0.035 per 10k calls)
- **Prod**: StandardV2 or Consumption (based on traffic)

**Example**:

```hcl
module "apim" {
  source              = "../../modules/apim"
  name                = "apim-orderapp-dev"
  resource_group_name = module.resource_group.name
  location            = module.resource_group.location
  sku_name            = "Consumption"
  publisher_name      = "OrderApp"
  publisher_email     = "admin@orderapp.com"

  apis = {
    user-api = {
      name         = "user-api"
      path         = "users"
      display_name = "User Management API"
      protocols    = ["https"]
      openapi_spec = file("${path.module}/../../../src/services/UserService/UserService.Presentation/swagger.json")
    }
  }

  backend_services = {
    user-service = {
      url = module.container_apps.app_urls["user-service"]
    }
  }

  jwt_validation_policy = {
    enabled               = true
    issuer                = "https://login.microsoftonline.com/${var.entra_tenant_id}/v2.0"
    audience              = module.entra.api_client_id
    required_claims = {
      "scp" = "User.Read User.Write"
    }
  }
}
```

---

### 6. Container Registry Module

**Purpose**: Azure Container Registry for Docker images

**Inputs**:

- `name`: Registry name (globally unique)
- `resource_group_name`: Parent resource group
- `location`: Azure region
- `sku`: SKU (Basic, Standard, Premium)
- `admin_enabled`: Enable admin user

**Outputs**:

- `login_server`: Registry login server
- `admin_username`: Admin username
- `admin_password`: Admin password

**Example**:

```hcl
module "acr" {
  source              = "../../modules/acr"
  name                = "acrorderappdev"  # Must be globally unique
  resource_group_name = module.resource_group.name
  location            = module.resource_group.location
  sku                 = "Basic"
  admin_enabled       = true
}
```

---

### 7. Key Vault Module

**Purpose**: Azure Key Vault for secrets management

**Inputs**:

- `name`: Key Vault name
- `resource_group_name`: Parent resource group
- `location`: Azure region
- `sku_name`: SKU (standard, premium)
- `tenant_id`: Azure AD tenant ID
- `access_policies`: List of access policies
- `secrets`: Map of secrets to create

**Outputs**:

- `vault_uri`: Key Vault URI
- `secrets`: Map of secret names to values

**Example**:

```hcl
module "key_vault" {
  source              = "../../modules/key-vault"
  name                = "kv-orderapp-dev"
  resource_group_name = module.resource_group.name
  location            = module.resource_group.location
  sku_name            = "standard"
  tenant_id           = var.tenant_id

  access_policies = [
    {
      object_id = data.azurerm_client_config.current.object_id
      secret_permissions = ["Get", "List", "Set", "Delete"]
    },
    {
      object_id = module.container_apps.user_service_identity_id
      secret_permissions = ["Get", "List"]
    }
  ]

  secrets = {
    postgresql-password = random_password.postgresql_password.result
    jwt-secret          = random_password.jwt_secret.result
    jwt-issuer          = "https://login.microsoftonline.com/${var.entra_tenant_id}/v2.0"
    jwt-audience        = module.entra.api_client_id
  }
}
```

---

### 8. Entra External ID Module

**Purpose**: Configure Microsoft Entra External ID (Azure AD B2C successor)

**Inputs**:

- `tenant_name`: External ID tenant name
- `spa_app_name`: Single Page Application registration name
- `api_app_name`: API application registration name
- `spa_redirect_uris`: List of redirect URIs for SPA
- `api_scopes`: List of scopes to expose

**Outputs**:

- `spa_client_id`: SPA application client ID
- `api_client_id`: API application client ID
- `authority`: Authority URL
- `token_endpoint`: Token endpoint

**Example**:

```hcl
module "entra" {
  source         = "../../modules/entra-external-id"
  tenant_name    = "orderappdev"
  spa_app_name   = "OrderApp Frontend"
  api_app_name   = "OrderApp API"

  spa_redirect_uris = [
    "http://localhost:3000",
    "https://orderapp-dev.azurewebsites.net"
  ]

  api_scopes = [
    "User.Read",
    "User.Write",
    "Order.Create",
    "Order.Read"
  ]
}
```

---

### 9. Monitoring Module

**Purpose**: Application Insights and Log Analytics

**Inputs**:

- `name_prefix`: Prefix for resource names
- `resource_group_name`: Parent resource group
- `location`: Azure region
- `retention_in_days`: Log retention period

**Outputs**:

- `app_insights_instrumentation_key`: Application Insights key
- `app_insights_connection_string`: Connection string
- `workspace_id`: Log Analytics workspace ID

**Example**:

```hcl
module "monitoring" {
  source              = "../../modules/monitoring"
  name_prefix         = "orderapp-dev"
  resource_group_name = module.resource_group.name
  location            = module.resource_group.location
  retention_in_days   = 30
}
```

---

## Environment-Specific Configuration

### Dev Environment (`environments/dev/terraform.tfvars`)

```hcl
# General
environment = "dev"
location    = "eastus"
project     = "orderapp"

# Cost-optimized for development
postgresql_sku_name         = "B_Standard_B1ms"
postgresql_storage_mb       = 32768
postgresql_backup_retention = 7

redis_sku_name  = "Basic"
redis_capacity  = 0

apim_sku_name = "Consumption"

# Container Apps - Scale to zero when idle
container_apps_min_replicas = 0
container_apps_max_replicas = 2

# Monitoring
log_retention_days = 30

# Tags
tags = {
  Environment = "Development"
  Project     = "OrderProcessing"
  ManagedBy   = "Terraform"
  CostCenter  = "Engineering"
}
```

### Staging Environment (`environments/staging/terraform.tfvars`)

```hcl
# General
environment = "staging"
location    = "eastus"
project     = "orderapp"

# Mid-tier for staging
postgresql_sku_name         = "GP_Standard_D2s_v3"
postgresql_storage_mb       = 131072
postgresql_backup_retention = 14

redis_sku_name  = "Standard"
redis_capacity  = 1

apim_sku_name = "Consumption"

# Container Apps - Always running with 1 replica minimum
container_apps_min_replicas = 1
container_apps_max_replicas = 5

# Monitoring
log_retention_days = 60

tags = {
  Environment = "Staging"
  Project     = "OrderProcessing"
  ManagedBy   = "Terraform"
  CostCenter  = "Engineering"
}
```

### Production Environment (`environments/prod/terraform.tfvars`)

```hcl
# General
environment = "prod"
location    = "eastus"
project     = "orderapp"

# Production-grade resources
postgresql_sku_name         = "GP_Standard_D4s_v3"
postgresql_storage_mb       = 262144
postgresql_backup_retention = 35
postgresql_geo_redundant    = true

redis_sku_name  = "Premium"
redis_capacity  = 1
redis_zones     = ["1", "2", "3"]

apim_sku_name = "StandardV2"

# Container Apps - High availability
container_apps_min_replicas = 2
container_apps_max_replicas = 10

# Monitoring
log_retention_days = 90

tags = {
  Environment = "Production"
  Project     = "OrderProcessing"
  ManagedBy   = "Terraform"
  CostCenter  = "Production"
  Compliance  = "SOC2"
}
```

---

## State Management

### Backend Configuration

Each environment has its own state file stored in Azure Storage.

**Dev Backend** (`environments/dev/backend.tf`):

```hcl
terraform {
  backend "azurerm" {
    resource_group_name  = "rg-terraform-state"
    storage_account_name = "stterraformstate"
    container_name       = "tfstate"
    key                  = "orderapp-dev.tfstate"
  }
}
```

**Staging Backend**:

```hcl
terraform {
  backend "azurerm" {
    resource_group_name  = "rg-terraform-state"
    storage_account_name = "stterraformstate"
    container_name       = "tfstate"
    key                  = "orderapp-staging.tfstate"
  }
}
```

**Production Backend**:

```hcl
terraform {
  backend "azurerm" {
    resource_group_name  = "rg-terraform-state"
    storage_account_name = "stterraformstate"
    container_name       = "tfstate"
    key                  = "orderapp-prod.tfstate"
  }
}
```

---

## Deployment Commands

### Initialize Environment

```bash
# Navigate to environment folder
cd infrastructure/terraform/environments/dev

# Initialize Terraform
terraform init

# Validate configuration
terraform validate

# Plan deployment
terraform plan -out=tfplan

# Apply changes
terraform apply tfplan
```

### Deploy to Specific Environment

```bash
# Development
cd infrastructure/terraform/environments/dev
terraform init
terraform plan -var-file="terraform.tfvars"
terraform apply -var-file="terraform.tfvars"

# Staging
cd ../staging
terraform init
terraform plan -var-file="terraform.tfvars"
terraform apply -var-file="terraform.tfvars"

# Production
cd ../prod
terraform init
terraform plan -var-file="terraform.tfvars"
terraform apply -var-file="terraform.tfvars" -auto-approve=false
```

### Destroy Resources

```bash
# CAUTION: This will destroy all resources
terraform destroy -var-file="terraform.tfvars"
```

---

## Cost Estimation

### Monthly Cost Breakdown by Environment

| Resource             | Dev            | Staging         | Production          |
| -------------------- | -------------- | --------------- | ------------------- |
| Container Apps       | $10-20         | $50-100         | $100-200            |
| PostgreSQL           | $12            | $80             | $160                |
| Redis                | $16            | $75             | $250                |
| APIM                 | $35            | $35             | $700                |
| Container Registry   | $5             | $5              | $5                  |
| Key Vault            | $1             | $1              | $1                  |
| Application Insights | $10            | $30             | $100                |
| Storage (state)      | $1             | $1              | $1                  |
| **Total Est.**       | **$90-105/mo** | **$277-327/mo** | **$1,317-1,417/mo** |

---

## Security Best Practices

1. **Secrets Management**:

   - Store all secrets in Azure Key Vault
   - Use managed identities for service-to-service authentication
   - Rotate secrets regularly

2. **Network Security**:

   - Enable private endpoints for PostgreSQL and Redis (production)
   - Use NSGs to restrict traffic
   - Implement WAF in APIM (production)

3. **Access Control**:

   - Use RBAC for resource access
   - Implement least privilege principle
   - Audit access regularly

4. **Compliance**:
   - Enable Azure Policy for compliance checks
   - Tag all resources appropriately
   - Implement backup and disaster recovery

---

**Next**: See [03-USER-SERVICE.md](03-USER-SERVICE.md) for User Service implementation details.
