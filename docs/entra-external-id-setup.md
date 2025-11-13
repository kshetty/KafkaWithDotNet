# Entra External ID (Azure AD B2C) Setup Guide

## Overview

This guide walks through setting up Microsoft Entra External ID (formerly Azure AD B2C) for the UserService. Entra External ID provides customer identity and access management (CIAM) with support for social logins, custom branding, and user flows.

## Why Entra External ID?

- **Customer-Facing Authentication**: Purpose-built for external user authentication
- **Social Identity Providers**: Easy integration with Google, Facebook, Microsoft accounts
- **Custom User Flows**: Configurable sign-up, sign-in, password reset experiences
- **Custom Branding**: White-label authentication pages
- **Multi-Factor Authentication**: Built-in MFA support
- **Scalability**: Handles millions of users
- **Compliance**: SOC 2, ISO 27001, GDPR compliant

## Prerequisites

- Azure subscription with appropriate permissions
- Azure CLI installed: `az version`
- Owner or Contributor role on the subscription
- Basic understanding of OAuth 2.0 and OpenID Connect

## Architecture

```
┌─────────────────┐
│  React SPA      │
│  (Frontend)     │
└────────┬────────┘
         │
         │ 1. Redirect to login
         ▼
┌─────────────────────────────┐
│  Entra External ID          │
│  (Azure AD B2C)             │
│  - User Flows               │
│  - Identity Providers       │
│  - Custom Policies          │
└────────┬────────────────────┘
         │
         │ 2. Return tokens
         ▼
┌─────────────────┐
│  React SPA      │
│  (Has tokens)   │
└────────┬────────┘
         │
         │ 3. API calls with Bearer token
         ▼
┌─────────────────────────────┐
│  Azure APIM                 │
│  - Validate JWT             │
│  - Rate limiting            │
└────────┬────────────────────┘
         │
         │ 4. Forward request
         ▼
┌─────────────────────────────┐
│  UserService API            │
│  - Validate JWT             │
│  - Process request          │
└─────────────────────────────┘
```

## Step 1: Create Entra External ID Tenant

### Using Azure Portal

1. **Navigate to Azure Portal**

   - Go to https://portal.azure.com
   - Sign in with your Azure account

2. **Create Resource**

   - Click "Create a resource"
   - Search for "Azure Active Directory B2C"
   - Click "Create"

3. **Select Creation Type**

   - Choose "Create a new Azure AD B2C Tenant"
   - Fill in details:
     - **Organization name**: `UserService B2C`
     - **Initial domain name**: `userservice` (results in userservice.onmicrosoft.com)
     - **Country/Region**: Select your region
     - **Subscription**: Select your subscription
     - **Resource group**: `rg-userservice-dev` (or create new)

4. **Review and Create**

   - Review settings
   - Click "Create"
   - Wait 2-3 minutes for tenant creation

5. **Link to Subscription**
   - After creation, click "Link existing Azure AD B2C Tenant to my Azure subscription"
   - Select the B2C tenant you created
   - Select subscription and resource group
   - Provide a resource name: `userservice-b2c`
   - Click "Create"

### Using Azure CLI

```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "YOUR_SUBSCRIPTION_ID"

# Create resource group (if not exists)
az group create \
  --name rg-userservice-dev \
  --location eastus

# Note: Azure CLI doesn't support creating B2C tenants directly
# Use Azure Portal or Azure PowerShell for tenant creation

# After creating tenant in portal, link it to subscription
az resource create \
  --resource-group rg-userservice-dev \
  --resource-type Microsoft.AzureActiveDirectory/b2cDirectories \
  --name userservice-b2c \
  --location "United States" \
  --properties '{
    "createTenantProperties": {
      "displayName": "UserService B2C",
      "countryCode": "US"
    }
  }'
```

## Step 2: Configure User Flows

### Sign-Up and Sign-In Flow

1. **Navigate to B2C Tenant**

   - In Azure Portal, search for "Azure AD B2C"
   - Select your B2C tenant

2. **Create User Flow**

   - Left menu → User flows
   - Click "+ New user flow"
   - Select "Sign up and sign in"
   - Choose "Recommended" version
   - Name: `B2C_1_signup_signin`

3. **Configure Identity Providers**

   - Select "Email signup"
   - Optionally add social providers:
     - Microsoft Account
     - Google
     - Facebook

4. **Configure User Attributes**

   - **Collect during sign-up**:

     - ✅ Email Address
     - ✅ Display Name
     - ✅ Given Name
     - ✅ Surname

   - **Return in token**:
     - ✅ Email Addresses
     - ✅ Display Name
     - ✅ Given Name
     - ✅ Surname
     - ✅ User's Object ID

