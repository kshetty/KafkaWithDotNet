terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }
}

# API Management Instance
resource "azurerm_api_management" "main" {
  name                = var.apim_name
  location            = var.location
  resource_group_name = var.resource_group_name
  publisher_name      = var.publisher_name
  publisher_email     = var.publisher_email
  sku_name            = var.sku_name

  identity {
    type = "SystemAssigned"
  }

  # VNet configuration (Premium tier only)
  dynamic "virtual_network_configuration" {
    for_each = var.virtual_network_type != "None" && var.subnet_id != null ? [1] : []
    content {
      subnet_id = var.subnet_id
    }
  }

  virtual_network_type = var.virtual_network_type

  # Security
  min_api_version = var.min_api_version

  # Developer portal
  sign_in {
    enabled = var.enable_sign_in
  }

  sign_up {
    enabled = var.enable_sign_up

    terms_of_service {
      consent_required = var.terms_of_service.consent_required
      enabled          = var.terms_of_service.enabled
      text             = var.terms_of_service.text
    }
  }

  # Notification sender email
  notification_sender_email = var.notification_sender_email

  # Availability zones (Premium tier only)
  zones = length(var.zones) > 0 ? var.zones : null

  tags = merge(
    var.tags,
    {
      Environment = var.environment
      ManagedBy   = "Terraform"
    }
  )

  lifecycle {
    ignore_changes = [
      # These can be modified in portal and shouldn't trigger redeployment
      tags["LastModified"],
      notification_sender_email
    ]
  }
}

# Application Insights for APIM Monitoring
resource "azurerm_application_insights" "apim" {
  name                = "${var.apim_name}-appinsights"
  location            = var.location
  resource_group_name = var.resource_group_name
  application_type    = "web"
  retention_in_days   = 90

  tags = merge(
    var.tags,
    {
      Environment = var.environment
      Purpose     = "APIM Monitoring"
    }
  )
}

# Logger for Application Insights
resource "azurerm_api_management_logger" "appinsights" {
  name                = "appinsights-logger"
  api_management_name = azurerm_api_management.main.name
  resource_group_name = var.resource_group_name
  resource_id         = azurerm_application_insights.apim.id

  application_insights {
    instrumentation_key = azurerm_application_insights.apim.instrumentation_key
  }
}

# Custom Domain (Optional)
resource "azurerm_api_management_custom_domain" "main" {
  count             = var.custom_domain != null ? 1 : 0
  api_management_id = azurerm_api_management.main.id

  gateway {
    host_name                    = var.custom_domain
    certificate                  = var.certificate_base64
    certificate_password         = var.certificate_password
    negotiate_client_certificate = false
  }
}

# Named Values (Configuration)
resource "azurerm_api_management_named_value" "backend_url" {
  name                = "backend-url"
  resource_group_name = var.resource_group_name
  api_management_name = azurerm_api_management.main.name
  display_name        = "backend-url"
  value               = var.backend_url
}

resource "azurerm_api_management_named_value" "b2c_tenant_id" {
  name                = "b2c-tenant-id"
  resource_group_name = var.resource_group_name
  api_management_name = azurerm_api_management.main.name
  display_name        = "b2c-tenant-id"
  value               = var.b2c_tenant_id
  secret              = true
}

resource "azurerm_api_management_named_value" "b2c_domain" {
  name                = "b2c-domain"
  resource_group_name = var.resource_group_name
  api_management_name = azurerm_api_management.main.name
  display_name        = "b2c-domain"
  value               = var.b2c_domain
}

resource "azurerm_api_management_named_value" "b2c_client_id" {
  name                = "b2c-client-id"
  resource_group_name = var.resource_group_name
  api_management_name = azurerm_api_management.main.name
  display_name        = "b2c-client-id"
  value               = var.b2c_client_id
  secret              = true
}

resource "azurerm_api_management_named_value" "b2c_policy_name" {
  name                = "b2c-policy-name"
  resource_group_name = var.resource_group_name
  api_management_name = azurerm_api_management.main.name
  display_name        = "b2c-policy-name"
  value               = var.b2c_policy_name
}

resource "azurerm_api_management_named_value" "cache_duration" {
  name                = "cache-duration"
  resource_group_name = var.resource_group_name
  api_management_name = azurerm_api_management.main.name
  display_name        = "cache-duration"
  value               = tostring(var.cache_duration_seconds)
}

# Backend Definition
resource "azurerm_api_management_backend" "userservice" {
  name                = "userservice-backend"
  resource_group_name = var.resource_group_name
  api_management_name = azurerm_api_management.main.name
  protocol            = "http"
  url                 = var.backend_url
  description         = "UserService API Backend"

  tls {
    validate_certificate_chain = true
    validate_certificate_name  = true
  }
}

# Diagnostic Settings (if Log Analytics provided)
resource "azurerm_monitor_diagnostic_setting" "apim" {
  count                      = var.log_analytics_workspace_id != null ? 1 : 0
  name                       = "apim-diagnostics"
  target_resource_id         = azurerm_api_management.main.id
  log_analytics_workspace_id = var.log_analytics_workspace_id

  enabled_log {
    category = "GatewayLogs"
  }

  enabled_log {
    category = "WebSocketConnectionLogs"
  }

  enabled_log {
    category = "AllMetrics"
  }
}

# Global Policy
resource "azurerm_api_management_policy" "global" {
  api_management_id = azurerm_api_management.main.id

  xml_content = templatefile("${path.module}/policies/global-policy.xml", {
    allowed_origins = join(",", [for origin in var.allowed_origins : format("<origin>%s</origin>", origin)])
  })

  depends_on = [
    azurerm_api_management_logger.appinsights
  ]
}
