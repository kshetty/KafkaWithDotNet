# Azure AD B2C Terraform Module

variable "b2c_domain_name" {
  description = "The domain name for the Azure AD B2C tenant (e.g., 'userservice')"
  type        = string
}

variable "b2c_display_name" {
  description = "The display name for the Azure AD B2C tenant"
  type        = string
  default     = "UserService B2C"
}

variable "country_code" {
  description = "The country code for data residency (e.g., 'US', 'EU')"
  type        = string
  default     = "US"
}

variable "data_residency_location" {
  description = "The data residency location (e.g., 'United States', 'Europe')"
  type        = string
  default     = "United States"
}

variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
}

variable "sku_name" {
  description = "The SKU name for Azure AD B2C (PremiumP1 or PremiumP2)"
  type        = string
  default     = "PremiumP1"
}

variable "spa_redirect_uris" {
  description = "List of redirect URIs for the SPA application"
  type        = list(string)
  default = [
    "http://localhost:3000",
    "http://localhost:5173"
  ]
}

variable "api_identifier_uri" {
  description = "The identifier URI for the API"
  type        = string
}

variable "environment" {
  description = "The environment name (dev, staging, prod)"
  type        = string
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
