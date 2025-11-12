# Development Environment - Main Configuration

terraform {
  required_version = ">= 1.9.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.14.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.0.2"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6.0"
    }
  }

  # Backend for Dev environment state
  # backend "azurerm" {
  #   resource_group_name  = "terraform-state-rg"
  #   storage_account_name = "tfstatekafkadotnet"
  #   container_name       = "tfstate"
  #   key                  = "dev/kafka-dotnet.tfstate"
  # }
}

# Import shared configuration
module "shared_config" {
  source = "../../shared"
}

# Providers
provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = true
    }

    key_vault {
      purge_soft_delete_on_destroy    = false
      recover_soft_deleted_key_vaults = true
    }

    api_management {
      purge_soft_delete_on_destroy = false
      recover_soft_deleted         = true
    }
  }

  subscription_id = var.subscription_id
}

provider "azuread" {
  tenant_id = var.tenant_id
}

# Resource Group Module
module "resource_group" {
  source = "../../modules/resource-group"

  project_name = var.project_name
  environment  = var.environment
  location     = var.location
  tags         = var.tags
}

# Azure Container Registry Module
module "acr" {
  source = "../../modules/acr"

  resource_group_name = module.resource_group.name
  location            = var.location
  project_name        = var.project_name
  environment         = var.environment
  sku                 = var.acr_sku
  tags                = var.tags
}

# Key Vault Module
module "key_vault" {
  source = "../../modules/key-vault"

  resource_group_name = module.resource_group.name
  location            = var.location
  project_name        = var.project_name
  environment         = var.environment
  tenant_id           = var.tenant_id
  sku_name            = var.key_vault_sku_name
  tags                = var.tags

  # Secrets to store
  secrets = {
    postgresql-admin-password = random_password.postgresql_password.result
    redis-password            = random_password.redis_password.result
    jwt-secret-key            = random_password.jwt_secret.result
    entra-client-secret       = var.entra_client_secret
  }
}

# Monitoring Module
module "monitoring" {
  source = "../../modules/monitoring"

  resource_group_name                 = module.resource_group.name
  location                            = var.location
  project_name                        = var.project_name
  environment                         = var.environment
  log_analytics_retention_days        = var.log_analytics_retention_days
  application_insights_retention_days = var.application_insights_retention_days
  tags                                = var.tags
}

# PostgreSQL Module
module "postgresql" {
  source = "../../modules/postgresql"

  resource_group_name = module.resource_group.name
  location            = var.location
  project_name        = var.project_name
  environment         = var.environment
  postgresql_version  = var.postgresql_version
  sku_name            = var.postgresql_sku_name
  storage_mb          = var.postgresql_storage_mb
  admin_password      = random_password.postgresql_password.result
  tags                = var.tags
}

# Redis Module
module "redis" {
  source = "../../modules/redis"

  resource_group_name = module.resource_group.name
  location            = var.location
  project_name        = var.project_name
  environment         = var.environment
  sku_name            = var.redis_sku_name
  family              = var.redis_family
  capacity            = var.redis_capacity
  redis_version       = var.redis_version
  tags                = var.tags
}

# Container Apps Module
module "container_apps" {
  source = "../../modules/container-apps"

  resource_group_name        = module.resource_group.name
  location                   = var.location
  project_name               = var.project_name
  environment                = var.environment
  workload_profile           = var.container_apps_workload_profile
  min_replicas               = var.container_apps_min_replicas
  max_replicas               = var.container_apps_max_replicas
  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id
  acr_login_server           = module.acr.login_server
  acr_admin_username         = module.acr.admin_username
  acr_admin_password         = module.acr.admin_password

  # Environment variables
  postgresql_connection_string           = module.postgresql.connection_string
  redis_connection_string                = module.redis.connection_string
  application_insights_connection_string = module.monitoring.application_insights_connection_string

  tags = var.tags

  depends_on = [
    module.postgresql,
    module.redis,
    module.monitoring,
    module.acr
  ]
}

# API Management Module
module "apim" {
  source = "../../modules/apim"

  resource_group_name      = module.resource_group.name
  location                 = var.location
  project_name             = var.project_name
  environment              = var.environment
  sku_name                 = var.apim_sku_name
  publisher_name           = var.apim_publisher_name
  publisher_email          = var.apim_publisher_email
  application_insights_id  = module.monitoring.application_insights_id
  application_insights_key = module.monitoring.application_insights_instrumentation_key
  backend_url              = module.container_apps.user_service_url
  tags                     = var.tags

  depends_on = [
    module.container_apps,
    module.monitoring
  ]
}

# Random Passwords
resource "random_password" "postgresql_password" {
  length  = 32
  special = true
}

resource "random_password" "redis_password" {
  length  = 32
  special = true
}

resource "random_password" "jwt_secret" {
  length  = 64
  special = false
}
