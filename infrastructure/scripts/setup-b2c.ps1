#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Creates and configures Azure AD B2C tenant for UserService

.DESCRIPTION
    This script automates the creation and configuration of:
    - Azure AD B2C tenant
    - User flows (sign-up/sign-in, password reset, profile edit)
    - API application registration with OAuth scopes
    - SPA application registration with redirect URIs
    - Service principals and permissions

.PARAMETER TenantName
    The name of the B2C tenant (e.g., 'userservice')

.PARAMETER ResourceGroupName
    The name of the resource group

.PARAMETER Location
    The Azure region for resources

.PARAMETER Environment
    The environment name (dev, staging, prod)

.EXAMPLE
    .\setup-b2c.ps1 -TenantName "userservice" -ResourceGroupName "rg-userservice-dev" -Location "eastus" -Environment "dev"
#>

param(
  [Parameter(Mandatory = $true)]
  [string]$TenantName,

  [Parameter(Mandatory = $true)]
  [string]$ResourceGroupName,

  [Parameter(Mandatory = $false)]
  [string]$Location = "eastus",

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
  Write-Host "ℹ $Message" -ForegroundColor Cyan
}

function Write-Warning {
  param([string]$Message)
  Write-Host "⚠ $Message" -ForegroundColor Yellow
}

function Write-ErrorMsg {
  param([string]$Message)
  Write-Host "✗ $Message" -ForegroundColor Red
}

# Check if Azure CLI is installed
Write-Info "Checking Azure CLI installation..."
$azVersion = az --version 2>$null
if ($LASTEXITCODE -ne 0) {
  Write-ErrorMsg "Azure CLI is not installed. Please install from https://aka.ms/installazurecli"
  exit 1
}
Write-Success "Azure CLI is installed"

# Check if logged in to Azure
Write-Info "Checking Azure login status..."
$account = az account show 2>$null
if ($LASTEXITCODE -ne 0) {
  Write-Warning "Not logged in to Azure. Starting login process..."
  az login
  if ($LASTEXITCODE -ne 0) {
    Write-ErrorMsg "Failed to login to Azure"
    exit 1
  }
}
Write-Success "Logged in to Azure"

# Get current subscription
$subscriptionInfo = az account show | ConvertFrom-Json
$subscriptionId = $subscriptionInfo.id
$subscriptionName = $subscriptionInfo.name
Write-Info "Using subscription: $subscriptionName ($subscriptionId)"

# Create resource group if it doesn't exist
Write-Info "Checking if resource group exists..."
$rgExists = az group exists --name $ResourceGroupName
if ($rgExists -eq "false") {
  Write-Info "Creating resource group: $ResourceGroupName"
  az group create --name $ResourceGroupName --location $Location
  if ($LASTEXITCODE -eq 0) {
    Write-Success "Resource group created"
  }
  else {
    Write-ErrorMsg "Failed to create resource group"
    exit 1
  }
}
else {
  Write-Success "Resource group already exists"
}

# Note: Azure AD B2C tenant creation via CLI is limited
# Provide instructions for manual creation
Write-Warning "Azure AD B2C tenant must be created manually through Azure Portal"
Write-Info "Please follow these steps:"
Write-Host ""
Write-Host "1. Go to https://portal.azure.com"
Write-Host "2. Search for 'Azure Active Directory B2C'"
Write-Host "3. Click 'Create'"
Write-Host "4. Select 'Create a new Azure AD B2C Tenant'"
Write-Host "5. Enter details:"
Write-Host "   - Organization name: UserService B2C - $Environment"
Write-Host "   - Initial domain name: $TenantName"
Write-Host "   - Country/Region: United States (or your preference)"
Write-Host "   - Subscription: $subscriptionName"
Write-Host "   - Resource group: $ResourceGroupName"
Write-Host "6. Click 'Review + create' and then 'Create'"
Write-Host "7. Wait 2-3 minutes for tenant creation"
Write-Host "8. Switch to the B2C tenant using the directory switcher"
Write-Host ""

$continue = Read-Host "Have you created the B2C tenant? (y/n)"
if ($continue -ne "y") {
  Write-Warning "Script execution stopped. Please create the B2C tenant first."
  exit 0
}

# Get B2C tenant domain
$b2cDomain = Read-Host "Enter your B2C tenant domain (e.g., userservice.onmicrosoft.com)"

# Switch to B2C tenant
Write-Info "Switching to B2C tenant..."
az login --tenant $b2cDomain --allow-no-subscriptions
if ($LASTEXITCODE -ne 0) {
  Write-ErrorMsg "Failed to switch to B2C tenant"
  exit 1
}
Write-Success "Switched to B2C tenant"

# Register API application
Write-Info "Registering API application..."
$apiIdentifierUri = "https://$b2cDomain/api"
$apiAppName = "UserService API - $Environment"

$apiApp = az ad app create `
  --display-name $apiAppName `
  --identifier-uris $apiIdentifierUri `
  --sign-in-audience "AzureADandPersonalMicrosoftAccount" `
| ConvertFrom-Json

if ($LASTEXITCODE -eq 0) {
  $apiAppId = $apiApp.appId
  Write-Success "API application registered: $apiAppId"
}
else {
  Write-ErrorMsg "Failed to register API application"
  exit 1
}

