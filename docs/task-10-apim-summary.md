# Task 10: Azure API Management Setup - Completion Summary

## Overview

Successfully created comprehensive documentation, Terraform configurations, policy definitions, and automation scripts for setting up Azure API Management (APIM) as the secure gateway for the UserService API.

## Deliverables

### 1. Documentation (1 file)

- **docs/azure-apim-setup.md** - 800+ line comprehensive guide covering:
  - Why Azure API Management (benefits and use cases)
  - Complete architecture diagrams with request flow
  - APIM tier comparison and pricing recommendations
  - Infrastructure setup with Terraform
  - Policy configuration (JWT validation, rate limiting, caching)
  - API import and configuration procedures
  - Security features (subscription keys, IP filtering, mutual TLS)
  - Monitoring and analytics with Application Insights
  - Testing procedures and best practices
  - Troubleshooting guide with common issues

### 2. Terraform Configuration (4 files)

**Core Module**:

- **infrastructure/terraform/modules/apim/main.tf** - APIM instance with:

  - API Management resource with managed identity
  - Application Insights for monitoring
  - Custom domain support (optional)
  - Named values for configuration management
  - Backend definition for UserService
  - Global policy configuration
  - Diagnostic settings integration

- **infrastructure/terraform/modules/apim/variables.tf** - Module inputs (45+ variables):

  - APIM configuration (name, SKU, publisher info)
  - Backend and B2C configuration
  - Rate limiting and quota settings
  - CORS and security settings
  - VNet integration support
  - Environment-specific customization

- **infrastructure/terraform/modules/apim/outputs.tf** - Module outputs (20+ outputs):
  - APIM URLs (gateway, portal, management)
  - API configuration details
  - Subscription keys (for dev environment)
  - Application Insights connection strings
  - Test commands

**API Configuration**:

- **infrastructure/terraform/modules/apim/api-userservice.tf** - UserService API:
  - API definition with OpenAPI import
  - API version set
  - JWT validation policy
  - Rate limiting and caching
  - Product definitions (Starter, Unlimited)
  - Product associations
  - Development subscription (dev environment only)

### 3. Policy Definitions (4 XML files)

- **policies/global-policy.xml** - Global policies:

  - CORS configuration for SPA applications
  - Security headers (CSP, HSTS, X-Frame-Options, etc.)
  - Request ID generation for distributed tracing
  - Response time tracking
  - Error handling with structured responses
  - Internal header removal

- **policies/userservice-api-policy.xml** - API-level policies:

  - Azure AD B2C JWT token validation
  - User context extraction from token
  - Subscription key validation
  - Rate limiting (100 calls/min per user)
  - Quota management (10,000 calls/day)
  - Response caching for GET requests (5 min TTL)
  - Backend URL configuration
  - Custom error responses for rate limit violations

- **policies/starter-product-policy.xml** - Starter tier policies:

  - Rate limiting: 50 calls/minute
  - Daily quota: 5,000 calls/day
  - Rate limit headers in response
  - Subscription key-based tracking

- **policies/unlimited-product-policy.xml** - Unlimited tier policies:
  - Rate limiting: 1,000 calls/minute
  - Daily quota: 1,000,000 calls/day
  - Priority routing header
  - Enhanced rate limit headers

### 4. Environment Configuration (1 file)

- **infrastructure/terraform/environments/dev/apim.tf** - Dev environment:
  - APIM module instantiation
  - Developer tier SKU
  - B2C integration configuration
  - CORS origins (localhost + Azure)
  - Rate limit configurations
  - Comprehensive outputs

### 5. Automation Scripts (2 files)

- **infrastructure/scripts/setup-apim.sh** - Bash script for Unix/Linux/macOS:

  - APIM instance creation (30-45 minute wait handling)
  - Named values configuration
  - API import from OpenAPI spec
  - Product creation and configuration
  - Subscription management
  - Configuration file generation
  - Color-coded progress output

- **infrastructure/scripts/setup-apim.ps1** - PowerShell script for Windows:
  - Same functionality as bash script
  - Windows-compatible cmdlets
  - PowerShell-style parameter handling
  - JSON configuration export

### 6. Variable Updates (1 file)

