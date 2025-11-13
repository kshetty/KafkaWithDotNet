#!/bin/bash

# Azure API Management Setup Script
# This script automates the creation and configuration of Azure APIM for UserService

set -e

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_header() {
    echo -e "\n${BLUE}═══════════════════════════════════════════════════════${NC}"
    echo -e "${BLUE}  $1${NC}"
    echo -e "${BLUE}═══════════════════════════════════════════════════════${NC}\n"
}

# Default values
RESOURCE_GROUP="rg-userservice-dev"
LOCATION="eastus"
APIM_NAME="apim-userservice-dev"
PUBLISHER_NAME="UserService"
PUBLISHER_EMAIL="admin@userservice.dev"
SKU="Developer"
BACKEND_URL=""
B2C_TENANT_ID=""
B2C_DOMAIN=""
B2C_CLIENT_ID=""
ENVIRONMENT="dev"

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -g|--resource-group)
            RESOURCE_GROUP="$2"
            shift 2
            ;;
        -l|--location)
            LOCATION="$2"
            shift 2
            ;;
        -n|--name)
            APIM_NAME="$2"
            shift 2
            ;;
        -s|--sku)
            SKU="$2"
            shift 2
            ;;
        -b|--backend-url)
            BACKEND_URL="$2"
            shift 2
            ;;
        --b2c-tenant-id)
            B2C_TENANT_ID="$2"
            shift 2
            ;;
        --b2c-domain)
            B2C_DOMAIN="$2"
            shift 2
            ;;
        --b2c-client-id)
            B2C_CLIENT_ID="$2"
            shift 2
            ;;
        -e|--environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        -h|--help)
            echo "Azure APIM Setup Script"
            echo ""
            echo "Usage: $0 [options]"
            echo ""
            echo "Options:"
            echo "  -g, --resource-group    Resource group name (default: rg-userservice-dev)"
            echo "  -l, --location          Azure region (default: eastus)"
            echo "  -n, --name              APIM instance name (default: apim-userservice-dev)"
            echo "  -s, --sku               SKU tier (Developer, Basic, Standard, Premium) (default: Developer)"
            echo "  -b, --backend-url       Backend API URL (required)"
            echo "  --b2c-tenant-id         Azure AD B2C Tenant ID (required)"
            echo "  --b2c-domain            Azure AD B2C domain (required)"
            echo "  --b2c-client-id         Azure AD B2C API Client ID (required)"
            echo "  -e, --environment       Environment (dev, staging, prod) (default: dev)"
            echo "  -h, --help              Show this help message"
            echo ""
            echo "Example:"
            echo "  $0 -b https://userservice-dev.azurewebsites.net \\"
            echo "     --b2c-tenant-id xxx-xxx --b2c-domain userservice.b2clogin.com \\"
            echo "     --b2c-client-id yyy-yyy"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            echo "Use -h or --help for usage information"
            exit 1
            ;;
    esac
done

# Validate required parameters
if [ -z "$BACKEND_URL" ]; then
    print_error "Backend URL is required. Use -b or --backend-url"
    exit 1
fi

if [ -z "$B2C_TENANT_ID" ] || [ -z "$B2C_DOMAIN" ] || [ -z "$B2C_CLIENT_ID" ]; then
    print_error "Azure AD B2C configuration is required"
    echo "  --b2c-tenant-id: B2C Tenant ID"
    echo "  --b2c-domain: B2C domain (e.g., userservice.b2clogin.com)"
    echo "  --b2c-client-id: API Client ID"
    exit 1
fi

print_header "Azure API Management Setup"

print_info "Configuration:"
echo "  Resource Group: $RESOURCE_GROUP"
echo "  Location: $LOCATION"
echo "  APIM Name: $APIM_NAME"
echo "  SKU: $SKU"
echo "  Backend URL: $BACKEND_URL"
echo "  B2C Domain: $B2C_DOMAIN"
echo "  Environment: $ENVIRONMENT"
echo ""

