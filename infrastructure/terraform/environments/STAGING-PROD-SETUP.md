# Staging and Production Environment Setup

## Overview

This document describes the Terraform configuration for **Staging** and **Production** environments, which have been set up to mirror the Development environment structure with appropriate configuration differences for each environment tier.

## Environment Differences

### SKU and Tier Comparisons

| Component        | Development                     | Staging                             | Production                           |
| ---------------- | ------------------------------- | ----------------------------------- | ------------------------------------ |
| **APIM SKU**     | Developer_1 ($50/mo)            | Basic_1 ($150/mo)                   | Standard_1 ($700/mo)                 |
| **PostgreSQL**   | B_Standard_B1ms (1 vCore, 2 GB) | GP_Standard_D2s_v3 (2 vCores, 8 GB) | GP_Standard_D4s_v3 (4 vCores, 16 GB) |
| **Storage**      | 32 GB                           | 64 GB                               | 128 GB                               |
| **Redis**        | Basic C0 (250 MB)               | Standard C1 (1 GB)                  | Premium P1 (6 GB + persistence)      |
| **ACR**          | Basic                           | Standard                            | Premium (geo-replication)            |
| **Key Vault**    | Standard                        | Standard                            | Premium (HSM-backed keys)            |
| **B2C SKU**      | PremiumP1                       | PremiumP1                           | PremiumP2                            |
| **Min Replicas** | 1                               | 2                                   | 3                                    |
| **Max Replicas** | 2                               | 5                                   | 10                                   |

### Rate Limiting Differences

| Tier                        | Development     | Staging          | Production       |
| --------------------------- | --------------- | ---------------- | ---------------- |
| **Starter - Rate Limit**    | 50 calls/min    | 100 calls/min    | 200 calls/min    |
| **Starter - Daily Quota**   | 5,000 calls/day | 10,000 calls/day | 20,000 calls/day |
| **Unlimited - Rate Limit**  | 1,000 calls/min | 2,000 calls/min  | 5,000 calls/min  |
| **Unlimited - Daily Quota** | 1M calls/day    | 2M calls/day     | 10M calls/day    |
| **Cache Duration**          | 5 minutes       | 10 minutes       | 15 minutes       |
| **Backend Timeout**         | 30 seconds      | 60 seconds       | 90 seconds       |

### CORS Configuration

| Environment     | Allowed Origins                                                                                       |
| --------------- | ----------------------------------------------------------------------------------------------------- |
| **Development** | `http://localhost:3000`<br>`http://localhost:5173`<br>`https://userservice-spa-dev.azurewebsites.net` |
| **Staging**     | `https://userservice-spa-staging.azurewebsites.net`<br>`https://staging.userservice.com`              |
| **Production**  | `https://userservice.com`<br>`https://www.userservice.com`<br>`https://app.userservice.com`           |

### Monitoring Retention

| Environment     | Log Analytics | Application Insights |
| --------------- | ------------- | -------------------- |
| **Development** | 30 days       | 90 days              |
| **Staging**     | 60 days       | 180 days             |
| **Production**  | 90 days       | 365 days             |

## Directory Structure

```
infrastructure/terraform/environments/
├── dev/
│   ├── main.tf
│   ├── variables.tf
│   ├── outputs.tf
│   ├── apim.tf
│   ├── b2c.tf
│   └── terraform.tfvars.example
├── staging/
│   ├── main.tf
│   ├── variables.tf
│   ├── outputs.tf
│   ├── apim.tf
│   ├── b2c.tf
│   └── terraform.tfvars.example
└── prod/
    ├── main.tf
    ├── variables.tf
    ├── outputs.tf
    ├── apim.tf
    ├── b2c.tf
    └── terraform.tfvars.example
```

## Files Created

### Staging Environment

1. **main.tf** - Terraform provider configuration and resource group
2. **variables.tf** - All input variables for the staging environment
3. **outputs.tf** - Output values for created resources
4. **apim.tf** - Azure API Management configuration with Basic SKU
5. **b2c.tf** - Azure AD B2C configuration for staging authentication
6. **terraform.tfvars.example** - Example variable values for staging

### Production Environment