- **infrastructure/terraform/environments/dev/variables.tf** - Added B2C variables:
  - `b2c_tenant_id` - B2C Tenant ID (sensitive)
  - `b2c_domain` - B2C domain
  - `b2c_api_client_id` - API Client ID (sensitive)

## Architecture

### APIM Request Flow

```
React SPA → APIM Gateway → Backend API
    ↓           ↓              ↓
1. Request   2. Validate    3. Process
             - JWT token
             - Subscription key
             - Rate limits
             - Cache lookup
```

### APIM Components

1. **Gateway**: Public endpoint for API requests
2. **Developer Portal**: API documentation and testing
3. **Products**: Starter (limited) and Unlimited (premium)
4. **APIs**: UserService API v1 with OpenAPI import
5. **Policies**: JWT validation, rate limiting, caching
6. **Backend**: UserService API endpoint
7. **Application Insights**: Monitoring and analytics

## Configuration Details

### SKU Tiers

| Tier      | Cost/Month | Throughput     | SLA    | Use Case                       |
| --------- | ---------- | -------------- | ------ | ------------------------------ |
| Developer | ~$50       | 500 req/sec    | None   | Development/Testing            |
| Basic     | ~$150      | 1,000 req/sec  | 99.95% | Small production               |
| Standard  | ~$700      | 2,500 req/sec  | 99.95% | **Recommended for production** |
| Premium   | ~$3,000+   | 4,000+ req/sec | 99.95% | Enterprise/Multi-region        |

### Security Features

✅ **JWT Token Validation**: Azure AD B2C token verification
✅ **Subscription Keys**: Ocp-Apim-Subscription-Key header required
✅ **Rate Limiting**: Per-user and per-subscription limits
✅ **CORS**: Configurable allowed origins
✅ **Security Headers**: HSTS, CSP, X-Frame-Options, etc.
✅ **IP Filtering**: Optional IP whitelist/blacklist
✅ **Mutual TLS**: Client certificate validation (Standard/Premium)
✅ **Managed Identity**: System-assigned identity for secure access

### Rate Limits

**Starter Product**:

- Rate limit: 50 calls/minute per subscription
- Daily quota: 5,000 calls/day
- Approval: Automatic
- Subscription limit: 10

**Unlimited Product**:

- Rate limit: 1,000 calls/minute per subscription
- Daily quota: 1,000,000 calls/day
- Approval: Manual (admin approval required)
- Subscription limit: Unlimited

**API-Level**:

- Per-user rate limit: 100 calls/minute
- Per-subscription quota: 10,000 calls/day

### Caching Strategy

**Cache Enabled For**:

- HTTP GET requests only
- Successful responses (200 OK)
- Cache duration: 300 seconds (5 minutes)

**Cache Varies By**:

- Accept header
- Accept-Charset header
- Accept-Encoding header
- Authorization header (user-specific caching)
- Query parameters

**Cache Control**:

- X-Cache header added (HIT/MISS)
- Cache invalidation on POST/PUT/DELETE

### Monitoring

**Application Insights Metrics**:

- Request count and duration
- Failed requests (4xx, 5xx)
- Cache hit ratio
- Rate limit violations
- Backend response time
- Availability percentage

**Log Analytics Integration**:

- Gateway logs (all requests)
- WebSocket connection logs
- Diagnostic metrics

**Custom Alerts** (Recommended):

- Response time > 2 seconds
- Error rate > 5%
- Availability < 99.5%
- Rate limit violations spike

## Setup Process

### Automated Setup

**Using Bash Script** (macOS/Linux):

```bash
./infrastructure/scripts/setup-apim.sh \
  -b https://userservice-dev.azurewebsites.net \
  --b2c-tenant-id "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" \
  --b2c-domain "userservice.b2clogin.com" \
  --b2c-client-id "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy"
```

**Using PowerShell** (Windows):

```powershell
./infrastructure/scripts/setup-apim.ps1 `
  -BackendUrl "https://userservice-dev.azurewebsites.net" `
  -B2cTenantId "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" `
  -B2cDomain "userservice.b2clogin.com" `
  -B2cClientId "yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy"
```

**Script Actions**:

