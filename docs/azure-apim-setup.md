# Azure API Management Setup Guide

## Overview

This guide provides comprehensive instructions for setting up Azure API Management (APIM) as the gateway for the UserService API. APIM acts as a secure, scalable, and feature-rich API gateway with capabilities including JWT validation, rate limiting, request transformation, caching, and detailed analytics.

## Table of Contents

1. [Why Azure API Management](#why-azure-api-management)
2. [Architecture](#architecture)
3. [APIM Tiers and Pricing](#apim-tiers-and-pricing)
4. [Prerequisites](#prerequisites)
5. [Infrastructure Setup](#infrastructure-setup)
6. [Policy Configuration](#policy-configuration)
7. [API Import and Configuration](#api-import-and-configuration)
8. [Security Configuration](#security-configuration)
9. [Monitoring and Analytics](#monitoring-and-analytics)
10. [Testing](#testing)
11. [Best Practices](#best-practices)
12. [Troubleshooting](#troubleshooting)

## Why Azure API Management

### Key Benefits

**Security**:

- JWT token validation before reaching backend
- OAuth 2.0 / OpenID Connect integration
- Subscription key management
- IP filtering and CORS policies
- Rate limiting and throttling
- DDoS protection

**Performance**:

- Response caching (reduce backend load)
- Content delivery network (CDN) integration
- Compression
- Request/response transformation
- Load balancing across backend instances

**Management**:

- Centralized API governance
- Version management
- Developer portal for API consumers
- API analytics and monitoring
- Request logging and tracing

**Scalability**:

- Auto-scaling capabilities
- Multi-region deployment
- High availability with 99.95% SLA (Premium tier)

### Use Cases

1. **API Gateway**: Single entry point for all client applications
2. **Security Layer**: Validate tokens, enforce rate limits, protect backends
3. **API Monetization**: Subscription-based access with usage quotas
4. **Legacy Modernization**: Expose legacy APIs with modern REST/GraphQL interfaces
5. **Multi-Cloud**: Integrate APIs across Azure, AWS, on-premises

## Architecture

### Request Flow

```
┌──────────────────┐
│   React SPA      │
│   (Frontend)     │
└────────┬─────────┘
         │ 1. API Request
         │    + Bearer Token
         │    + Subscription Key (optional)
         ▼
┌─────────────────────────────────────────────┐
│   Azure API Management                      │
│                                             │
│   ┌─────────────────────────────────────┐  │
│   │  Inbound Policies                   │  │
│   │  - CORS validation                  │  │
│   │  - JWT token validation             │  │
│   │  - Rate limiting (per user/IP)      │  │
│   │  - Request transformation           │  │
│   │  - IP filtering                     │  │
│   └──────────────┬──────────────────────┘  │
│                  │ 2. Validated Request     │
│                  ▼                          │
│   ┌─────────────────────────────────────┐  │
│   │  Backend APIs                       │  │
│   │  - UserService API                  │  │
│   │  - Future services                  │  │
│   └──────────────┬──────────────────────┘  │
│                  │ 3. Response              │
│                  ▼                          │
│   ┌─────────────────────────────────────┐  │
│   │  Outbound Policies                  │  │
│   │  - Response caching                 │  │
│   │  - Response transformation          │  │
│   │  - Header manipulation              │  │
│   └──────────────┬──────────────────────┘  │
│                  │                          │
└──────────────────┼──────────────────────────┘
                   │ 4. Final Response
                   ▼
         ┌──────────────────┐
         │   React SPA      │
         └──────────────────┘
```

### Component Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Azure APIM Instance                  │
│                                                         │
│  ┌──────────────────┐  ┌──────────────────┐           │
│  │  Gateway         │  │  Developer Portal │           │
│  │  - Public APIs   │  │  - Documentation  │           │
│  │  - Policies      │  │  - Try-it console │           │
│  └──────────────────┘  └──────────────────┘           │
│                                                         │
│  ┌──────────────────┐  ┌──────────────────┐           │
│  │  Products        │  │  Subscriptions    │           │
│  │  - Starter       │  │  - API keys       │           │
│  │  - Unlimited     │  │  - Quotas         │           │
│  └──────────────────┘  └──────────────────┘           │
│                                                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │  APIs                                            │  │
│  │  - UserService API v1                           │  │
│  │    • GET    /api/v1/users                       │  │
│  │    • GET    /api/v1/users/{id}                  │  │
│  │    • POST   /api/v1/users                       │  │
│  │    • PUT    /api/v1/users/{id}                  │  │
│  │    • DELETE /api/v1/users/{id}                  │  │
│  │    • GET    /api/v1/health                      │  │
│  └──────────────────────────────────────────────────┘  │
│                                                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Backends                                        │  │
│  │  - UserService: https://userservice.azurewebsites │  │
│  │  - Health endpoint: /health                      │  │
│  └──────────────────────────────────────────────────┘  │
│                                                         │
│  ┌──────────────────────────────────────────────────┐  │
│  │  Policies (XML-based)                            │  │
│  │  - Global policies                               │  │
│  │  - API-level policies                            │  │
│  │  - Operation-level policies                      │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

## APIM Tiers and Pricing

### Tier Comparison

| Feature                 | Developer   | Basic         | Standard      | Premium        |
| ----------------------- | ----------- | ------------- | ------------- | -------------- |
| **Monthly Cost**        | ~$50        | ~$150         | ~$700         | ~$3,000        |
| **SLA**                 | None        | 99.95%        | 99.95%        | 99.95%         |
| **Units**               | 1           | 2             | 4             | Unlimited      |
| **Max Throughput**      | 500 req/sec | 1,000 req/sec | 2,500 req/sec | 4,000+ req/sec |
| **Cache**               | 10 MB       | 50 MB         | 1 GB          | 5 GB per unit  |
| **Multi-region**        | ❌          | ❌            | ❌            | ✅             |
| **VNet Integration**    | ❌          | ❌            | ❌            | ✅             |
| **Developer Portal**    | ✅          | ✅            | ✅            | ✅             |
| **Custom Domains**      | ❌          | ❌            | ✅            | ✅             |
| **Client Certificates** | ❌          | ❌            | ✅            | ✅             |

### Recommendation for UserService

**Development/Testing**: Developer tier ($50/month)

- No SLA required for non-production
- Sufficient for development and testing
- 500 requests/second adequate for dev workloads

**Production**: Standard tier ($700/month)

- 99.95% SLA
- 2,500 requests/second
- 1 GB cache
- Custom domains support
- Suitable for most production workloads

**Enterprise**: Premium tier ($3,000+/month)

- Multi-region deployment
- VNet integration for security
- Unlimited scaling
- High-availability scenarios

## Prerequisites

### Azure Resources Required

1. **Azure Subscription**: Active subscription with contributor access
2. **Resource Group**: `rg-userservice-{environment}`
3. **Virtual Network** (Premium tier only): For VNet integration
4. **Application Insights**: For monitoring and analytics
5. **Azure AD B2C Tenant**: For JWT token validation
6. **UserService API**: Backend API deployed to Azure App Service

### Tools Required

- Azure CLI 2.50+
- Terraform 1.5+
- PowerShell 7+ or Bash
- OpenAPI/Swagger spec from UserService

### Configuration Information Needed

From Azure AD B2C (Task 9):

- Tenant ID
- B2C Domain (e.g., `userservice.b2clogin.com`)
- API Client ID
- Valid Issuer URL
- Audience (API Client ID)

From UserService:

- Backend URL (e.g., `https://userservice-dev.azurewebsites.net`)
- OpenAPI spec URL (e.g., `https://userservice-dev.azurewebsites.net/swagger/v1/swagger.json`)
- Health check endpoint (`/health`)

## Infrastructure Setup

### Terraform Configuration

#### Module: APIM Core

**File**: `infrastructure/terraform/modules/apim/main.tf`

```hcl
resource "azurerm_api_management" "main" {
  name                = var.apim_name
  location            = var.location
  resource_group_name = var.resource_group_name
  publisher_name      = var.publisher_name
  publisher_email     = var.publisher_email

  sku_name = var.sku_name # "Developer_1", "Standard_1", "Premium_1"

  identity {
    type = "SystemAssigned"
  }

  tags = var.tags
}

# Application Insights for monitoring
resource "azurerm_application_insights" "apim" {
  name                = "${var.apim_name}-appinsights"
  location            = var.location
  resource_group_name = var.resource_group_name
  application_type    = "web"

  tags = var.tags
}

# Connect APIM to Application Insights
resource "azurerm_api_management_logger" "appinsights" {
  name                = "appinsights-logger"
  api_management_name = azurerm_api_management.main.name
  resource_group_name = var.resource_group_name

  application_insights {
    instrumentation_key = azurerm_application_insights.apim.instrumentation_key
  }
}

# Custom domain (optional)
resource "azurerm_api_management_custom_domain" "main" {
  count               = var.custom_domain != null ? 1 : 0
  api_management_id   = azurerm_api_management.main.id

  gateway {
    host_name    = var.custom_domain
    certificate  = var.certificate_base64
    certificate_password = var.certificate_password
  }
}

# Named values (reusable config)
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

resource "azurerm_api_management_named_value" "b2c_client_id" {
  name                = "b2c-client-id"
  resource_group_name = var.resource_group_name
  api_management_name = azurerm_api_management.main.name
  display_name        = "b2c-client-id"
  value               = var.b2c_client_id
  secret              = true
}

# Backend definition
resource "azurerm_api_management_backend" "userservice" {
  name                = "userservice-backend"
  resource_group_name = var.resource_group_name
  api_management_name = azurerm_api_management.main.name
  protocol            = "http"
  url                 = var.backend_url

  description = "UserService API Backend"

  # Health check configuration
  tls {
    validate_certificate_chain = true
    validate_certificate_name  = true
  }
}

# Global policy (applies to all APIs)
resource "azurerm_api_management_policy" "global" {
  api_management_id = azurerm_api_management.main.id

  xml_content = templatefile("${path.module}/policies/global-policy.xml", {
    application_insights_key = azurerm_application_insights.apim.instrumentation_key
  })
}
```

#### Module: UserService API

**File**: `infrastructure/terraform/modules/apim/api-userservice.tf`

```hcl
# UserService API
resource "azurerm_api_management_api" "userservice" {
  name                = "userservice-api"
  resource_group_name = var.resource_group_name
  api_management_name = azurerm_api_management.main.name
  revision            = "1"
  display_name        = "UserService API"
  path                = "userservice"
  protocols           = ["https"]

  subscription_required = true

  service_url = var.backend_url

  # Import from OpenAPI spec
  import {
    content_format = "openapi+json-link"
    content_value  = "${var.backend_url}/swagger/v1/swagger.json"
  }
}

# API Policy (JWT validation, rate limiting)
resource "azurerm_api_management_api_policy" "userservice" {
  api_name            = azurerm_api_management_api.userservice.name
  api_management_name = azurerm_api_management.main.name
  resource_group_name = var.resource_group_name

  xml_content = templatefile("${path.module}/policies/userservice-api-policy.xml", {
    b2c_tenant_id  = var.b2c_tenant_id
    b2c_domain     = var.b2c_domain
    b2c_client_id  = var.b2c_client_id
    backend_url    = var.backend_url
  })
}

# Product: Starter (limited access)
resource "azurerm_api_management_product" "starter" {
  product_id            = "starter"
  api_management_name   = azurerm_api_management.main.name
  resource_group_name   = var.resource_group_name
  display_name          = "Starter"
  description           = "Starter tier with rate limits"
  subscription_required = true
  approval_required     = false
  published             = true

  subscriptions_limit = 10
}

# Product: Unlimited
resource "azurerm_api_management_product" "unlimited" {
  product_id            = "unlimited"
  api_management_name   = azurerm_api_management.main.name
  resource_group_name   = var.resource_group_name
  display_name          = "Unlimited"
  description           = "Unlimited access for premium users"
  subscription_required = true
  approval_required     = true
  published             = true
}

# Associate API with products
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

# Product policies (rate limiting)
resource "azurerm_api_management_product_policy" "starter" {
  product_id          = azurerm_api_management_product.starter.product_id
  api_management_name = azurerm_api_management.main.name
  resource_group_name = var.resource_group_name

  xml_content = templatefile("${path.module}/policies/starter-product-policy.xml", {})
}

resource "azurerm_api_management_product_policy" "unlimited" {
  product_id          = azurerm_api_management_product.unlimited.product_id
  api_management_name = azurerm_api_management.main.name
  resource_group_name = var.resource_group_name

  xml_content = templatefile("${path.module}/policies/unlimited-product-policy.xml", {})
}
```

### Azure CLI Setup

**Script**: `infrastructure/scripts/setup-apim.sh`

```bash
#!/bin/bash

# Configuration
RESOURCE_GROUP="rg-userservice-dev"
LOCATION="eastus"
APIM_NAME="apim-userservice-dev"
PUBLISHER_NAME="UserService"
PUBLISHER_EMAIL="admin@userservice.com"
SKU="Developer"
BACKEND_URL="https://userservice-dev.azurewebsites.net"

echo "Creating API Management instance..."
az apim create \
  --name "$APIM_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --publisher-name "$PUBLISHER_NAME" \
  --publisher-email "$PUBLISHER_EMAIL" \
  --sku-name "$SKU" \
  --enable-managed-identity true

echo "API Management provisioning started (takes 30-45 minutes)..."
echo "Instance: $APIM_NAME"
echo "Gateway URL: https://${APIM_NAME}.azure-api.net"
```

## Policy Configuration

### Global Policy

**File**: `infrastructure/terraform/modules/apim/policies/global-policy.xml`

```xml
<policies>
    <inbound>
        <!-- CORS policy -->
        <cors allow-credentials="true">
            <allowed-origins>
                <origin>http://localhost:3000</origin>
                <origin>http://localhost:5173</origin>
                <origin>https://userservice-spa-dev.azurewebsites.net</origin>
            </allowed-origins>
            <allowed-methods>
                <method>GET</method>
                <method>POST</method>
                <method>PUT</method>
                <method>DELETE</method>
                <method>OPTIONS</method>
            </allowed-methods>
            <allowed-headers>
                <header>*</header>
            </allowed-headers>
            <expose-headers>
                <header>*</header>
            </expose-headers>
        </cors>

        <!-- Remove backend URL from response headers -->
        <set-header name="X-Powered-By" exists-action="delete" />
        <set-header name="X-AspNet-Version" exists-action="delete" />

        <!-- Add request ID for tracing -->
        <set-variable name="requestId" value="@(Guid.NewGuid().ToString())" />
        <set-header name="X-Request-ID" exists-action="override">
            <value>@((string)context.Variables["requestId"])</value>
        </set-header>
    </inbound>

    <backend>
        <forward-request />
    </backend>

    <outbound>
        <!-- Add security headers -->
        <set-header name="X-Content-Type-Options" exists-action="override">
            <value>nosniff</value>
        </set-header>
        <set-header name="X-Frame-Options" exists-action="override">
            <value>DENY</value>
        </set-header>
        <set-header name="X-XSS-Protection" exists-action="override">
            <value>1; mode=block</value>
        </set-header>
        <set-header name="Strict-Transport-Security" exists-action="override">
            <value>max-age=31536000; includeSubDomains</value>
        </set-header>

        <!-- Add request ID to response -->
        <set-header name="X-Request-ID" exists-action="override">
            <value>@((string)context.Variables["requestId"])</value>
        </set-header>
    </outbound>

    <on-error>
        <!-- Log errors to Application Insights -->
        <trace source="global-policy-error" severity="error">
            <message>@(context.LastError.Message)</message>
            <metadata name="RequestId" value="@((string)context.Variables["requestId"])" />
            <metadata name="ErrorReason" value="@(context.LastError.Reason)" />
        </trace>
    </on-error>
</policies>
```

### UserService API Policy (JWT Validation)

**File**: `infrastructure/terraform/modules/apim/policies/userservice-api-policy.xml`

```xml
<policies>
    <inbound>
        <base />

        <!-- Validate Azure AD B2C JWT token -->
        <validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized. Valid JWT token is required.">
            <openid-config url="https://${b2c_domain}/${b2c_tenant_id}/v2.0/.well-known/openid-configuration?p=B2C_1_signup_signin" />
            <audiences>
                <audience>${b2c_client_id}</audience>
            </audiences>
            <issuers>
                <issuer>https://${b2c_domain}/${b2c_tenant_id}/v2.0/</issuer>
            </issuers>
            <required-claims>
                <claim name="oid" match="any">
                    <value>.*</value>
                </claim>
            </required-claims>
        </validate-jwt>

        <!-- Extract user information from JWT -->
        <set-variable name="userId" value="@{
            var jwt = context.Request.Headers.GetValueOrDefault("Authorization", "").Replace("Bearer ", "");
            if (string.IsNullOrEmpty(jwt)) return null;

            var base64 = jwt.Split('.')[1];
            var padded = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
            var bytes = Convert.FromBase64String(padded);
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            var token = Newtonsoft.Json.Linq.JObject.Parse(json);

            return token["oid"]?.ToString();
        }" />

        <!-- Forward user ID to backend -->
        <set-header name="X-User-ID" exists-action="override">
            <value>@((string)context.Variables["userId"])</value>
        </set-header>

        <!-- Check subscription key -->
        <check-header name="Ocp-Apim-Subscription-Key" failed-check-httpcode="401" failed-check-error-message="Subscription key is required" ignore-case="false" />

        <!-- Rate limiting per user -->
        <rate-limit-by-key calls="100" renewal-period="60" counter-key="@(context.Request.Headers.GetValueOrDefault("X-User-ID", "anonymous"))" />

        <!-- Quota per subscription -->
        <quota-by-key calls="10000" renewal-period="86400" counter-key="@(context.Subscription?.Key ?? "anonymous")" />

        <!-- Cache lookup (GET requests only) -->
        <cache-lookup vary-by-developer="false" vary-by-developer-groups="false" downstream-caching-type="none">
            <vary-by-header>Accept</vary-by-header>
            <vary-by-header>Accept-Charset</vary-by-header>
            <vary-by-query-parameter>*</vary-by-query-parameter>
        </cache-lookup>

        <!-- Set backend service URL -->
        <set-backend-service base-url="${backend_url}" />
    </inbound>

    <backend>
        <forward-request timeout="30" />
    </backend>

    <outbound>
        <base />

        <!-- Store response in cache (GET requests, 200 status) -->
        <cache-store duration="300" />

        <!-- Remove internal headers -->
        <set-header name="X-AspNetCore-Environment" exists-action="delete" />
        <set-header name="X-User-ID" exists-action="delete" />
    </outbound>

    <on-error>
        <base />

        <!-- Return custom error response -->
        <return-response>
            <set-status code="@(context.Response.StatusCode)" reason="@(context.Response.StatusReason)" />
            <set-header name="Content-Type" exists-action="override">
                <value>application/json</value>
            </set-header>
            <set-body>@{
                return new JObject(
                    new JProperty("error", new JObject(
                        new JProperty("code", context.Response.StatusCode),
                        new JProperty("message", context.LastError?.Message ?? "An error occurred"),
                        new JProperty("requestId", context.Variables["requestId"])
                    ))
                ).ToString();
            }</set-body>
        </return-response>
    </on-error>
</policies>
```

### Starter Product Policy (Rate Limiting)

**File**: `infrastructure/terraform/modules/apim/policies/starter-product-policy.xml`

```xml
<policies>
    <inbound>
        <base />

        <!-- Rate limiting: 50 calls per minute -->
        <rate-limit-by-key calls="50" renewal-period="60" counter-key="@(context.Subscription.Key)" />

        <!-- Daily quota: 5000 calls per day -->
        <quota-by-key calls="5000" renewal-period="86400" counter-key="@(context.Subscription.Key)" />
    </inbound>

    <backend>
        <base />
    </backend>

    <outbound>
        <base />

        <!-- Add rate limit headers -->
        <set-header name="X-Rate-Limit-Limit" exists-action="override">
            <value>50</value>
        </set-header>
        <set-header name="X-Rate-Limit-Remaining" exists-action="override">
            <value>@{
                var rateLimitRemaining = 50 - context.Variables.GetValueOrDefault<int>("rate-limit-counter", 0);
                return rateLimitRemaining.ToString();
            }</value>
        </set-header>
    </outbound>

    <on-error>
        <base />
    </on-error>
</policies>
```

### Unlimited Product Policy

**File**: `infrastructure/terraform/modules/apim/policies/unlimited-product-policy.xml`

```xml
<policies>
    <inbound>
        <base />

        <!-- Rate limiting: 1000 calls per minute -->
        <rate-limit-by-key calls="1000" renewal-period="60" counter-key="@(context.Subscription.Key)" />

        <!-- Daily quota: 1,000,000 calls per day -->
        <quota-by-key calls="1000000" renewal-period="86400" counter-key="@(context.Subscription.Key)" />
    </inbound>

    <backend>
        <base />
    </backend>

    <outbound>
        <base />

        <!-- Add rate limit headers -->
        <set-header name="X-Rate-Limit-Limit" exists-action="override">
            <value>1000</value>
        </set-header>
    </outbound>

    <on-error>
        <base />
    </on-error>
</policies>
```

## API Import and Configuration

### Import from OpenAPI Spec

```bash
# Import API from Swagger/OpenAPI spec
az apim api import \
  --resource-group rg-userservice-dev \
  --service-name apim-userservice-dev \
  --path userservice \
  --specification-url "https://userservice-dev.azurewebsites.net/swagger/v1/swagger.json" \
  --specification-format OpenApi \
  --api-id userservice-api \
  --display-name "UserService API" \
  --protocols https \
  --subscription-required true
```

### Manual Operation Configuration

If importing fails, configure operations manually:

```bash
# Create API
az apim api create \
  --resource-group rg-userservice-dev \
  --service-name apim-userservice-dev \
  --api-id userservice-api \
  --path userservice \
  --display-name "UserService API" \
  --service-url "https://userservice-dev.azurewebsites.net" \
  --protocols https

# Add GET /users operation
az apim api operation create \
  --resource-group rg-userservice-dev \
  --service-name apim-userservice-dev \
  --api-id userservice-api \
  --url-template "/api/v1/users" \
  --method GET \
  --display-name "Get All Users" \
  --description "Retrieve all users with pagination"

# Add GET /users/{id} operation
az apim api operation create \
  --resource-group rg-userservice-dev \
  --service-name apim-userservice-dev \
  --api-id userservice-api \
  --url-template "/api/v1/users/{id}" \
  --method GET \
  --display-name "Get User by ID" \
  --template-parameters name=id description="User ID" type="string" required=true
```

## Security Configuration

### Subscription Keys

Generate subscription keys for API access:

```bash
# Create subscription for Starter product
az apim product subscription create \
  --resource-group rg-userservice-dev \
  --service-name apim-userservice-dev \
  --product-id starter \
  --subscription-id dev-subscription \
  --subscription-name "Development Subscription" \
  --state active

# Get subscription key
az apim product subscription show \
  --resource-group rg-userservice-dev \
  --service-name apim-userservice-dev \
  --product-id starter \
  --subscription-id dev-subscription \
  --query "{PrimaryKey:primaryKey, SecondaryKey:secondaryKey}" \
  --output table
```

### IP Filtering

Restrict access by IP address:

```xml
<policies>
    <inbound>
        <ip-filter action="allow">
            <address>203.0.113.0/24</address>
            <address>198.51.100.42</address>
        </ip-filter>
    </inbound>
</policies>
```

### Client Certificates (Mutual TLS)

For Premium/Standard tiers:

```xml
<policies>
    <inbound>
        <choose>
            <when condition="@(context.Request.Certificate == null)">
                <return-response>
                    <set-status code="403" reason="Client certificate required" />
                </return-response>
            </when>
            <when condition="@(!context.Request.Certificate.Verify())">
                <return-response>
                    <set-status code="403" reason="Invalid client certificate" />
                </return-response>
            </when>
        </choose>
    </inbound>
</policies>
```

## Monitoring and Analytics

### Application Insights Integration

Key metrics to monitor:

1. **Request Metrics**:

   - Total requests
   - Failed requests (4xx, 5xx)
   - Average response time
   - P50, P95, P99 latency

2. **Availability**:

   - Uptime percentage
   - Health check success rate

3. **Performance**:
   - Backend response time
   - Cache hit ratio
   - Rate limit violations

### Custom Dashboards

```kusto
// Failed requests in last 24 hours
requests
| where timestamp > ago(24h)
| where success == false
| summarize count() by resultCode, operation_Name
| order by count_ desc

// Top 10 slowest operations
requests
| where timestamp > ago(1h)
| summarize avg(duration), percentile(duration, 95) by operation_Name
| order by avg_duration desc
| take 10

// Rate limit violations
traces
| where timestamp > ago(1h)
| where message contains "rate limit"
| summarize count() by bin(timestamp, 5m)
```

### Alerts

Configure alerts for:

- Response time > 2 seconds
- Error rate > 5%
- Availability < 99.5%
- Rate limit violations spike

## Testing

### Test with Subscription Key

```bash
# Get subscription key
SUBSCRIPTION_KEY=$(az apim product subscription show \
  --resource-group rg-userservice-dev \
  --service-name apim-userservice-dev \
  --product-id starter \
  --subscription-id dev-subscription \
  --query primaryKey \
  --output tsv)

# Test API call
curl -X GET "https://apim-userservice-dev.azure-api.net/userservice/api/v1/health" \
  -H "Ocp-Apim-Subscription-Key: $SUBSCRIPTION_KEY"
```

### Test with JWT Token

```bash
# Get B2C token (use Postman or MSAL.js)
TOKEN="eyJ0eXAiOiJKV1QiLCJhbGc..."

# Test authenticated endpoint
curl -X GET "https://apim-userservice-dev.azure-api.net/userservice/api/v1/users" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Ocp-Apim-Subscription-Key: $SUBSCRIPTION_KEY"
```

### Test Rate Limiting

```bash
# Rapid fire requests to trigger rate limit
for i in {1..60}; do
  curl -X GET "https://apim-userservice-dev.azure-api.net/userservice/api/v1/health" \
    -H "Ocp-Apim-Subscription-Key: $SUBSCRIPTION_KEY" \
    -w "\n%{http_code}\n"
done
```

Expected: First 50 requests succeed (200), remaining return 429 (Too Many Requests).

### Test Developer Portal

1. Navigate to developer portal: `https://apim-userservice-dev.developer.azure-api.net`
2. Sign up for account
3. Subscribe to Starter product
4. Get subscription key
5. Test API using built-in console

## Best Practices

### Policy Design

✅ **Principle of Least Privilege**: Apply most restrictive policies first
✅ **Defense in Depth**: Layer multiple security policies (JWT + subscription key + rate limit)
✅ **Fail Secure**: Default deny, explicitly allow
✅ **Centralized Logging**: Log all requests to Application Insights
✅ **Idempotency**: Use request IDs for duplicate detection

### Performance

✅ **Cache Aggressively**: Cache GET requests with appropriate TTL
✅ **Minimize Transformations**: Avoid complex transformations in policies
✅ **Backend Pooling**: Use connection pooling for backend services
✅ **Compression**: Enable response compression
✅ **CDN Integration**: Use Azure Front Door for static content

### Security

✅ **HTTPS Only**: Disable HTTP protocol
✅ **Hide Backend**: Never expose backend URLs in responses
✅ **Rotate Keys**: Rotate subscription keys quarterly
✅ **Audit Logs**: Enable diagnostic logs for all operations
✅ **Least Privilege**: Use managed identities for backend access

### Scalability

✅ **Autoscaling**: Configure autoscale rules based on CPU/memory
✅ **Multi-Region**: Deploy to multiple regions (Premium tier)
✅ **Health Checks**: Configure backend health probes
✅ **Circuit Breaker**: Implement retry and circuit breaker patterns
✅ **Load Testing**: Regular load testing to identify bottlenecks

## Troubleshooting

### Common Issues

**Issue**: 401 Unauthorized even with valid JWT token

**Solutions**:

- Verify JWT issuer matches B2C tenant
- Check audience matches API client ID
- Ensure token hasn't expired
- Verify OpenID config URL is correct
- Check required claims are present

**Issue**: 429 Too Many Requests

**Solutions**:

- Check rate limit policy configuration
- Verify counter-key is correct
- Review subscription quota limits
- Consider upgrading to Unlimited product

**Issue**: 500 Internal Server Error from APIM

**Solutions**:

- Check Application Insights for detailed errors
- Verify backend service is healthy
- Review policy XML for syntax errors
- Check named values are configured correctly

**Issue**: Slow response times

**Solutions**:

- Enable response caching
- Optimize backend queries
- Reduce policy transformations
- Scale up APIM instance
- Use Azure Front Door for CDN

**Issue**: Cannot import OpenAPI spec

**Solutions**:

- Validate OpenAPI spec at https://editor.swagger.io
- Ensure backend URL is accessible from APIM
- Check CORS settings on backend
- Try manual operation configuration

### Diagnostic Tools

```bash
# Check APIM status
az apim show \
  --resource-group rg-userservice-dev \
  --name apim-userservice-dev \
  --query "{Name:name, Status:provisioningState, Gateway:gatewayUrl}" \
  --output table

# Test backend connectivity
az apim api test \
  --resource-group rg-userservice-dev \
  --service-name apim-userservice-dev \
  --api-id userservice-api \
  --operation-id get-health

# View diagnostic logs
az monitor diagnostic-settings list \
  --resource-id "/subscriptions/{subscription-id}/resourceGroups/rg-userservice-dev/providers/Microsoft.ApiManagement/service/apim-userservice-dev"
```

### Support Resources

- [Azure APIM Documentation](https://learn.microsoft.com/en-us/azure/api-management/)
- [Policy Reference](https://learn.microsoft.com/en-us/azure/api-management/api-management-policies)
- [Troubleshooting Guide](https://learn.microsoft.com/en-us/azure/api-management/api-management-troubleshoot)
- [Azure Support](https://azure.microsoft.com/en-us/support/)

## Conclusion

This guide provides comprehensive instructions for setting up Azure API Management as a secure, scalable gateway for the UserService API. With JWT validation, rate limiting, caching, and detailed monitoring, APIM provides enterprise-grade API management capabilities.

**Next Steps**:

1. Provision APIM instance (30-45 minutes)
2. Configure policies and named values
3. Import UserService API
4. Create products and subscriptions
5. Test all endpoints
6. Configure monitoring and alerts
7. Proceed to Task 11: Implement React Frontend