1. **main.tf** - Terraform provider configuration and resource group
2. **variables.tf** - All input variables for the production environment
3. **outputs.tf** - Output values for created resources
4. **apim.tf** - Azure API Management configuration with Standard SKU
5. **b2c.tf** - Azure AD B2C configuration for production authentication
6. **terraform.tfvars.example** - Example variable values for production

## Current Implementation Status

### ✅ Fully Implemented

- **APIM Module** - Complete Terraform module with all resources

  - API Management instance
  - UserService API configuration
  - Product definitions (Starter, Unlimited)
  - JWT validation policies
  - Rate limiting policies
  - Response caching
  - CORS configuration
  - Application Insights integration

- **B2C Module** - Complete Terraform module (from Task 9)
  - Tenant configuration
  - Application registrations (API + SPA)
  - OAuth scopes
  - Redirect URIs

### ⚠️ Placeholder/Future Implementation

The following modules are referenced in the configurations but need to be implemented:

- **ACR Module** - Azure Container Registry
- **Key Vault Module** - Secret management
- **Monitoring Module** - Log Analytics + Application Insights
- **PostgreSQL Module** - Database server
- **Redis Module** - Cache server
- **Container Apps Module** - Application hosting

> **Note**: The main.tf files in staging and prod environments have been simplified to only include the resource group and random password resources. The placeholder module references have been removed to avoid Terraform errors. These modules should be added when fully implemented.

## Setup Instructions

### Prerequisites

- Azure CLI installed and authenticated
- Terraform 1.9.0 or later installed
- Azure subscription with appropriate permissions
- B2C tenant created and configured (see Task 9 documentation)

### Staging Environment Setup

1. **Navigate to staging directory**:

   ```bash
   cd infrastructure/terraform/environments/staging
   ```

2. **Create terraform.tfvars file**:

   ```bash
   cp terraform.tfvars.example terraform.tfvars
   ```

3. **Update terraform.tfvars** with your actual values:

   ```hcl
   subscription_id = "your-subscription-id"
   tenant_id       = "your-tenant-id"
   b2c_tenant_id   = "your-b2c-tenant-id"
   b2c_domain      = "userservice-staging.b2clogin.com"
   b2c_api_client_id = "your-b2c-api-client-id"
   ```

4. **Initialize Terraform**:

   ```bash
   terraform init
   ```

5. **Review the plan**:

   ```bash
   terraform plan
   ```

6. **Apply the configuration**:
   ```bash
   terraform apply
   ```

### Production Environment Setup

1. **Navigate to production directory**:

   ```bash
   cd infrastructure/terraform/environments/prod
   ```

2. **Create terraform.tfvars file**:

   ```bash
   cp terraform.tfvars.example terraform.tfvars
   ```

3. **Update terraform.tfvars** with your actual values:

   ```hcl
   subscription_id = "your-subscription-id"
   tenant_id       = "your-tenant-id"
   b2c_tenant_id   = "your-b2c-tenant-id"
   b2c_domain      = "userservice-prod.b2clogin.com"
   b2c_api_client_id = "your-b2c-api-client-id"
   ```

4. **Initialize Terraform**:

   ```bash
   terraform init
   ```

5. **Review the plan**:

   ```bash
   terraform plan
   ```

6. **Apply the configuration**:
   ```bash
   terraform apply
   ```

## Backend Configuration

For production use, uncomment and configure the backend block in each environment's main.tf:

```hcl
backend "azurerm" {
  resource_group_name  = "terraform-state-rg"
  storage_account_name = "tfstatekafkadotnet"
  container_name       = "tfstate"
  key                  = "staging/kafka-dotnet.tfstate"  # or "prod/kafka-dotnet.tfstate"
}
```

## Testing APIM Configuration

After applying the Terraform configuration, test the APIM setup:

### Staging

```bash
# Get the gateway URL
terraform output apim_gateway_url

# Test with curl (requires valid JWT token)
curl -X GET "$(terraform output -raw apim_gateway_url)/userservice/api/users" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Ocp-Apim-Subscription-Key: $(terraform output -raw apim_dev_subscription_key)"
```

### Production

```bash
# Get the gateway URL
terraform output apim_gateway_url

# Test with curl (requires valid JWT token)
curl -X GET "$(terraform output -raw apim_gateway_url)/userservice/api/users" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Ocp-Apim-Subscription-Key: $(terraform output -raw apim_dev_subscription_key)"
```

