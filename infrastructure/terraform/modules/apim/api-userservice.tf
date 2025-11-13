# UserService API
resource "azurerm_api_management_api" "userservice" {
  name                  = "userservice-api"
  resource_group_name   = var.resource_group_name
  api_management_name   = azurerm_api_management.main.name
  revision              = var.api_revision
  display_name          = "UserService API"
  path                  = "userservice"
  protocols             = ["https"]
  subscription_required = true
  service_url           = var.backend_url

  description = "User management API with CRUD operations"

  # API version set
  version        = "v1"
  version_set_id = azurerm_api_management_api_version_set.userservice.id

  # Import from OpenAPI (Swagger) spec
  # Note: This will be applied on first deployment. Subsequent updates should be done via CI/CD
  import {
    content_format = "openapi+json-link"
    content_value  = "${var.backend_url}/swagger/v1/swagger.json"
  }

  depends_on = [
    azurerm_api_management_backend.userservice
  ]
}

# API Version Set
resource "azurerm_api_management_api_version_set" "userservice" {
  name                = "userservice-version-set"
  resource_group_name = var.resource_group_name
  api_management_name = azurerm_api_management.main.name
  display_name        = "UserService API"
  versioning_scheme   = "Segment"
}

# API Policy (JWT Validation, Rate Limiting, Caching)
resource "azurerm_api_management_api_policy" "userservice" {
  api_name            = azurerm_api_management_api.userservice.name
  api_management_name = azurerm_api_management.main.name
  resource_group_name = var.resource_group_name

  xml_content = templatefile("${path.module}/policies/userservice-api-policy.xml", {
    b2c_tenant_id           = var.b2c_tenant_id
    b2c_domain              = var.b2c_domain
    b2c_client_id           = var.b2c_client_id
    b2c_policy_name         = var.b2c_policy_name
    backend_url             = var.backend_url
    cache_duration          = var.cache_duration_seconds
    backend_request_timeout = var.backend_request_timeout
  })
}

# Products
resource "azurerm_api_management_product" "starter" {
  product_id            = "starter"
  api_management_name   = azurerm_api_management.main.name
  resource_group_name   = var.resource_group_name
  display_name          = "Starter"
  description           = "Starter tier with rate limits (${var.starter_rate_limit_calls} calls/min, ${var.starter_quota_calls} calls/day)"
  subscription_required = true
  approval_required     = false
  published             = true
  subscriptions_limit   = 10
}

resource "azurerm_api_management_product" "unlimited" {
  product_id            = "unlimited"
  api_management_name   = azurerm_api_management.main.name
  resource_group_name   = var.resource_group_name
  display_name          = "Unlimited"
  description           = "Unlimited tier for premium users (${var.unlimited_rate_limit_calls} calls/min, ${var.unlimited_quota_calls} calls/day)"
  subscription_required = true
  approval_required     = true
  published             = true
}

# Associate API with Products
resource "azurerm_api_management_product_api" "starter_userservice" {
  api_name            = azurerm_api_management_api.userservice.name
  product_id          = azurerm_api_management_product.starter.product_id
  api_management_name = azurerm_api_management.main.name
  resource_group_name = var.resource_group_name
}

resource "azurerm_api_management_product_api" "unlimited_userservice" {
  api_name            = azurerm_api_management_api.userservice.name
  product_id          = azurerm_api_management_product.unlimited.product_id
  api_management_name = azurerm_api_management.main.name
  resource_group_name = var.resource_group_name
}

# Product Policies (Rate Limiting per Product)
resource "azurerm_api_management_product_policy" "starter" {
  product_id          = azurerm_api_management_product.starter.product_id
  api_management_name = azurerm_api_management.main.name
  resource_group_name = var.resource_group_name

  xml_content = templatefile("${path.module}/policies/starter-product-policy.xml", {
    rate_limit_calls = var.starter_rate_limit_calls
    quota_calls      = var.starter_quota_calls
  })
}

resource "azurerm_api_management_product_policy" "unlimited" {
  product_id          = azurerm_api_management_product.unlimited.product_id
  api_management_name = azurerm_api_management.main.name
  resource_group_name = var.resource_group_name

  xml_content = templatefile("${path.module}/policies/unlimited-product-policy.xml", {
    rate_limit_calls = var.unlimited_rate_limit_calls
    quota_calls      = var.unlimited_quota_calls
  })
}

# Subscription for Development (Starter Product)
resource "azurerm_api_management_subscription" "dev" {
  count               = var.environment == "dev" ? 1 : 0
  api_management_name = azurerm_api_management.main.name
  resource_group_name = var.resource_group_name
  product_id          = azurerm_api_management_product.starter.id
  display_name        = "Development Subscription"
  state               = "active"
  allow_tracing       = true
}
