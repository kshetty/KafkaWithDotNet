output "apim_id" {
  description = "The ID of the API Management instance"
  value       = azurerm_api_management.main.id
}

output "apim_name" {
  description = "The name of the API Management instance"
  value       = azurerm_api_management.main.name
}

output "gateway_url" {
  description = "The URL of the API Management gateway"
  value       = azurerm_api_management.main.gateway_url
}

output "gateway_regional_url" {
  description = "The regional URL of the API Management gateway"
  value       = azurerm_api_management.main.gateway_regional_url
}

output "developer_portal_url" {
  description = "The URL of the developer portal"
  value       = "https://${azurerm_api_management.main.name}.developer.azure-api.net"
}

output "management_api_url" {
  description = "The URL of the management API"
  value       = azurerm_api_management.main.management_api_url
}

output "portal_url" {
  description = "The URL of the publisher portal"
  value       = azurerm_api_management.main.portal_url
}

output "scm_url" {
  description = "The URL of the SCM endpoint"
  value       = azurerm_api_management.main.scm_url
}

output "public_ip_addresses" {
  description = "The public IP addresses of the API Management instance"
  value       = azurerm_api_management.main.public_ip_addresses
}

output "private_ip_addresses" {
  description = "The private IP addresses of the API Management instance"
  value       = azurerm_api_management.main.private_ip_addresses
}

output "principal_id" {
  description = "The Principal ID of the System Assigned Managed Identity"
  value       = azurerm_api_management.main.identity[0].principal_id
}

output "tenant_id" {
  description = "The Tenant ID of the System Assigned Managed Identity"
  value       = azurerm_api_management.main.identity[0].tenant_id
}

output "application_insights_id" {
  description = "The ID of the Application Insights instance"
  value       = azurerm_application_insights.apim.id
}

output "application_insights_instrumentation_key" {
  description = "The instrumentation key of Application Insights"
  value       = azurerm_application_insights.apim.instrumentation_key
  sensitive   = true
}

output "application_insights_connection_string" {
  description = "The connection string of Application Insights"
  value       = azurerm_application_insights.apim.connection_string
  sensitive   = true
}

output "userservice_api_id" {
  description = "The ID of the UserService API"
  value       = azurerm_api_management_api.userservice.id
}

output "userservice_api_name" {
  description = "The name of the UserService API"
  value       = azurerm_api_management_api.userservice.name
}

output "userservice_api_path" {
  description = "The path of the UserService API"
  value       = azurerm_api_management_api.userservice.path
}

output "starter_product_id" {
  description = "The ID of the Starter product"
  value       = azurerm_api_management_product.starter.id
}

output "unlimited_product_id" {
  description = "The ID of the Unlimited product"
  value       = azurerm_api_management_product.unlimited.id
}

output "dev_subscription_primary_key" {
  description = "Primary subscription key for development"
  value       = var.environment == "dev" ? azurerm_api_management_subscription.dev[0].primary_key : null
  sensitive   = true
}

output "dev_subscription_secondary_key" {
  description = "Secondary subscription key for development"
  value       = var.environment == "dev" ? azurerm_api_management_subscription.dev[0].secondary_key : null
  sensitive   = true
}

output "backend_id" {
  description = "The ID of the UserService backend"
  value       = azurerm_api_management_backend.userservice.id
}

output "api_urls" {
  description = "Complete API URLs"
  value = {
    base_url = "${azurerm_api_management.main.gateway_url}/${azurerm_api_management_api.userservice.path}"
    health   = "${azurerm_api_management.main.gateway_url}/${azurerm_api_management_api.userservice.path}/api/v1/health"
    users    = "${azurerm_api_management.main.gateway_url}/${azurerm_api_management_api.userservice.path}/api/v1/users"
    swagger  = "${azurerm_api_management.main.gateway_url}/${azurerm_api_management_api.userservice.path}/swagger"
  }
}

output "configuration_summary" {
  description = "APIM configuration summary for frontend"
  value = {
    gateway_url             = azurerm_api_management.main.gateway_url
    userservice_base_path   = azurerm_api_management_api.userservice.path
    subscription_key_header = "Ocp-Apim-Subscription-Key"
    developer_portal        = "https://${azurerm_api_management.main.name}.developer.azure-api.net"
    products = {
      starter = {
        id         = azurerm_api_management_product.starter.product_id
        rate_limit = "${var.starter_rate_limit_calls} calls/min"
        quota      = "${var.starter_quota_calls} calls/day"
      }
      unlimited = {
        id         = azurerm_api_management_product.unlimited.product_id
        rate_limit = "${var.unlimited_rate_limit_calls} calls/min"
        quota      = "${var.unlimited_quota_calls} calls/day"
      }
    }
  }
}

output "curl_test_command" {
  description = "Example curl command to test the API"
  value       = var.environment == "dev" ? "curl -X GET '${azurerm_api_management.main.gateway_url}/${azurerm_api_management_api.userservice.path}/api/v1/health' -H 'Ocp-Apim-Subscription-Key: ${azurerm_api_management_subscription.dev[0].primary_key}'" : "Subscription key not available for non-dev environments"
  sensitive   = true
}