1. ✅ Validate Azure CLI installation and login
2. ✅ Create/verify resource group
3. ✅ Create APIM instance (30-45 minutes)
4. ✅ Configure named values (backend URL, B2C config)
5. ✅ Import UserService API from OpenAPI spec
6. ✅ Create products (Starter, Unlimited)
7. ✅ Associate API with products
8. ✅ Create development subscription (dev only)
9. ✅ Generate configuration file

### Terraform Deployment

```bash
cd infrastructure/terraform/environments/dev

# Initialize
terraform init

# Plan (review changes)
terraform plan -var-file="dev.tfvars"

# Apply
terraform apply -var-file="dev.tfvars"
```

**Terraform Creates**:

- ✅ APIM instance with managed identity
- ✅ Application Insights for monitoring
- ✅ Named values for configuration
- ✅ Backend definition
- ✅ UserService API with version set
- ✅ API policies (JWT, rate limiting, caching)
- ✅ Products (Starter, Unlimited)
- ✅ Product policies
- ✅ Development subscription (dev environment)
- ✅ Diagnostic settings

## Testing

### Test Health Endpoint

```bash
# Get subscription key from script output or Azure Portal
SUBSCRIPTION_KEY="your-subscription-key"
GATEWAY_URL="https://apim-userservice-dev.azure-api.net"

# Test health endpoint (no auth required)
curl -X GET "$GATEWAY_URL/userservice/api/v1/health" \
  -H "Ocp-Apim-Subscription-Key: $SUBSCRIPTION_KEY"
```

Expected: `200 OK` with health status

### Test Authenticated Endpoint

```bash
# Get B2C token (from Postman or MSAL.js)
TOKEN="eyJ0eXAiOiJKV1QiLCJhbGc..."

# Test users endpoint (requires JWT + subscription key)
curl -X GET "$GATEWAY_URL/userservice/api/v1/users" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Ocp-Apim-Subscription-Key: $SUBSCRIPTION_KEY"
```

Expected: `200 OK` with user list (if token valid) or `401 Unauthorized`

### Test Rate Limiting

```bash
# Rapid fire 60 requests (Starter limit is 50/min)
for i in {1..60}; do
  curl -X GET "$GATEWAY_URL/userservice/api/v1/health" \
    -H "Ocp-Apim-Subscription-Key: $SUBSCRIPTION_KEY" \
    -w "\n%{http_code}\n" \
    -s -o /dev/null
done
```

Expected: First 50 requests return `200`, remaining return `429 Too Many Requests`

### Test Developer Portal

1. Navigate to: `https://apim-userservice-dev.developer.azure-api.net`
2. Sign up for account
3. Subscribe to Starter product
4. Get subscription key
5. Test API using built-in console
6. View API documentation

## Integration Points

### Frontend (React)

**Environment Variables**:

```env
VITE_APIM_GATEWAY_URL=https://apim-userservice-dev.azure-api.net
VITE_APIM_SUBSCRIPTION_KEY=your-subscription-key
VITE_APIM_API_PATH=/userservice
```

**API Calls**:

```typescript
const response = await fetch(
  `${VITE_APIM_GATEWAY_URL}${VITE_APIM_API_PATH}/api/v1/users`,
  {
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "Ocp-Apim-Subscription-Key": VITE_APIM_SUBSCRIPTION_KEY,
    },
  }
);
```

### Backend (ASP.NET Core)

**No Changes Required**: Backend receives requests from APIM with:

- `X-User-ID` header (extracted from JWT)
- `X-User-Email` header (extracted from JWT)
- `X-Request-ID` header (for tracing)
- `Authorization` header (original JWT token)

**Optional**: Add middleware to trust APIM headers

## Best Practices Implemented

✅ **Defense in Depth**: JWT validation + subscription key + rate limits
✅ **Principle of Least Privilege**: User vs admin scopes
✅ **Fail Secure**: Default deny, explicit allow
✅ **Distributed Tracing**: Request IDs for correlation
✅ **Response Caching**: Reduce backend load for read operations
✅ **Security Headers**: HSTS, CSP, X-Frame-Options, etc.
✅ **Error Handling**: Structured error responses with request ID
✅ **Monitoring**: Application Insights integration
✅ **Scalability**: Auto-scaling support, multi-region capable
✅ **High Availability**: 99.95% SLA (Standard tier)

