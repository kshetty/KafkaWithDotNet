# Staging and Production Environment Configuration - Summary

## Overview

Terraform configurations for **Staging** and **Production** environments have been successfully created, mirroring the Development environment structure with appropriate environment-specific configurations.

## Files Created

### Staging Environment (6 files)

```
infrastructure/terraform/environments/staging/
├── main.tf                      - Provider config and resource group
├── variables.tf                 - All input variables
├── outputs.tf                   - Output values
├── apim.tf                      - APIM with Basic SKU ($150/mo)
├── b2c.tf                       - Azure AD B2C configuration
└── terraform.tfvars.example     - Example variable values
```

### Production Environment (6 files)

```
infrastructure/terraform/environments/prod/
├── main.tf                      - Provider config and resource group
├── variables.tf                 - All input variables
├── outputs.tf                   - Output values
├── apim.tf                      - APIM with Standard SKU ($700/mo)
├── b2c.tf                       - Azure AD B2C configuration
└── terraform.tfvars.example     - Example variable values
```

### Documentation (1 file)

```
infrastructure/terraform/environments/
└── STAGING-PROD-SETUP.md        - Complete setup guide with all details
```

**Total: 13 files created**

## Key Configuration Differences

### Infrastructure Tiers

| Component      | Dev             | Staging        | Production      |
| -------------- | --------------- | -------------- | --------------- |
| **APIM**       | Developer ($50) | Basic ($150)   | Standard ($700) |
| **PostgreSQL** | 1 vCore, 2 GB   | 2 vCores, 8 GB | 4 vCores, 16 GB |
| **Redis**      | Basic 250 MB    | Standard 1 GB  | Premium 6 GB    |
| **ACR**        | Basic           | Standard       | Premium         |
| **Replicas**   | 1-2             | 2-5            | 3-10            |

### Rate Limits

| Product Tier  | Dev            | Staging          | Production       |
| ------------- | -------------- | ---------------- | ---------------- |
| **Starter**   | 50/min, 5K/day | 100/min, 10K/day | 200/min, 20K/day |
| **Unlimited** | 1K/min, 1M/day | 2K/min, 2M/day   | 5K/min, 10M/day  |

### CORS Origins

- **Dev**: localhost:3000, localhost:5173, dev Azure URLs
- **Staging**: staging.userservice.com, Azure staging URLs
- **Production**: userservice.com, www.userservice.com, app.userservice.com

## Quick Start

### Staging Deployment

```bash
cd infrastructure/terraform/environments/staging
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values
terraform init
terraform plan
terraform apply
```

### Production Deployment

```bash
cd infrastructure/terraform/environments/prod
cp terraform.tfvars.example terraform.tfvars
# Edit terraform.tfvars with your values
terraform init
terraform plan
terraform apply
```

## Configuration Requirements

Each environment needs these values in `terraform.tfvars`:

### Required Variables

- `subscription_id` - Azure subscription ID
- `tenant_id` - Azure AD tenant ID
- `b2c_tenant_id` - B2C tenant ID
- `b2c_domain` - B2C domain (e.g., `userservice-staging.b2clogin.com`)
- `b2c_api_client_id` - B2C API application client ID

### Environment-Specific URLs

- **Staging backend**: `https://userservice-staging.azurewebsites.net`
- **Production backend**: `https://userservice-prod.azurewebsites.net`

## APIM Configuration Highlights

### Staging

- **SKU**: Basic_1 (1 unit, 2 GB cache)
- **Rate Limit**: 100 calls/min (Starter), 2K calls/min (Unlimited)
- **Cache**: 10 minutes
- **Timeout**: 60 seconds
- **Developer Portal**: Enabled (sign-up disabled)

### Production

- **SKU**: Standard_1 (1 unit, 4 GB cache)
- **Rate Limit**: 200 calls/min (Starter), 5K calls/min (Unlimited)
- **Cache**: 15 minutes
- **Timeout**: 90 seconds
- **Developer Portal**: Enabled (sign-up disabled)
- **Consider**: VNet integration for enhanced security

