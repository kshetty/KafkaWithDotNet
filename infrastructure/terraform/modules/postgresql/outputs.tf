output "id" {
  description = "PostgreSQL server ID"
  value       = azurerm_postgresql_flexible_server.main.id
}

output "fqdn" {
  description = "PostgreSQL server FQDN"
  value       = azurerm_postgresql_flexible_server.main.fqdn
}

output "name" {
  description = "PostgreSQL server name"
  value       = azurerm_postgresql_flexible_server.main.name
}

output "admin_login" {
  description = "Administrator login"
  value       = azurerm_postgresql_flexible_server.main.administrator_login
}

output "connection_string" {
  description = "Connection string"
  value       = "Host=${azurerm_postgresql_flexible_server.main.fqdn};Port=5432;Database=userservice;Username=psqladmin;Password=${var.admin_password};SSL Mode=Require;"
  sensitive   = true
}

output "database_name" {
  description = "Database name"
  value       = azurerm_postgresql_flexible_server_database.userservice.name
}
