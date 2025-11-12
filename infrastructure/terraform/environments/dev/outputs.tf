# Development Environment - Outputs

output "resource_group_name" {
  description = "Resource group name"
  value       = module.resource_group.name
}

output "resource_group_location" {
  description = "Resource group location"
  value       = module.resource_group.location
}

output "acr_login_server" {
  description = "Azure Container Registry login server"
  value       = module.acr.login_server
}

output "acr_name" {
  description = "Azure Container Registry name"
  value       = module.acr.name
}

output "key_vault_uri" {
  description = "Key Vault URI"
  value       = module.key_vault.vault_uri
}

output "log_analytics_workspace_id" {
  description = "Log Analytics Workspace ID"
  value       = module.monitoring.log_analytics_workspace_id
}

output "application_insights_connection_string" {
  description = "Application Insights connection string"
  value       = module.monitoring.application_insights_connection_string
  sensitive   = true
}

output "postgresql_fqdn" {
  description = "PostgreSQL FQDN"
  value       = module.postgresql.fqdn
}

output "redis_hostname" {
  description = "Redis hostname"
  value       = module.redis.hostname
}

output "container_apps_environment_name" {
  description = "Container Apps environment name"
  value       = module.container_apps.environment_name
}

output "user_service_url" {
  description = "User Service URL"
  value       = module.container_apps.user_service_url
}

output "apim_gateway_url" {
  description = "API Management gateway URL"
  value       = module.apim.gateway_url
}

output "apim_management_url" {
  description = "API Management portal URL"
  value       = module.apim.management_url
}
