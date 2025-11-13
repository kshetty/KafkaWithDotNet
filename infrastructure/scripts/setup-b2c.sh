#!/bin/bash

# Azure AD B2C Setup Script for UserService
# This script automates the creation and configuration of Azure AD B2C

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Print functions
print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_info() {
    echo -e "${CYAN}ℹ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_header() {
    echo -e "${CYAN}═══════════════════════════════════════════════════════════${NC}"
    echo -e "${CYAN}  $1${NC}"
    echo -e "${CYAN}═══════════════════════════════════════════════════════════${NC}"
}

# Parse arguments
TENANT_NAME=""
RESOURCE_GROUP=""
LOCATION="eastus"
ENVIRONMENT="dev"

while [[ $# -gt 0 ]]; do
    case $1 in
        -t|--tenant)
            TENANT_NAME="$2"
            shift 2
            ;;
        -g|--resource-group)
            RESOURCE_GROUP="$2"
            shift 2
            ;;
        -l|--location)
            LOCATION="$2"
            shift 2
            ;;
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -h|--help)
            echo "Usage: $0 -t TENANT_NAME -g RESOURCE_GROUP [-l LOCATION] [-e ENVIRONMENT]"
            echo ""
            echo "Options:"
            echo "  -t, --tenant           B2C tenant name (e.g., 'userservice')"
            echo "  -g, --resource-group   Resource group name"
            echo "  -l, --location         Azure region (default: eastus)"
            echo "  -e, --environment      Environment (dev, staging, prod) (default: dev)"
            echo "  -h, --help            Show this help message"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Validate required parameters
if [ -z "$TENANT_NAME" ] || [ -z "$RESOURCE_GROUP" ]; then
    print_error "Tenant name and resource group are required"
    echo "Usage: $0 -t TENANT_NAME -g RESOURCE_GROUP [-l LOCATION] [-e ENVIRONMENT]"
    exit 1
fi

print_header "Azure AD B2C Setup for UserService"
echo ""

# Check if Azure CLI is installed
print_info "Checking Azure CLI installation..."
if ! command -v az &> /dev/null; then
    print_error "Azure CLI is not installed"
    echo "Please install from: https://aka.ms/installazurecli"
    exit 1
fi
print_success "Azure CLI is installed"

# Check if logged in to Azure
print_info "Checking Azure login status..."
if ! az account show &> /dev/null; then
    print_warning "Not logged in to Azure. Starting login process..."
    az login
fi
print_success "Logged in to Azure"

# Get current subscription
SUBSCRIPTION_INFO=$(az account show)
SUBSCRIPTION_ID=$(echo $SUBSCRIPTION_INFO | jq -r '.id')
SUBSCRIPTION_NAME=$(echo $SUBSCRIPTION_INFO | jq -r '.name')
print_info "Using subscription: $SUBSCRIPTION_NAME ($SUBSCRIPTION_ID)"

# Create resource group if it doesn't exist
print_info "Checking if resource group exists..."
if ! az group exists --name "$RESOURCE_GROUP" | grep -q "true"; then
    print_info "Creating resource group: $RESOURCE_GROUP"
    az group create --name "$RESOURCE_GROUP" --location "$LOCATION" > /dev/null
    print_success "Resource group created"
else
    print_success "Resource group already exists"
fi

# Note about manual B2C tenant creation
echo ""
print_warning "Azure AD B2C tenant must be created manually through Azure Portal"
print_info "Please follow these steps:"
echo ""
echo "1. Go to https://portal.azure.com"
echo "2. Search for 'Azure Active Directory B2C'"
echo "3. Click 'Create'"
echo "4. Select 'Create a new Azure AD B2C Tenant'"
echo "5. Enter details:"
echo "   - Organization name: UserService B2C - $ENVIRONMENT"
echo "   - Initial domain name: $TENANT_NAME"
echo "   - Country/Region: United States (or your preference)"
echo "   - Subscription: $SUBSCRIPTION_NAME"
echo "   - Resource group: $RESOURCE_GROUP"
echo "6. Click 'Review + create' and then 'Create'"
echo "7. Wait 2-3 minutes for tenant creation"
echo "8. Switch to the B2C tenant using the directory switcher"
echo ""

read -p "Have you created the B2C tenant? (y/n): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    print_warning "Script execution stopped. Please create the B2C tenant first."
    exit 0
fi

# Get B2C tenant domain
echo ""
read -p "Enter your B2C tenant domain (e.g., userservice.onmicrosoft.com): " B2C_DOMAIN

# Switch to B2C tenant
print_info "Switching to B2C tenant..."
az login --tenant "$B2C_DOMAIN" --allow-no-subscriptions > /dev/null
print_success "Switched to B2C tenant"

# Register API application
print_info "Registering API application..."
API_IDENTIFIER_URI="https://$B2C_DOMAIN/api"
API_APP_NAME="UserService API - $ENVIRONMENT"

API_APP_JSON=$(az ad app create \
    --display-name "$API_APP_NAME" \
    --identifier-uris "$API_IDENTIFIER_URI" \
    --sign-in-audience "AzureADandPersonalMicrosoftAccount")

API_APP_ID=$(echo $API_APP_JSON | jq -r '.appId')
print_success "API application registered: $API_APP_ID"

