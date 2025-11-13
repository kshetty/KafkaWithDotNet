terraform {
  required_version = ">= 1.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }
}

# Note: Azure AD B2C Directory creation via Terraform is limited
# Some configurations must be done via Azure Portal or Microsoft Graph API
# This configuration focuses on app registrations and service principals

# Random UUIDs for OAuth scopes
resource "random_uuid" "user_read_scope" {}
resource "random_uuid" "user_write_scope" {}
resource "random_uuid" "user_delete_scope" {}

# API Application Registration
resource "azuread_application" "api" {
  display_name = "UserService API - ${var.environment}"

  identifier_uris = [var.api_identifier_uri]

  sign_in_audience = "AzureADandPersonalMicrosoftAccount"

  api {
    # Define OAuth 2.0 permission scopes
    oauth2_permission_scope {
      admin_consent_description  = "Allow the application to read user profile"
      admin_consent_display_name = "Read user profile"
      enabled                    = true
      id                         = random_uuid.user_read_scope.result
      type                       = "User"
      user_consent_description   = "Allow the application to read your profile"
      user_consent_display_name  = "Read your profile"
      value                      = "user.read"
    }

    oauth2_permission_scope {
      admin_consent_description  = "Allow the application to write user profile"
      admin_consent_display_name = "Write user profile"
      enabled                    = true
      id                         = random_uuid.user_write_scope.result
      type                       = "User"
      user_consent_description   = "Allow the application to update your profile"
      user_consent_display_name  = "Update your profile"
      value                      = "user.write"
    }

    oauth2_permission_scope {
      admin_consent_description  = "Allow the application to delete user profile"
      admin_consent_display_name = "Delete user profile"
      enabled                    = true
      id                         = random_uuid.user_delete_scope.result
      type                       = "Admin"
      user_consent_description   = "Allow the application to delete your profile"
      user_consent_display_name  = "Delete your profile"
      value                      = "user.delete"
    }
  }

  web {
    implicit_grant {
      access_token_issuance_enabled = false
      id_token_issuance_enabled     = false
    }
  }

  tags = concat(
    [
      "Environment:${var.environment}",
      "ManagedBy:Terraform",
      "Application:UserService",
      "Component:API"
    ],
    [for key, value in var.tags : "${key}:${value}"]
  )
}

# Service Principal for API
resource "azuread_service_principal" "api" {
  client_id                    = azuread_application.api.client_id
  app_role_assignment_required = false

  tags = concat(
    [
      "Environment:${var.environment}",
      "ManagedBy:Terraform",
      "Application:UserService",
      "Component:API"
    ],
    [for key, value in var.tags : "${key}:${value}"]
  )
}

# SPA Application Registration
resource "azuread_application" "spa" {
  display_name = "UserService SPA - ${var.environment}"

  sign_in_audience = "AzureADandPersonalMicrosoftAccount"

  single_page_application {
    redirect_uris = var.spa_redirect_uris
  }

  required_resource_access {
    resource_app_id = azuread_application.api.application_id

    # user.read scope
    resource_access {
      id   = random_uuid.user_read_scope.result
      type = "Scope"
    }

    # user.write scope
    resource_access {
      id   = random_uuid.user_write_scope.result
      type = "Scope"
    }
  }

  # Microsoft Graph permissions
  required_resource_access {
    resource_app_id = "00000003-0000-0000-c000-000000000000" # Microsoft Graph

    # User.Read - delegated
    resource_access {
      id   = "e1fe6dd8-ba31-4d61-89e7-88639da4683d"
      type = "Scope"
    }

    # openid - delegated
    resource_access {
      id   = "37f7f235-527c-4136-accd-4a02d197296e"
      type = "Scope"
    }

    # profile - delegated
    resource_access {
      id   = "14dad69e-099b-42c9-810b-d002981feec1"
      type = "Scope"
    }

    # email - delegated
    resource_access {
      id   = "64a6cdd6-aab1-4aaf-94b8-3cc8405e90d0"
      type = "Scope"
    }
  }

  web {
    implicit_grant {
      access_token_issuance_enabled = true
      id_token_issuance_enabled     = true
    }
  }

  tags = concat(
    [
      "Environment:${var.environment}",
      "ManagedBy:Terraform",
      "Application:UserService",
      "Component:SPA"
    ],
    [for key, value in var.tags : "${key}:${value}"]
  )
}

# Service Principal for SPA
resource "azuread_service_principal" "spa" {
  client_id                    = azuread_application.spa.client_id
  app_role_assignment_required = false

  tags = concat(
    [
      "Environment:${var.environment}",
      "ManagedBy:Terraform",
      "Application:UserService",
      "Component:SPA"
    ],
    [for key, value in var.tags : "${key}:${value}"]
  )
}

# Grant admin consent for SPA to access API (optional, can be done manually)
# Note: This may require Azure AD administrator privileges
resource "azuread_service_principal_delegated_permission_grant" "spa_to_api" {
  service_principal_object_id          = azuread_service_principal.spa.object_id
  resource_service_principal_object_id = azuread_service_principal.api.object_id
  claim_values                         = ["user.read", "user.write"]
}

# Output for application configuration
locals {
  api_scopes = [
    "${var.api_identifier_uri}/user.read",
    "${var.api_identifier_uri}/user.write",
    "${var.api_identifier_uri}/user.delete"
  ]
}
