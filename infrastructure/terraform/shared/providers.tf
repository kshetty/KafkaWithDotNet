# Terraform Configuration for KafkaWithDotNet - Azure Deployment
# Backend configuration for remote state management

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

  # Backend configuration - uncomment and configure for remote state
  # backend "azurerm" {
  #   resource_group_name  = "terraform-state-rg"
  #   storage_account_name = "tfstate<unique-id>"
  #   container_name       = "tfstate"
  #   key                  = "kafka-dotnet.tfstate"
  # }
}

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
