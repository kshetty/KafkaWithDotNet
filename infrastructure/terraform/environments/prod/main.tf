# Production Environment - Main Configuration
# Note: Some modules (ACR, Key Vault, Monitoring, PostgreSQL, Redis, Container Apps) are referenced
# but need to be implemented. Currently only APIM module is fully functional.
# This file is a placeholder for future complete infrastructure setup.

terraform {
  required_version = ">= 1.9.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.14.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.0.2"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6.0"
    }
  }

  # Backend for Production environment state
  # backend "azurerm" {
  #   resource_group_name  = "terraform-state-rg"
  #   storage_account_name = "tfstatekafkadotnet"
  #   container_name       = "tfstate"
  #   key                  = "prod/kafka-dotnet.tfstate"
  # }
}

# Providers
provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = true
    }

    key_vault {
      purge_soft_delete_on_destroy    = false
      recover_soft_deleted_key_vaults = true
    }

    api_management {
      purge_soft_delete_on_destroy = false
      recover_soft_deleted         = true
    }
  }

  subscription_id = var.subscription_id
}

provider "azuread" {
  tenant_id = var.tenant_id
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = "${var.project_name}-${var.environment}-rg"
  location = var.location
  tags     = var.tags
}

# Random Passwords (for future use when other modules are implemented)
resource "random_password" "postgresql_password" {
  length  = 32
  special = true
}

resource "random_password" "redis_password" {
  length  = 32
  special = true
}

resource "random_password" "jwt_secret" {
  length  = 64
  special = false
}

# TODO: Add other infrastructure modules (ACR, Key Vault, Monitoring, PostgreSQL, Redis, Container Apps)
# when they are fully implemented. Currently only APIM module is available - see apim.tf and b2c.tf