# Check if Azure CLI is installed
print_info "Checking Azure CLI..."
if ! command -v az &> /dev/null; then
    print_error "Azure CLI is not installed. Please install it first."
    echo "Visit: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi
print_success "Azure CLI is installed"

# Check if logged in
print_info "Checking Azure login status..."
if ! az account show &> /dev/null; then
    print_warning "Not logged in to Azure. Starting login..."
    az login
fi

SUBSCRIPTION_NAME=$(az account show --query name -o tsv)
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
print_success "Logged in to subscription: $SUBSCRIPTION_NAME ($SUBSCRIPTION_ID)"

# Check if resource group exists
print_info "Checking resource group..."
if az group show --name "$RESOURCE_GROUP" &> /dev/null; then
    print_success "Resource group '$RESOURCE_GROUP' exists"
else
    print_warning "Resource group '$RESOURCE_GROUP' does not exist. Creating..."
    az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
    print_success "Resource group created"
fi

# Create APIM instance
print_header "Creating API Management Instance"
print_warning "This will take 30-45 minutes. Please be patient..."

print_info "Creating APIM instance: $APIM_NAME..."
az apim create \
    --name "$APIM_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --location "$LOCATION" \
    --publisher-name "$PUBLISHER_NAME" \
    --publisher-email "$PUBLISHER_EMAIL" \
    --sku-name "$SKU" \
    --enable-managed-identity true \
    --no-wait

print_success "APIM creation initiated (running in background)"
print_info "Waiting for APIM to be provisioned..."

# Wait for APIM to be ready
while true; do
    STATE=$(az apim show --name "$APIM_NAME" --resource-group "$RESOURCE_GROUP" --query "provisioningState" -o tsv 2>/dev/null || echo "NotFound")
    
    if [ "$STATE" == "Succeeded" ]; then
        print_success "APIM instance provisioned successfully"
        break
    elif [ "$STATE" == "Failed" ]; then
        print_error "APIM provisioning failed"
        exit 1
    else
        echo -ne "\rProvisioning state: $STATE (checking again in 60 seconds)...   "
        sleep 60
    fi
done

# Get APIM details
GATEWAY_URL=$(az apim show --name "$APIM_NAME" --resource-group "$RESOURCE_GROUP" --query "gatewayUrl" -o tsv)
print_info "Gateway URL: $GATEWAY_URL"

# Create named values
print_header "Configuring Named Values"

print_info "Setting backend-url..."
az apim nv create \
    --service-name "$APIM_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --named-value-id "backend-url" \
    --display-name "backend-url" \
    --value "$BACKEND_URL"

print_info "Setting B2C configuration..."
az apim nv create \
    --service-name "$APIM_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --named-value-id "b2c-tenant-id" \
    --display-name "b2c-tenant-id" \
    --value "$B2C_TENANT_ID" \
    --secret true

az apim nv create \
    --service-name "$APIM_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --named-value-id "b2c-domain" \
    --display-name "b2c-domain" \
    --value "$B2C_DOMAIN"

az apim nv create \
    --service-name "$APIM_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --named-value-id "b2c-client-id" \
    --display-name "b2c-client-id" \
    --value "$B2C_CLIENT_ID" \
    --secret true

print_success "Named values configured"

# Import UserService API
print_header "Importing UserService API"

print_info "Importing API from OpenAPI spec..."
az apim api import \
    --service-name "$APIM_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --path "userservice" \
    --api-id "userservice-api" \
    --display-name "UserService API" \
    --service-url "$BACKEND_URL" \
    --specification-url "${BACKEND_URL}/swagger/v1/swagger.json" \
    --specification-format "OpenApi" \
    --protocols "https" \
    --subscription-required true || {
        print_warning "OpenAPI import failed. API may need to be configured manually."
    }

print_success "API import completed"

# Create Products
print_header "Creating Products"