# Create service principal for API
print_info "Creating service principal for API..."
az ad sp create --id "$API_APP_ID" > /dev/null
print_success "API service principal created"

# Generate UUIDs for scopes
USER_READ_SCOPE_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')
USER_WRITE_SCOPE_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')

# Add OAuth scopes to API
print_info "Adding OAuth scopes to API..."
cat > api-manifest.json <<EOF
{
  "api": {
    "oauth2PermissionScopes": [
      {
        "adminConsentDescription": "Allow the application to read user profile",
        "adminConsentDisplayName": "Read user profile",
        "id": "$USER_READ_SCOPE_ID",
        "isEnabled": true,
        "type": "User",
        "userConsentDescription": "Allow the application to read your profile",
        "userConsentDisplayName": "Read your profile",
        "value": "user.read"
      },
      {
        "adminConsentDescription": "Allow the application to write user profile",
        "adminConsentDisplayName": "Write user profile",
        "id": "$USER_WRITE_SCOPE_ID",
        "isEnabled": true,
        "type": "User",
        "userConsentDescription": "Allow the application to update your profile",
        "userConsentDisplayName": "Update your profile",
        "value": "user.write"
      }
    ]
  }
}
EOF

az ad app update --id "$API_APP_ID" --set @api-manifest.json > /dev/null
rm api-manifest.json
print_success "OAuth scopes added to API"

# Register SPA application
print_info "Registering SPA application..."
SPA_APP_NAME="UserService SPA - $ENVIRONMENT"
REDIRECT_URIS="http://localhost:3000 http://localhost:5173"

SPA_APP_JSON=$(az ad app create \
    --display-name "$SPA_APP_NAME" \
    --sign-in-audience "AzureADandPersonalMicrosoftAccount" \
    --web-redirect-uris $REDIRECT_URIS \
    --enable-id-token-issuance \
    --enable-access-token-issuance)

SPA_APP_ID=$(echo $SPA_APP_JSON | jq -r '.appId')
print_success "SPA application registered: $SPA_APP_ID"

# Create service principal for SPA
print_info "Creating service principal for SPA..."
az ad sp create --id "$SPA_APP_ID" > /dev/null
print_success "SPA service principal created"

# Output configuration
echo ""
print_header "Azure AD B2C Configuration Complete"
echo ""
echo -e "${YELLOW}API Application Configuration:${NC}"
echo "  Client ID: $API_APP_ID"
echo "  Identifier URI: $API_IDENTIFIER_URI"
echo "  Scopes: user.read, user.write"
echo ""
echo -e "${YELLOW}SPA Application Configuration:${NC}"
echo "  Client ID: $SPA_APP_ID"
echo "  Redirect URIs: $REDIRECT_URIS"
echo ""
echo -e "${YELLOW}Add to appsettings.json:${NC}"
cat <<EOF
{
  "AzureAdB2C": {
    "Instance": "https://$TENANT_NAME.b2clogin.com",
    "Domain": "$B2C_DOMAIN",
    "ClientId": "$API_APP_ID",
    "SignUpSignInPolicyId": "B2C_1_signup_signin",
    "ResetPasswordPolicyId": "B2C_1_password_reset",
    "EditProfilePolicyId": "B2C_1_profile_edit"
  }
}
EOF
echo ""
echo -e "${YELLOW}Add to frontend .env:${NC}"
cat <<EOF
VITE_AZURE_AD_B2C_CLIENT_ID=$SPA_APP_ID
VITE_AZURE_AD_B2C_AUTHORITY=https://$TENANT_NAME.b2clogin.com/$B2C_DOMAIN/B2C_1_signup_signin
VITE_AZURE_AD_B2C_KNOWN_AUTHORITIES=$TENANT_NAME.b2clogin.com
VITE_API_SCOPES=$API_IDENTIFIER_URI/user.read $API_IDENTIFIER_URI/user.write
EOF
echo ""
echo -e "${GREEN}Next Steps:${NC}"
echo "1. Create user flows in Azure Portal:"
echo "   - B2C_1_signup_signin (Sign up and sign in)"
echo "   - B2C_1_password_reset (Password reset)"
echo "   - B2C_1_profile_edit (Profile editing)"
echo "2. Test user flows from Azure Portal"
echo "3. Update application configuration files with above values"
echo "4. Grant admin consent for API permissions if needed"
echo ""

# Save configuration to file
CONFIG_FILE="b2c-config-$ENVIRONMENT.json"
cat > "$CONFIG_FILE" <<EOF
{
  "tenant": {
    "name": "$TENANT_NAME",
    "domain": "$B2C_DOMAIN"
  },
  "api": {
    "clientId": "$API_APP_ID",
    "identifierUri": "$API_IDENTIFIER_URI",
    "scopes": ["user.read", "user.write"]
  },
  "spa": {
    "clientId": "$SPA_APP_ID",
    "redirectUris": ["http://localhost:3000", "http://localhost:5173"]
  },
  "environment": "$ENVIRONMENT",
  "createdDate": "$(date '+%Y-%m-%d %H:%M:%S')"
}
EOF

print_success "Configuration saved to: $CONFIG_FILE"
print_info "Keep this file secure and do not commit it to source control"
