# Shared Outputs

output "common_tags" {
  description = "Common tags applied to all resources"
  value       = local.common_tags
}

output "resource_naming_prefix" {
  description = "Resource naming prefix"
  value       = local.resource_prefix
}

output "location" {
  description = "Azure region"
  value       = var.location
}

output "environment" {
  description = "Environment name"
  value       = var.environment
}
