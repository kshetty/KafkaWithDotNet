# Shared Variables for All Environments

variable "subscription_id" {
  description = "Azure Subscription ID"
  type        = string
  sensitive   = true
}

variable "tenant_id" {
  description = "Azure AD Tenant ID"
  type        = string
  sensitive   = true
}

variable "project_name" {
  description = "Project name used for resource naming"
  type        = string
  default     = "kafkadotnet"

  validation {
    condition     = can(regex("^[a-z0-9]+$", var.project_name))
    error_message = "Project name must contain only lowercase letters and numbers."
  }
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be dev, staging, or prod."
  }
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "eastus"
}

variable "tags" {
  description = "Common tags for all resources"
  type        = map(string)
  default     = {}
}

# Networking Variables
variable "vnet_address_space" {
  description = "Address space for virtual network"
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

# PostgreSQL Variables
variable "postgresql_version" {
  description = "PostgreSQL version"
  type        = string
  default     = "17"
}

variable "postgresql_sku_name" {
  description = "PostgreSQL SKU name"
  type        = string
  default     = "B_Standard_B1ms" # Burstable, 1 vCore, 2 GiB RAM
}

variable "postgresql_storage_mb" {
  description = "PostgreSQL storage in MB"
  type        = number
  default     = 32768 # 32 GB
}

# Redis Variables
variable "redis_sku_name" {
  description = "Redis SKU name"
  type        = string
  default     = "Basic"
}

variable "redis_family" {
  description = "Redis SKU family"
  type        = string
  default     = "C"
}

variable "redis_capacity" {
  description = "Redis cache size (0-6 for Basic/Standard, 1-5 for Premium)"
  type        = number
  default     = 0
}

variable "redis_version" {
  description = "Redis version"
  type        = string
  default     = "6" # Azure Redis uses 6.x (8.0 not yet available)
}

# Container Apps Variables
variable "container_apps_workload_profile" {
  description = "Container Apps workload profile"
  type        = string
  default     = "Consumption"
}

variable "container_apps_min_replicas" {
  description = "Minimum number of replicas"
  type        = number
  default     = 1
}

variable "container_apps_max_replicas" {
  description = "Maximum number of replicas"
  type        = number
  default     = 3
}

# APIM Variables
variable "apim_sku_name" {
  description = "API Management SKU"
  type        = string
  default     = "Developer_1" # Developer for dev, Standard/Premium for prod
}

variable "apim_publisher_name" {
  description = "API Management publisher name"
  type        = string
  default     = "Kafka DotNet Team"
}

variable "apim_publisher_email" {
  description = "API Management publisher email"
  type        = string
}

# ACR Variables
variable "acr_sku" {
  description = "Azure Container Registry SKU"
  type        = string
  default     = "Basic"
}

# Key Vault Variables
variable "key_vault_sku_name" {
  description = "Key Vault SKU"
  type        = string
  default     = "standard"
}

# Monitoring Variables
variable "log_analytics_retention_days" {
  description = "Log Analytics retention in days"
  type        = number
  default     = 30
}

variable "application_insights_retention_days" {
  description = "Application Insights retention in days"
  type        = number
  default     = 90
}

# Entra External ID Variables
variable "entra_external_id_domain" {
  description = "Entra External ID domain (e.g., yourtenant.onmicrosoft.com)"
  type        = string
}

variable "entra_client_id" {
  description = "Entra External ID client ID for User Service"
  type        = string
  sensitive   = true
}

variable "entra_client_secret" {
  description = "Entra External ID client secret"
  type        = string
  sensitive   = true
}