5. **Multi-Factor Authentication**

   - Method: `Email`
   - Enforcement: `Conditional` (recommended for production: `Always on`)

6. **Page Layouts**

   - Use default templates or customize with your branding
   - Can add custom HTML/CSS/JavaScript

7. **Create**
   - Click "Create"
   - Wait for flow creation

### Password Reset Flow

1. **Create User Flow**

   - Click "+ New user flow"
   - Select "Password reset"
   - Choose "Recommended" version
   - Name: `B2C_1_password_reset`

2. **Configure**

   - Identity provider: Email signup
   - Return claims: Email, Display Name, Object ID
   - Multi-factor: Email (optional)

3. **Create**

### Profile Edit Flow

1. **Create User Flow**

   - Click "+ New user flow"
   - Select "Profile editing"
   - Choose "Recommended" version
   - Name: `B2C_1_profile_edit`

2. **Configure**

   - Identity provider: Local account
   - User attributes: Display Name, Given Name, Surname
   - Return claims: Same as collected
   - Multi-factor: Off

3. **Create**

## Step 3: Register Applications

### Backend API Application

1. **Register Application**

   - Azure AD B2C → App registrations
   - Click "+ New registration"
   - Fill in details:
     - **Name**: `UserService API`
     - **Supported account types**: Accounts in any identity provider or organizational directory (for authenticating users with user flows)
     - **Redirect URI**: Leave blank for API
   - Click "Register"

2. **Configure API Scopes**

   - Left menu → Expose an API
   - Click "Add a scope"
   - **Application ID URI**: `https://userservice.onmicrosoft.com/api` (or use default)
   - Click "Save and continue"

   - **Add Scope**:
     - **Scope name**: `user.read`
     - **Admin consent display name**: Read user profile
     - **Admin consent description**: Allows the app to read the signed-in user's profile
     - **User consent display name**: Read your profile
     - **User consent description**: Allows the app to read your profile information
     - **State**: Enabled
   - Click "Add scope"

   - **Add Another Scope**:
     - **Scope name**: `user.write`
     - **Display names**: Write user profile / Allows the app to update your profile
     - **State**: Enabled
   - Click "Add scope"

3. **Note Application ID**
   - Overview → Application (client) ID
   - Save this for configuration: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`

### Frontend SPA Application

1. **Register Application**

   - App registrations → "+ New registration"
   - Fill in details:
     - **Name**: `UserService SPA`
     - **Supported account types**: Accounts in any identity provider or organizational directory (for authenticating users with user flows)
     - **Redirect URI**:
       - Platform: Single-page application (SPA)
       - URI: `http://localhost:3000` (dev)
   - Click "Register"

2. **Configure Authentication**

   - Left menu → Authentication
   - Under "Single-page application" section:

     - Add redirect URIs:
       - `http://localhost:3000`
       - `http://localhost:5173` (Vite)
       - `https://your-domain.com` (production)

   - **Implicit grant and hybrid flows**:

     - ✅ Access tokens (used for implicit flows)
     - ✅ ID tokens (used for implicit and hybrid flows)

   - **Allow public client flows**: No
   - Click "Save"

3. **Configure API Permissions**

   - Left menu → API permissions
   - Click "+ Add a permission"
   - Select "My APIs" tab
   - Select "UserService API"
   - Select delegated permissions:
     - ✅ user.read
     - ✅ user.write
   - Click "Add permissions"

   - **Grant Admin Consent** (optional for dev):
     - Click "Grant admin consent for [tenant]"
     - Click "Yes"

4. **Note Application ID**
   - Overview → Application (client) ID
   - Save this for frontend configuration

## Step 4: Configure Custom Domain (Optional)

### Prerequisites

- Custom domain (e.g., login.yourdomain.com)
- SSL certificate

### Steps

1. **Add Custom Domain**

   - Azure AD B2C → Company branding → Custom domains
   - Click "+ Add custom domain"
   - Enter domain: `login.yourdomain.com`
   - Follow verification steps

2. **Configure DNS**

   - Add CNAME record:
     - Name: `login`
     - Value: `yourtenantname.b2clogin.com`
     - TTL: 3600

3. **Upload Certificate**
   - Upload SSL certificate for custom domain
   - Wait for validation

## Step 5: Application Configuration

### Backend API (appsettings.json)

