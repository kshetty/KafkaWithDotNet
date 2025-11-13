# Azure API Management for UserService - Development Environment

module "apim" {
  source = "../../modules/apim"

  # Basic Configuration
  apim_name           = "apim-userservice-dev"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name

  # Publisher Information
  publisher_name  = "UserService Development"
  publisher_email = "admin@userservice.dev"

  # SKU Configuration (Developer tier for dev environment)
  sku_name = "Developer_1"

  # Backend Configuration
  backend_url = "https://userservice-dev.azurewebsites.net" # Update with actual backend URL

  # Azure AD B2C Configuration (from Task 9)
  b2c_tenant_id   = var.b2c_tenant_id
  b2c_domain      = var.b2c_domain
  b2c_client_id   = var.b2c_api_client_id
  b2c_policy_name = "B2C_1_signup_signin"

  # CORS Configuration
  allowed_origins = [
    "http://localhost:3000",
    "http://localhost:5173",
    "https://userservice-spa-dev.azurewebsites.net"
  ]

  # Rate Limiting Configuration
  starter_rate_limit_calls   = 50
  starter_quota_calls        = 5000
  unlimited_rate_limit_calls = 1000
  unlimited_quota_calls      = 1000000

  # Cache Configuration
  cache_duration_seconds  = 300 # 5 minutes
  backend_request_timeout = 30  # 30 seconds

  # Developer Portal
  enable_developer_portal = true
  enable_sign_in          = true
  enable_sign_up          = true

  # Networking
  virtual_network_type = "None" # Use "External" or "Internal" for VNet integration

  # Environment
  environment = "dev"

  # Tags
  tags = {
    Environment = "dev"
    Application = "UserService"
    ManagedBy   = "Terraform"
    CostCenter  = "Engineering"
    Owner       = "Platform Team"
  }
}

# Outputs
output "apim_gateway_url" {
  description = "APIM Gateway URL"
  value       = module.apim.gateway_url
}

output "apim_developer_portal_url" {
  description = "APIM Developer Portal URL"
  value       = module.apim.developer_portal_url
}

output "apim_management_url" {
  description = "APIM Management API URL"
  value       = module.apim.management_api_url
}

output "apim_principal_id" {
  description = "APIM Managed Identity Principal ID"
  value       = module.apim.principal_id
}

output "userservice_api_base_url" {
  description = "UserService API Base URL through APIM"
  value       = "${module.apim.gateway_url}/${module.apim.userservice_api_path}"
}

output "apim_dev_subscription_key" {
  description = "Development subscription key (Primary)"
  value       = module.apim.dev_subscription_primary_key
  sensitive   = true
}

output "apim_test_command" {
  description = "Curl command to test APIM"
  value       = module.apim.curl_test_command
  sensitive   = true
}

output "apim_configuration" {
  description = "APIM configuration for frontend"
  value       = module.apim.configuration_summary
}

output "apim_api_urls" {
  description = "Complete API URLs"
  value       = module.apim.api_urls
}

output "application_insights_connection_string" {
  description = "Application Insights connection string for APIM"
  value       = module.apim.application_insights_connection_string
  sensitive   = true
}