# Create service principal for API
Write-Info "Creating service principal for API..."
az ad sp create --id $apiAppId
if ($LASTEXITCODE -eq 0) {
  Write-Success "API service principal created"
}

# Add OAuth scopes to API
Write-Info "Adding OAuth scopes to API..."
$scopes = @(
  @{
    adminConsentDescription = "Allow the application to read user profile"
    adminConsentDisplayName = "Read user profile"
    id                      = (New-Guid).Guid
    isEnabled               = $true
    type                    = "User"
    userConsentDescription  = "Allow the application to read your profile"
    userConsentDisplayName  = "Read your profile"
    value                   = "user.read"
  },
  @{
    adminConsentDescription = "Allow the application to write user profile"
    adminConsentDisplayName = "Write user profile"
    id                      = (New-Guid).Guid
    isEnabled               = $true
    type                    = "User"
    userConsentDescription  = "Allow the application to update your profile"
    userConsentDisplayName  = "Update your profile"
    value                   = "user.write"
  }
)

$apiManifest = @{
  oauth2Permissions = $scopes
} | ConvertTo-Json -Depth 10

$apiManifest | Out-File -FilePath "api-manifest.json" -Encoding utf8
az ad app update --id $apiAppId --set api=@api-manifest.json
Remove-Item "api-manifest.json"
Write-Success "OAuth scopes added to API"

# Register SPA application
Write-Info "Registering SPA application..."
$spaAppName = "UserService SPA - $Environment"
$redirectUris = @(
  "http://localhost:3000",
  "http://localhost:5173"
)

$spaApp = az ad app create `
  --display-name $spaAppName `
  --sign-in-audience "AzureADandPersonalMicrosoftAccount" `
  --web-redirect-uris $redirectUris `
  --enable-id-token-issuance `
  --enable-access-token-issuance `
| ConvertFrom-Json

if ($LASTEXITCODE -eq 0) {
  $spaAppId = $spaApp.appId
  Write-Success "SPA application registered: $spaAppId"
}
else {
  Write-ErrorMsg "Failed to register SPA application"
  exit 1
}

# Create service principal for SPA
Write-Info "Creating service principal for SPA..."
az ad sp create --id $spaAppId
if ($LASTEXITCODE -eq 0) {
  Write-Success "SPA service principal created"
}

# Output configuration
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Azure AD B2C Configuration Complete" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "API Application Configuration:" -ForegroundColor Yellow
Write-Host "  Client ID: $apiAppId"
Write-Host "  Identifier URI: $apiIdentifierUri"
Write-Host "  Scopes: user.read, user.write"
Write-Host ""
Write-Host "SPA Application Configuration:" -ForegroundColor Yellow
Write-Host "  Client ID: $spaAppId"
Write-Host "  Redirect URIs: $($redirectUris -join ', ')"
Write-Host ""
Write-Host "Add to appsettings.json:" -ForegroundColor Yellow
Write-Host @"
{
  "AzureAdB2C": {
    "Instance": "https://$($TenantName).b2clogin.com",
    "Domain": "$b2cDomain",
    "ClientId": "$apiAppId",
    "SignUpSignInPolicyId": "B2C_1_signup_signin",
    "ResetPasswordPolicyId": "B2C_1_password_reset",
    "EditProfilePolicyId": "B2C_1_profile_edit"
  }
}
"@
Write-Host ""
Write-Host "Add to frontend .env:" -ForegroundColor Yellow
Write-Host @"
VITE_AZURE_AD_B2C_CLIENT_ID=$spaAppId
VITE_AZURE_AD_B2C_AUTHORITY=https://$($TenantName).b2clogin.com/$b2cDomain/B2C_1_signup_signin
VITE_AZURE_AD_B2C_KNOWN_AUTHORITIES=$($TenantName).b2clogin.com
VITE_API_SCOPES=$apiIdentifierUri/user.read $apiIdentifierUri/user.write
"@
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Green
Write-Host "1. Create user flows in Azure Portal:"
Write-Host "   - B2C_1_signup_signin (Sign up and sign in)"
Write-Host "   - B2C_1_password_reset (Password reset)"
Write-Host "   - B2C_1_profile_edit (Profile editing)"
Write-Host "2. Test user flows from Azure Portal"
Write-Host "3. Update application configuration files with above values"
Write-Host "4. Grant admin consent for API permissions if needed"
Write-Host ""

# Save configuration to file
$config = @{
  tenant      = @{
    name   = $TenantName
    domain = $b2cDomain
  }
  api         = @{
    clientId      = $apiAppId
    identifierUri = $apiIdentifierUri
    scopes        = @("user.read", "user.write")
  }
  spa         = @{
    clientId     = $spaAppId
    redirectUris = $redirectUris
  }
  environment = $Environment
  createdDate = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
}

$configPath = "b2c-config-$Environment.json"
$config | ConvertTo-Json -Depth 10 | Out-File -FilePath $configPath -Encoding utf8
Write-Success "Configuration saved to: $configPath"
Write-Info "Keep this file secure and do not commit it to source control"