```json
{
  "AzureAdB2C": {
    "Instance": "https://userservice.b2clogin.com",
    "Domain": "userservice.onmicrosoft.com",
    "TenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "ClientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "SignUpSignInPolicyId": "B2C_1_signup_signin",
    "ResetPasswordPolicyId": "B2C_1_password_reset",
    "EditProfilePolicyId": "B2C_1_profile_edit",
    "Scopes": "https://userservice.onmicrosoft.com/api/user.read https://userservice.onmicrosoft.com/api/user.write"
  }
}
```

### Frontend SPA (.env)

```env
VITE_AZURE_AD_B2C_AUTHORITY=https://userservice.b2clogin.com/userservice.onmicrosoft.com/B2C_1_signup_signin
VITE_AZURE_AD_B2C_CLIENT_ID=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
VITE_AZURE_AD_B2C_KNOWN_AUTHORITIES=userservice.b2clogin.com
VITE_AZURE_AD_B2C_REDIRECT_URI=http://localhost:3000
VITE_API_SCOPES=https://userservice.onmicrosoft.com/api/user.read https://userservice.onmicrosoft.com/api/user.write
```

## Step 6: Testing User Flows

### Test Sign-Up and Sign-In

1. **Run User Flow**

   - Azure AD B2C → User flows
   - Select `B2C_1_signup_signin`
   - Click "Run user flow"
   - Select application: UserService SPA
   - Reply URL: `http://localhost:3000`
   - Click "Run user flow"

2. **Test Sign-Up**

   - Click "Sign up now"
   - Enter email and password
   - Fill in profile information
   - Complete sign-up
   - Verify token returned

3. **Test Sign-In**
   - Enter credentials
   - Click "Sign in"
   - Verify token returned
   - Decode JWT at https://jwt.ms to verify claims

### Test Password Reset

1. **Run User Flow**
   - Select `B2C_1_password_reset`
   - Click "Run user flow"
   - Enter email
   - Receive verification code
   - Set new password
   - Verify success

## Step 7: Token Validation in API

### JWT Validation Middleware

The API needs to validate tokens from Entra External ID. Update `appsettings.json`:

```json
{
  "Authentication": {
    "Schemes": {
      "Bearer": {
        "ValidAudiences": ["xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"],
        "ValidIssuers": [
          "https://userservice.b2clogin.com/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx/v2.0/"
        ]
      }
    }
  },
  "AzureAdB2C": {
    "Instance": "https://userservice.b2clogin.com",
    "Domain": "userservice.onmicrosoft.com",
    "TenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "ClientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    "SignUpSignInPolicyId": "B2C_1_signup_signin"
  }
}
```

## Step 8: Social Identity Providers (Optional)

### Configure Google

1. **Get Google OAuth Credentials**

   - Go to https://console.cloud.google.com
   - Create project or select existing
   - Enable Google+ API
   - Create OAuth 2.0 credentials
   - Add authorized redirect URI: `https://userservice.b2clogin.com/userservice.onmicrosoft.com/oauth2/authresp`
   - Note Client ID and Client Secret

2. **Add to Azure AD B2C**

   - Azure AD B2C → Identity providers
   - Click "+ New OpenID Connect provider"
   - Name: `Google`
   - Metadata URL: `https://accounts.google.com/.well-known/openid-configuration`
   - Client ID: [from Google]
   - Client secret: [from Google]
   - Scope: `openid email profile`
   - Response type: `code`
   - Click "Save"

3. **Update User Flow**
   - User flows → B2C_1_signup_signin
   - Identity providers → Select Google
   - Save

### Configure Microsoft Account

1. **Register Application**

   - Go to https://portal.azure.com
   - Azure AD → App registrations
   - New registration
   - Redirect URI: `https://userservice.b2clogin.com/userservice.onmicrosoft.com/oauth2/authresp`
   - Note Application ID and create client secret

2. **Add to Azure AD B2C**
   - Azure AD B2C → Identity providers
   - Click "+ New OpenID Connect provider"
   - Name: `Microsoft Account`
   - Client ID: [from registration]
   - Client secret: [from registration]
   - Scope: `openid email profile`
   - Click "Save"

## Step 9: Custom Branding

### Upload Custom UI

1. **Create Custom HTML**

   - Create branded login page
   - Host on Azure Blob Storage with CORS enabled
   - Example URL: `https://yourstorage.blob.core.windows.net/b2c/unified.html`

2. **Configure User Flow**
   - User flows → B2C_1_signup_signin
   - Page layouts → Select page
   - Custom page URI: [your blob URL]
   - Save

### Company Branding

1. **Configure Branding**
   - Azure AD B2C → Company branding
   - Upload:
     - Logo
     - Background image
     - Banner logo
   - Set colors and themes
   - Save

