# Azure AD B2C Configuration for Production Environment

module "b2c" {
  source = "../../modules/b2c"

  # B2C Tenant Configuration
  b2c_domain_name         = "userservice-prod"
  b2c_display_name        = "UserService B2C - Production"
  country_code            = "US"
  data_residency_location = "United States"
  resource_group_name     = azurerm_resource_group.main.name
  sku_name                = "PremiumP2" # Higher tier for production

  # Application Configuration
  api_identifier_uri = "https://userservice-prod.onmicrosoft.com/api"

  spa_redirect_uris = [
    "https://userservice.com",
    "https://www.userservice.com",
    "https://app.userservice.com"
  ]

  environment = var.environment

  tags = {
    Environment = var.environment
    ManagedBy   = "Terraform"
    Application = "UserService"
    CostCenter  = "Production"
    Compliance  = "GDPR"
  }
}

# Output B2C configuration for reference
output "b2c_api_client_id" {
  description = "Azure AD B2C API Application Client ID"
  value       = module.b2c.api_application_id
}

output "b2c_spa_client_id" {
  description = "Azure AD B2C SPA Application Client ID"
  value       = module.b2c.spa_application_id
}

output "b2c_api_scopes" {
  description = "Available API scopes"
  value       = module.b2c.api_scopes
}

output "b2c_configuration_summary" {
  description = "Summary of B2C configuration for easy reference"
  value = {
    api = module.b2c.api_configuration
    spa = module.b2c.spa_configuration
  }
}
