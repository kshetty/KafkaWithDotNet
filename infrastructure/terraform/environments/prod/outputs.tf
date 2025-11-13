# Production Environment - Outputs

output "resource_group_name" {
  description = "Resource group name"
  value       = azurerm_resource_group.main.name
}

output "resource_group_location" {
  description = "Resource group location"
  value       = azurerm_resource_group.main.location
}

# TODO: Add outputs for other modules when implemented
# - ACR (login_server, name)
# - Key Vault (vault_uri)
# - Monitoring (log_analytics_workspace_id, application_insights_connection_string)
# - PostgreSQL (fqdn)
# - Redis (hostname)
# - Container Apps (environment_name, user_service_url)

# APIM outputs are defined in apim.tf