print_info "Creating Starter product..."
az apim product create \
    --service-name "$APIM_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --product-id "starter" \
    --product-name "Starter" \
    --description "Starter tier with rate limits (50 calls/min, 5000 calls/day)" \
    --subscription-required true \
    --approval-required false \
    --subscriptions-limit 10 \
    --state "published"

print_info "Creating Unlimited product..."
az apim product create \
    --service-name "$APIM_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --product-id "unlimited" \
    --product-name "Unlimited" \
    --description "Unlimited tier for premium users" \
    --subscription-required true \
    --approval-required true \
    --state "published"

print_success "Products created"

# Associate API with products
print_info "Associating API with products..."
az apim product api add \
    --service-name "$APIM_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --product-id "starter" \
    --api-id "userservice-api"

az apim product api add \
    --service-name "$APIM_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --product-id "unlimited" \
    --api-id "userservice-api"

print_success "API associated with products"

# Create development subscription
if [ "$ENVIRONMENT" == "dev" ]; then
    print_header "Creating Development Subscription"
    
    SUBSCRIPTION_ID_APIM="dev-subscription"
    az apim product subscription create \
        --service-name "$APIM_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --product-id "starter" \
        --subscription-id "$SUBSCRIPTION_ID_APIM" \
        --name "Development Subscription" \
        --state "active"
    
    PRIMARY_KEY=$(az apim product subscription show \
        --service-name "$APIM_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --product-id "starter" \
        --subscription-id "$SUBSCRIPTION_ID_APIM" \
        --query "primaryKey" -o tsv)
    
    SECONDARY_KEY=$(az apim product subscription show \
        --service-name "$APIM_NAME" \
        --resource-group "$RESOURCE_GROUP" \
        --product-id "starter" \
        --subscription-id "$SUBSCRIPTION_ID_APIM" \
        --query "secondaryKey" -o tsv)
    
    print_success "Development subscription created"
fi

# Summary
print_header "Setup Complete!"

echo "APIM Configuration:"
echo "  Name: $APIM_NAME"
echo "  Gateway URL: $GATEWAY_URL"
echo "  Developer Portal: https://${APIM_NAME}.developer.azure-api.net"
echo "  Management URL: https://${APIM_NAME}.management.azure-api.net"
echo ""
echo "API Endpoints:"
echo "  Base: ${GATEWAY_URL}/userservice"
echo "  Health: ${GATEWAY_URL}/userservice/api/v1/health"
echo "  Users: ${GATEWAY_URL}/userservice/api/v1/users"
echo ""

if [ "$ENVIRONMENT" == "dev" ]; then
    echo "Development Subscription:"
    echo "  Primary Key: $PRIMARY_KEY"
    echo "  Secondary Key: $SECONDARY_KEY"
    echo ""
    echo "Test Command:"
    echo "  curl -X GET '${GATEWAY_URL}/userservice/api/v1/health' \\"
    echo "       -H 'Ocp-Apim-Subscription-Key: $PRIMARY_KEY'"
    echo ""
    
    # Save configuration to file
    CONFIG_FILE="apim-config-${ENVIRONMENT}.json"
    cat > "$CONFIG_FILE" << EOF
{
  "apimName": "$APIM_NAME",
  "gatewayUrl": "$GATEWAY_URL",
  "developerPortal": "https://${APIM_NAME}.developer.azure-api.net",
  "apiBasePath": "/userservice",
  "subscriptionKey": "$PRIMARY_KEY",
  "secondaryKey": "$SECONDARY_KEY",
  "environment": "$ENVIRONMENT"
}
EOF
    
    print_success "Configuration saved to: $CONFIG_FILE"
fi

print_info "Next Steps:"
echo "  1. Visit the Developer Portal to explore APIs"
echo "  2. Test the API endpoints using the subscription key"
echo "  3. Configure custom policies if needed"
echo "  4. Set up Application Insights monitoring"
echo "  5. Update frontend configuration with APIM URLs"

print_success "APIM setup completed successfully!"
