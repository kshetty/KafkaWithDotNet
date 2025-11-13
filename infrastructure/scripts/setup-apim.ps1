# Azure API Management Setup Script (PowerShell)
# This script automates the creation and configuration of Azure APIM for UserService

[CmdletBinding()]
param(
  [Parameter(Mandatory = $false)]
  [string]$ResourceGroup = "rg-userservice-dev",
    
  [Parameter(Mandatory = $false)]
  [string]$Location = "eastus",
    
  [Parameter(Mandatory = $false)]
  [string]$ApimName = "apim-userservice-dev",
    
  [Parameter(Mandatory = $false)]
  [string]$PublisherName = "UserService",
    
  [Parameter(Mandatory = $false)]
  [string]$PublisherEmail = "admin@userservice.dev",
    
  [Parameter(Mandatory = $false)]
  [ValidateSet("Developer", "Basic", "Standard", "Premium")]
  [string]$Sku = "Developer",
    
  [Parameter(Mandatory = $true)]
  [string]$BackendUrl,
    
  [Parameter(Mandatory = $true)]
  [string]$B2cTenantId,
    
  [Parameter(Mandatory = $true)]
  [string]$B2cDomain,
    
  [Parameter(Mandatory = $true)]
  [string]$B2cClientId,
    
  [Parameter(Mandatory = $false)]
  [ValidateSet("dev", "staging", "prod")]
  [string]$Environment = "dev"
)