## Known Limitations

⚠️ **Provisioning Time**: 30-45 minutes to create APIM instance
⚠️ **OpenAPI Import**: May fail if backend not accessible; configure manually if needed
⚠️ **Custom Policies**: Advanced scenarios require XML policy editing
⚠️ **VNet Integration**: Only available in Premium tier
⚠️ **Multi-Region**: Only available in Premium tier
⚠️ **Token Size**: Large JWT tokens may exceed 8KB header limit

## Next Steps (Task 11 - React Frontend)

With APIM configured, proceed to:

1. **Create React Application**

   - Initialize Vite React TypeScript project
   - Install MSAL.js for authentication
   - Configure MSAL with Azure AD B2C settings

2. **Implement Authentication**

   - Create MSAL authentication provider
   - Implement login/logout flows
   - Handle token acquisition and renewal

3. **Integrate with APIM**

   - Configure API base URL to APIM gateway
   - Add subscription key to API calls
   - Implement error handling for rate limits

4. **UI Components**

   - Material-UI setup
   - User list component
   - User detail component
   - User create/edit forms

5. **State Management**
   - React Context or Redux
   - API response caching
   - Optimistic updates

## Files Created/Updated

### Created (11 files)

1. `docs/azure-apim-setup.md` - Comprehensive APIM guide
2. `infrastructure/terraform/modules/apim/main.tf` - APIM core resources
3. `infrastructure/terraform/modules/apim/variables.tf` - Module variables
4. `infrastructure/terraform/modules/apim/outputs.tf` - Module outputs
5. `infrastructure/terraform/modules/apim/api-userservice.tf` - API configuration
6. `infrastructure/terraform/modules/apim/policies/global-policy.xml` - Global policies
7. `infrastructure/terraform/modules/apim/policies/userservice-api-policy.xml` - API policies
8. `infrastructure/terraform/modules/apim/policies/starter-product-policy.xml` - Starter tier policies
9. `infrastructure/terraform/modules/apim/policies/unlimited-product-policy.xml` - Unlimited tier policies
10. `infrastructure/terraform/environments/dev/apim.tf` - Dev environment config
11. `infrastructure/scripts/setup-apim.sh` - Bash automation script
12. `infrastructure/scripts/setup-apim.ps1` - PowerShell automation script

### Updated (1 file)

1. `infrastructure/terraform/environments/dev/variables.tf` - Added B2C variables

## Success Criteria

✅ Comprehensive documentation created (800+ lines)
✅ Terraform module implemented with APIM, API, products, policies
✅ Policy XML files created for all security layers
✅ Automation scripts created for Windows and Unix
✅ Environment configuration updated with B2C integration
✅ JWT validation configured with Azure AD B2C
✅ Rate limiting implemented (per-user, per-subscription, per-product)
✅ Response caching enabled for GET requests
✅ Security headers configured
✅ Monitoring integrated with Application Insights
✅ Development subscription auto-created for testing
✅ Best practices documented

## Resources

- [Azure APIM Documentation](https://learn.microsoft.com/en-us/azure/api-management/)
- [Policy Reference](https://learn.microsoft.com/en-us/azure/api-management/api-management-policies)
- [JWT Validation Policy](https://learn.microsoft.com/en-us/azure/api-management/api-management-access-restriction-policies#ValidateJWT)
- [Rate Limiting Policy](https://learn.microsoft.com/en-us/azure/api-management/api-management-access-restriction-policies#LimitCallRate)
- [Caching Policies](https://learn.microsoft.com/en-us/azure/api-management/api-management-caching-policies)
- [Azure APIM Terraform](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/api_management)

## Conclusion

Task 10 is complete with comprehensive documentation, Terraform configurations, policy definitions, and automation scripts for Azure API Management. The APIM gateway provides enterprise-grade security with JWT validation, rate limiting, response caching, and detailed monitoring. The infrastructure is ready to serve as the secure entry point for all UserService API requests.

**Progress**: 10/15 tasks complete (67%)

**Ready to proceed with Task 11: Implement React Frontend**
