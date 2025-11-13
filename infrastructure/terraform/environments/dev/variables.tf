# Development Environment - Variables
# Imports shared variables from ../../shared/variables.tf

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
  description = "Project name"
  type        = string
}

variable "environment" {
  description = "Environment name"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
}

variable "tags" {
  description = "Resource tags"
  type        = map(string)
}

variable "vnet_address_space" {
  description = "Virtual network address space"
  type        = list(string)
}

variable "postgresql_version" {
  description = "PostgreSQL version"
  type        = string
}

variable "postgresql_sku_name" {
  description = "PostgreSQL SKU"
  type        = string
}

variable "postgresql_storage_mb" {
  description = "PostgreSQL storage in MB"
  type        = number
}

variable "redis_sku_name" {
  description = "Redis SKU"
  type        = string
}

variable "redis_family" {
  description = "Redis family"
  type        = string
}

variable "redis_capacity" {
  description = "Redis capacity"
  type        = number
}

variable "redis_version" {
  description = "Redis version"
  type        = string
}

variable "container_apps_workload_profile" {
  description = "Container Apps workload profile"
  type        = string
}

variable "container_apps_min_replicas" {
  description = "Minimum replicas"
  type        = number
}

variable "container_apps_max_replicas" {
  description = "Maximum replicas"
  type        = number
}

variable "apim_sku_name" {
  description = "APIM SKU"
  type        = string
}

variable "apim_publisher_name" {
  description = "APIM publisher name"
  type        = string
}

variable "apim_publisher_email" {
  description = "APIM publisher email"
  type        = string
}

variable "acr_sku" {
  description = "ACR SKU"
  type        = string
}

variable "key_vault_sku_name" {
  description = "Key Vault SKU"
  type        = string
}

variable "log_analytics_retention_days" {
  description = "Log Analytics retention days"
  type        = number
}

variable "application_insights_retention_days" {
  description = "Application Insights retention days"
  type        = number
}

variable "entra_external_id_domain" {
  description = "Entra External ID domain"
  type        = string
}

variable "entra_client_id" {
  description = "Entra client ID"
  type        = string
  sensitive   = true
}

variable "entra_client_secret" {
  description = "Entra client secret"
  type        = string
  sensitive   = true
}

# Azure AD B2C Configuration
variable "b2c_tenant_id" {
  description = "Azure AD B2C Tenant ID"
  type        = string
  sensitive   = true
}

variable "b2c_domain" {
  description = "Azure AD B2C domain (e.g., userservice.b2clogin.com)"
  type        = string
}

variable "b2c_api_client_id" {
  description = "Azure AD B2C API Client ID"
  type        = string
  sensitive   = true
}