## Outputs Available

Each environment provides the following outputs:

### Resource Group

- `resource_group_name` - Name of the resource group
- `resource_group_location` - Azure region

### APIM

- `apim_gateway_url` - API Gateway URL
- `apim_developer_portal_url` - Developer portal URL
- `apim_management_url` - Management API URL
- `apim_principal_id` - Managed identity principal ID
- `userservice_api_base_url` - Full API URL through APIM
- `apim_dev_subscription_key` - Development subscription key (sensitive)
- `apim_test_command` - Curl command for testing (sensitive)
- `apim_configuration` - Configuration summary for frontend
- `apim_api_urls` - Complete API URLs
- `application_insights_connection_string` - App Insights connection (sensitive)

### B2C

- `b2c_api_client_id` - API application client ID
- `b2c_spa_client_id` - SPA application client ID
- `b2c_api_scopes` - Available API scopes
- `b2c_configuration_summary` - Complete B2C configuration

## Security Considerations

### Staging

- Uses Basic tier APIM (no VNet support)
- Standard ACR (no geo-replication)
- Standard Key Vault
- Sign-up disabled on developer portal
- HTTPS-only origins

### Production

- Uses Standard/Premium tier APIM
- Consider VNet integration (`virtual_network_type = "External"` or `"Internal"`)
- Premium ACR with geo-replication
- Premium Key Vault with HSM-backed keys
- Sign-up disabled on developer portal
- HTTPS-only origins
- Higher rate limits and quotas
- Additional compliance tags (GDPR)
- Longer retention periods for logs

## Cost Estimation

### Staging Environment (Monthly)

- APIM Basic: ~$150
- PostgreSQL (GP 2 vCore): ~$150
- Redis Standard (1 GB): ~$75
- ACR Standard: ~$20
- Container Apps: ~$50
- Application Insights: ~$30
- **Total: ~$475/month**

### Production Environment (Monthly)

- APIM Standard: ~$700
- PostgreSQL (GP 4 vCore): ~$300
- Redis Premium (6 GB): ~$250
- ACR Premium: ~$40
- Container Apps: ~$100
- Application Insights: ~$50
- **Total: ~$1,440/month**

> **Note**: Costs are estimates and may vary based on actual usage, region, and Azure pricing changes.

## Next Steps

1. **Deploy Staging Environment**

   - Set up terraform.tfvars with staging values
   - Run `terraform apply` in staging directory
   - Test APIM endpoints
   - Configure frontend application with staging APIM URL

2. **Deploy Production Environment**

   - Set up terraform.tfvars with production values
   - Run `terraform apply` in prod directory
   - Test APIM endpoints
   - Configure frontend application with production APIM URL

3. **Complete Remaining Modules** (Future Work)

   - Implement ACR module
   - Implement Key Vault module
   - Implement Monitoring module
   - Implement PostgreSQL module
   - Implement Redis module
   - Implement Container Apps module
   - Update main.tf in each environment to use these modules

4. **Configure CI/CD** (Task 12)
   - Set up GitHub Actions for automated deployments
   - Configure deployment pipelines for staging and production
   - Implement blue-green or canary deployment strategies

## Troubleshooting

### Common Issues

1. **APIM provisioning timeout**

   - APIM can take 30-45 minutes to provision
   - This is normal behavior for Azure API Management

2. **B2C configuration not found**

   - Ensure B2C tenant is created first (manual step)
   - Verify B2C application registrations exist
   - Check that tenant ID and domain match

3. **Rate limit exceeded during testing**

   - Use different subscription keys for different products
   - Check rate limit headers in response
   - Wait for the renewal period to reset counters

4. **JWT validation fails**
   - Verify B2C tenant ID, domain, and client ID are correct
   - Ensure the JWT token is from the correct B2C tenant
   - Check that the audience claim matches the client ID

## Related Documentation

- [Task 9: Azure AD B2C Setup](../../../docs/azure-b2c-setup.md)
- [Task 10: APIM Setup](../../../docs/azure-apim-setup.md)
- [Task 10 Summary](../../../docs/task-10-apim-summary.md)
- [Terraform APIM Module](../../modules/apim/README.md)
- [Terraform B2C Module](../../modules/b2c/README.md)
