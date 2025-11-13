output "api_application_id" {
  description = "The Application (client) ID of the API application"
  value       = azuread_application.api.client_id
  sensitive   = false
}

output "api_object_id" {
  description = "The Object ID of the API application"
  value       = azuread_application.api.object_id
  sensitive   = false
}

output "api_identifier_uri" {
  description = "The identifier URI of the API application"
  value       = var.api_identifier_uri
  sensitive   = false
}

output "api_scopes" {
  description = "List of API scopes that can be requested"
  value       = local.api_scopes
  sensitive   = false
}

output "spa_application_id" {
  description = "The Application (client) ID of the SPA application"
  value       = azuread_application.spa.client_id
  sensitive   = false
}

output "spa_object_id" {
  description = "The Object ID of the SPA application"
  value       = azuread_application.spa.object_id
  sensitive   = false
}

output "spa_redirect_uris" {
  description = "List of redirect URIs configured for the SPA"
  value       = var.spa_redirect_uris
  sensitive   = false
}

output "api_service_principal_id" {
  description = "The Object ID of the API service principal"
  value       = azuread_service_principal.api.object_id
  sensitive   = false
}

output "spa_service_principal_id" {
  description = "The Object ID of the SPA service principal"
  value       = azuread_service_principal.spa.object_id
  sensitive   = false
}

output "user_read_scope_id" {
  description = "The ID of the user.read OAuth scope"
  value       = random_uuid.user_read_scope.result
  sensitive   = false
}

output "user_write_scope_id" {
  description = "The ID of the user.write OAuth scope"
  value       = random_uuid.user_write_scope.result
  sensitive   = false
}

output "user_delete_scope_id" {
  description = "The ID of the user.delete OAuth scope"
  value       = random_uuid.user_delete_scope.result
  sensitive   = false
}

# Configuration output for easy copy-paste to appsettings.json
output "api_configuration" {
  description = "Configuration values for the API appsettings.json"
  value = {
    ClientId = azuread_application.api.client_id
    Scopes   = local.api_scopes
  }
  sensitive = false
}

# Configuration output for frontend .env file
output "spa_configuration" {
  description = "Configuration values for the SPA .env file"
  value = {
    ClientId     = azuread_application.spa.client_id
    Scopes       = join(" ", local.api_scopes)
    RedirectUris = var.spa_redirect_uris
  }
  sensitive = false
}