# Color output functions
function Write-Success {
  param([string]$Message)
  Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Info {
  param([string]$Message)
  Write-Host "ℹ $Message" -ForegroundColor Blue
}

function Write-Warning2 {
  param([string]$Message)
  Write-Host "⚠ $Message" -ForegroundColor Yellow
}

function Write-ErrorMsg {
  param([string]$Message)
  Write-Host "✗ $Message" -ForegroundColor Red
}

function Write-Header {
  param([string]$Message)
  Write-Host ""
  Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Blue
  Write-Host "  $Message" -ForegroundColor Blue
  Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Blue
  Write-Host ""
}

# Main script
Write-Header "Azure API Management Setup"

Write-Info "Configuration:"
Write-Host "  Resource Group: $ResourceGroup"
Write-Host "  Location: $Location"
Write-Host "  APIM Name: $ApimName"
Write-Host "  SKU: $Sku"
Write-Host "  Backend URL: $BackendUrl"
Write-Host "  B2C Domain: $B2cDomain"
Write-Host "  Environment: $Environment"
Write-Host ""

# Check if Azure CLI is installed
Write-Info "Checking Azure CLI..."
try {
  $azVersion = az version | ConvertFrom-Json
  Write-Success "Azure CLI is installed (version $($azVersion.'azure-cli'))"
}
catch {
  Write-ErrorMsg "Azure CLI is not installed. Please install it first."
  Write-Host "Visit: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
  exit 1
}

# Check if logged in
Write-Info "Checking Azure login status..."
try {
  $account = az account show | ConvertFrom-Json
  Write-Success "Logged in to subscription: $($account.name) ($($account.id))"
}
catch {
  Write-Warning2 "Not logged in to Azure. Starting login..."
  az login
  $account = az account show | ConvertFrom-Json
  Write-Success "Logged in to subscription: $($account.name)"
}

# Check if resource group exists
Write-Info "Checking resource group..."
try {
  az group show --name $ResourceGroup | Out-Null
  Write-Success "Resource group '$ResourceGroup' exists"
}
catch {
  Write-Warning2 "Resource group '$ResourceGroup' does not exist. Creating..."
  az group create --name $ResourceGroup --location $Location | Out-Null
  Write-Success "Resource group created"
}

# Create APIM instance
Write-Header "Creating API Management Instance"
Write-Warning2 "This will take 30-45 minutes. Please be patient..."

Write-Info "Creating APIM instance: $ApimName..."
az apim create `
  --name $ApimName `
  --resource-group $ResourceGroup `
  --location $Location `
  --publisher-name $PublisherName `
  --publisher-email $PublisherEmail `
  --sku-name $Sku `
  --enable-managed-identity true `
  --no-wait

Write-Success "APIM creation initiated (running in background)"
Write-Info "Waiting for APIM to be provisioned..."

# Wait for APIM to be ready
$maxAttempts = 60
$attempt = 0
while ($attempt -lt $maxAttempts) {
  try {
    $apim = az apim show --name $ApimName --resource-group $ResourceGroup | ConvertFrom-Json
    $state = $apim.provisioningState
        
    if ($state -eq "Succeeded") {
      Write-Success "APIM instance provisioned successfully"
      break
    }
    elseif ($state -eq "Failed") {
      Write-ErrorMsg "APIM provisioning failed"
      exit 1
    }
    else {
      Write-Host "`rProvisioning state: $state (checking again in 60 seconds)...   " -NoNewline
      Start-Sleep -Seconds 60
      $attempt++
    }
  }
  catch {
    Write-Host "`rWaiting for APIM to appear (attempt $attempt/$maxAttempts)...   " -NoNewline
    Start-Sleep -Seconds 60
    $attempt++
  }
}

Write-Host ""

if ($attempt -ge $maxAttempts) {
  Write-ErrorMsg "Timeout waiting for APIM provisioning"
  exit 1
}

# Get APIM details
$apim = az apim show --name $ApimName --resource-group $ResourceGroup | ConvertFrom-Json
$gatewayUrl = $apim.gatewayUrl
Write-Info "Gateway URL: $gatewayUrl"

# Create named values
Write-Header "Configuring Named Values"

Write-Info "Setting backend-url..."
az apim nv create `
  --service-name $ApimName `
  --resource-group $ResourceGroup `
  --named-value-id "backend-url" `
  --display-name "backend-url" `
  --value $BackendUrl | Out-Null

Write-Info "Setting B2C configuration..."
az apim nv create `
  --service-name $ApimName `
  --resource-group $ResourceGroup `
  --named-value-id "b2c-tenant-id" `
  --display-name "b2c-tenant-id" `
  --value $B2cTenantId `
  --secret true | Out-Null

az apim nv create `
  --service-name $ApimName `
  --resource-group $ResourceGroup `
  --named-value-id "b2c-domain" `
  --display-name "b2c-domain" `
  --value $B2cDomain | Out-Null

az apim nv create `
  --service-name $ApimName `
  --resource-group $ResourceGroup `
  --named-value-id "b2c-client-id" `
  --display-name "b2c-client-id" `
  --value $B2cClientId `
  --secret true | Out-Null

Write-Success "Named values configured"

# Import UserService API
Write-Header "Importing UserService API"

Write-Info "Importing API from OpenAPI spec..."
try {
  az apim api import `
    --service-name $ApimName `
    --resource-group $ResourceGroup `
    --path "userservice" `
    --api-id "userservice-api" `
    --display-name "UserService API" `
    --service-url $BackendUrl `
    --specification-url "$BackendUrl/swagger/v1/swagger.json" `
    --specification-format "OpenApi" `
    --protocols "https" `
    --subscription-required true | Out-Null
  Write-Success "API import completed"
}
catch {
  Write-Warning2 "OpenAPI import failed. API may need to be configured manually."
}

# Create Products
Write-Header "Creating Products"

Write-Info "Creating Starter product..."
az apim product create `
  --service-name $ApimName `
  --resource-group $ResourceGroup `
  --product-id "starter" `
  --product-name "Starter" `
  --description "Starter tier with rate limits (50 calls/min, 5000 calls/day)" `
  --subscription-required true `
  --approval-required false `
  --subscriptions-limit 10 `
  --state "published" | Out-Null

Write-Info "Creating Unlimited product..."
az apim product create `
  --service-name $ApimName `
  --resource-group $ResourceGroup `
  --product-id "unlimited" `
  --product-name "Unlimited" `
  --description "Unlimited tier for premium users" `
  --subscription-required true `
  --approval-required true `
  --state "published" | Out-Null

Write-Success "Products created"

# Associate API with products
Write-Info "Associating API with products..."
az apim product api add `
  --service-name $ApimName `
  --resource-group $ResourceGroup `
  --product-id "starter" `
  --api-id "userservice-api" | Out-Null

az apim product api add `
  --service-name $ApimName `
  --resource-group $ResourceGroup `
  --product-id "unlimited" `
  --api-id "userservice-api" | Out-Null

Write-Success "API associated with products"

# Create development subscription
if ($Environment -eq "dev") {
  Write-Header "Creating Development Subscription"
    
  $subscriptionId = "dev-subscription"
  az apim product subscription create `
    --service-name $ApimName `
    --resource-group $ResourceGroup `
    --product-id "starter" `
    --subscription-id $subscriptionId `
    --name "Development Subscription" `
    --state "active" | Out-Null
    
  $subscription = az apim product subscription show `
    --service-name $ApimName `
    --resource-group $ResourceGroup `
    --product-id "starter" `
    --subscription-id $subscriptionId | ConvertFrom-Json
    
  $primaryKey = $subscription.primaryKey
  $secondaryKey = $subscription.secondaryKey
    
  Write-Success "Development subscription created"
}

# Summary
Write-Header "Setup Complete!"

Write-Host "APIM Configuration:"
Write-Host "  Name: $ApimName"
Write-Host "  Gateway URL: $gatewayUrl"
Write-Host "  Developer Portal: https://$ApimName.developer.azure-api.net"
Write-Host "  Management URL: https://$ApimName.management.azure-api.net"
Write-Host ""
Write-Host "API Endpoints:"
Write-Host "  Base: $gatewayUrl/userservice"
Write-Host "  Health: $gatewayUrl/userservice/api/v1/health"
Write-Host "  Users: $gatewayUrl/userservice/api/v1/users"
Write-Host ""

if ($Environment -eq "dev") {
  Write-Host "Development Subscription:"
  Write-Host "  Primary Key: $primaryKey"
  Write-Host "  Secondary Key: $secondaryKey"
  Write-Host ""
  Write-Host "Test Command:"
  Write-Host "  curl -X GET '$gatewayUrl/userservice/api/v1/health' \"
  Write-Host "       -H 'Ocp-Apim-Subscription-Key: $primaryKey'"
  Write-Host ""
    
  # Save configuration to file
  $configFile = "apim-config-$Environment.json"
  $config = @{
    apimName        = $ApimName
    gatewayUrl      = $gatewayUrl
    developerPortal = "https://$ApimName.developer.azure-api.net"
    apiBasePath     = "/userservice"
    subscriptionKey = $primaryKey
    secondaryKey    = $secondaryKey
    environment     = $Environment
  }
    
  $config | ConvertTo-Json | Set-Content -Path $configFile
  Write-Success "Configuration saved to: $configFile"
}

Write-Info "Next Steps:"
Write-Host "  1. Visit the Developer Portal to explore APIs"
Write-Host "  2. Test the API endpoints using the subscription key"
Write-Host "  3. Configure custom policies if needed"
Write-Host "  4. Set up Application Insights monitoring"
Write-Host "  5. Update frontend configuration with APIM URLs"

Write-Success "APIM setup completed successfully!"