## Security Features

Both environments include:

- ✅ JWT token validation via Azure AD B2C
- ✅ Subscription key authentication
- ✅ Per-user rate limiting (100 calls/min)
- ✅ CORS with whitelist-only origins
- ✅ HTTPS-only endpoints
- ✅ Security headers (HSTS, CSP, X-Frame-Options)
- ✅ Request ID tracking for distributed tracing
- ✅ Application Insights monitoring

## Outputs Available

Each environment provides:

- APIM gateway URL
- Developer portal URL
- Management API URL
- API base URL for frontend
- Subscription keys (sensitive)
- B2C client IDs and configuration
- Test curl commands

## Cost Estimates

### Staging: ~$475/month

- APIM Basic: $150
- PostgreSQL (2 vCore): $150
- Redis Standard: $75
- ACR Standard: $20
- Container Apps: $50
- App Insights: $30

### Production: ~$1,440/month

- APIM Standard: $700
- PostgreSQL (4 vCore): $300
- Redis Premium: $250
- ACR Premium: $40
- Container Apps: $100
- App Insights: $50

## Testing

Test APIM after deployment:

```bash
# Get subscription key
terraform output -raw apim_dev_subscription_key

# Test endpoint
curl -X GET "https://apim-userservice-staging.azure-api.net/userservice/api/users" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Ocp-Apim-Subscription-Key: YOUR_SUBSCRIPTION_KEY"
```

## Implementation Status

### ✅ Fully Functional

- APIM module (all features)
- B2C module (all features)
- Resource group creation
- Terraform backend configuration
- Variable definitions
- Output definitions

### ⚠️ Placeholder (Future Work)

The following modules are referenced but not yet implemented:

- ACR (Container Registry)
- Key Vault
- Monitoring (Log Analytics + App Insights)
- PostgreSQL
- Redis
- Container Apps

> These modules will be added when infrastructure modules are completed.

## Integration with CI/CD (Task 12)

The staging and production configurations are ready for CI/CD integration:

1. **GitHub Actions Workflow**: Deploy to staging on merge to `develop`
2. **GitHub Actions Workflow**: Deploy to production on merge to `main`
3. **Approval Gates**: Require approval before production deployment
4. **Terraform State**: Store in Azure Storage backend (commented in main.tf)

## Next Steps

1. ✅ **Staging/Prod Terraform Created** (Current)
2. ⏭️ **Task 11**: Implement React Frontend with MSAL.js
3. ⏭️ **Task 12**: Create CI/CD pipelines for automated deployment
4. ⏭️ **Complete Infrastructure Modules**: Implement remaining modules
5. ⏭️ **Enable Backend State**: Uncomment backend blocks in main.tf

## Documentation Links

- **Complete Setup Guide**: [STAGING-PROD-SETUP.md](../infrastructure/terraform/environments/STAGING-PROD-SETUP.md)
- **APIM Documentation**: [azure-apim-setup.md](./azure-apim-setup.md)
- **B2C Documentation**: [azure-b2c-setup.md](./azure-b2c-setup.md)
- **Task 10 Summary**: [task-10-apim-summary.md](./task-10-apim-summary.md)

## Troubleshooting

### Common Issues

1. **APIM takes 30-45 minutes to provision** - This is normal
2. **B2C validation fails** - Check tenant ID, domain, and client ID match
3. **Rate limit exceeded** - Check product subscription and limits
4. **CORS errors** - Verify frontend URL is in allowed_origins list

### Support Resources

- Azure APIM Documentation: https://learn.microsoft.com/azure/api-management/
- Azure B2C Documentation: https://learn.microsoft.com/azure/active-directory-b2c/
- Terraform azurerm Provider: https://registry.terraform.io/providers/hashicorp/azurerm/

---

**Status**: ✅ Staging and Production environments configured and ready for deployment
**Date**: November 2025
**Version**: 1.0