## Step 10: Monitoring and Logging

### Enable Diagnostics

1. **Configure Diagnostic Settings**

   - Azure AD B2C → Monitoring → Diagnostic settings
   - Add diagnostic setting
   - Name: `B2C-Diagnostics`
   - Logs:
     - ✅ AuditLogs
     - ✅ SignInLogs
   - Destination:
     - ✅ Send to Log Analytics workspace
     - ✅ Archive to storage account
   - Save

2. **Query Logs**

   ```kusto
   // Failed sign-ins
   SigninLogs
   | where TimeGenerated > ago(24h)
   | where ResultType != "0"
   | project TimeGenerated, UserPrincipalName, AppDisplayName, ResultType, ResultDescription
   | order by TimeGenerated desc

   // Sign-ins by application
   SigninLogs
   | where TimeGenerated > ago(7d)
   | summarize count() by AppDisplayName
   | order by count_ desc
   ```

## Terraform Configuration

Create Terraform scripts to automate B2C setup (some resources require manual steps):

```hcl
# Note: Azure AD B2C Terraform support is limited
# Some configurations must be done via Portal or Microsoft Graph API

# terraform/modules/b2c/main.tf

resource "azurerm_aadb2c_directory" "main" {
  domain_name             = var.b2c_domain_name
  display_name            = var.b2c_display_name
  country_code            = var.country_code
  data_residency_location = var.data_residency_location
  resource_group_name     = var.resource_group_name
  sku_name                = var.sku_name
}

# Application registrations via azuread provider
resource "azuread_application" "api" {
  display_name = "UserService API"

  identifier_uris = [
    "https://${var.b2c_domain_name}.onmicrosoft.com/api"
  ]

  api {
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
  }
}

resource "azuread_application" "spa" {
  display_name = "UserService SPA"

  single_page_application {
    redirect_uris = var.spa_redirect_uris
  }

  required_resource_access {
    resource_app_id = azuread_application.api.application_id

    resource_access {
      id   = random_uuid.user_read_scope.result
      type = "Scope"
    }

    resource_access {
      id   = random_uuid.user_write_scope.result
      type = "Scope"
    }
  }
}

resource "random_uuid" "user_read_scope" {}
resource "random_uuid" "user_write_scope" {}
```

## Security Best Practices

1. **Token Validation**

   - Always validate issuer, audience, and expiration
   - Verify signature using public keys from metadata endpoint
   - Check for required claims

2. **HTTPS Only**

   - Use HTTPS for all endpoints
   - Configure HSTS headers
   - Disable insecure protocols

3. **CORS Configuration**

   - Whitelist specific origins
   - Don't use wildcard (\*) in production
   - Enable credentials for authenticated requests

4. **Rate Limiting**

   - Implement rate limiting on authentication endpoints
   - Monitor for brute force attacks
   - Use Azure APIM for additional protection

5. **MFA Enforcement**

   - Enable MFA for sensitive operations
   - Consider risk-based authentication
   - Use conditional access policies

6. **Regular Updates**
   - Keep user flows updated
   - Review security recommendations
   - Monitor Azure security center

## Troubleshooting

### Issue: CORS Error in SPA

**Solution**:

1. Verify redirect URI is registered in SPA app registration
2. Check CORS settings in API
3. Ensure correct authority URL in SPA config

### Issue: Invalid Token

**Solution**:

1. Verify audience claim matches API client ID
2. Check issuer URL format: `https://{tenant}.b2clogin.com/{tenantId}/v2.0/`
3. Ensure token not expired
4. Verify signing keys are current

### Issue: User Flow Not Found

**Solution**:

1. Verify user flow name matches exactly (case-sensitive)
2. Ensure user flow is created and published
3. Check authority URL includes policy name

## Next Steps

After completing Entra External ID setup:

1. ✅ Test all user flows (sign-up, sign-in, password reset)
2. ✅ Verify token validation in API
3. ✅ Configure monitoring and alerts
4. ➡️ Proceed to Task 10: Configure Azure APIM
5. ➡️ Implement frontend with MSAL.js

## Resources

- [Microsoft Entra External ID Documentation](https://learn.microsoft.com/en-us/azure/active-directory-b2c/)
- [User Flow Documentation](https://learn.microsoft.com/en-us/azure/active-directory-b2c/user-flow-overview)
- [Custom Policies](https://learn.microsoft.com/en-us/azure/active-directory-b2c/custom-policy-overview)
- [MSAL.js Documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-js-initializing-client-applications)
- [Token Reference](https://learn.microsoft.com/en-us/azure/active-directory-b2c/tokens-overview)
