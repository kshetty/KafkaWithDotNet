variable "apim_name" {
  description = "Name of the API Management instance"
  type        = string
}

variable "location" {
  description = "Azure region for APIM instance"
  type        = string
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "publisher_name" {
  description = "Publisher name for APIM"
  type        = string
  default     = "UserService"
}

variable "publisher_email" {
  description = "Publisher email for APIM"
  type        = string
}

variable "sku_name" {
  description = "SKU name for APIM (Developer_1, Basic_1, Standard_1, Premium_1)"
  type        = string
  default     = "Developer_1"

  validation {
    condition     = can(regex("^(Developer|Basic|Standard|Premium)_[0-9]+$", var.sku_name))
    error_message = "SKU name must be in format: <tier>_<capacity>, e.g., Developer_1, Standard_2"
  }
}

variable "backend_url" {
  description = "Backend URL for UserService API"
  type        = string
}

variable "b2c_tenant_id" {
  description = "Azure AD B2C Tenant ID"
  type        = string
}

variable "b2c_domain" {
  description = "Azure AD B2C domain (e.g., userservice.b2clogin.com)"
  type        = string
}

variable "b2c_client_id" {
  description = "Azure AD B2C API Client ID (audience)"
  type        = string
}

variable "b2c_policy_name" {
  description = "Azure AD B2C sign-up/sign-in policy name"
  type        = string
  default     = "B2C_1_signup_signin"
}

variable "custom_domain" {
  description = "Custom domain for APIM gateway (optional)"
  type        = string
  default     = null
}

variable "certificate_base64" {
  description = "Base64 encoded certificate for custom domain (optional)"
  type        = string
  default     = null
  sensitive   = true
}

variable "certificate_password" {
  description = "Password for custom domain certificate (optional)"
  type        = string
  default     = null
  sensitive   = true
}

variable "virtual_network_type" {
  description = "Virtual network type (None, External, Internal)"
  type        = string
  default     = "None"

  validation {
    condition     = contains(["None", "External", "Internal"], var.virtual_network_type)
    error_message = "Virtual network type must be None, External, or Internal"
  }
}

variable "subnet_id" {
  description = "Subnet ID for VNet integration (required if virtual_network_type != None)"
  type        = string
  default     = null
}

variable "allowed_origins" {
  description = "List of allowed CORS origins"
  type        = list(string)
  default = [
    "http://localhost:3000",
    "http://localhost:5173"
  ]
}

variable "enable_developer_portal" {
  description = "Enable developer portal"
  type        = bool
  default     = true
}

variable "notification_sender_email" {
  description = "Email address for APIM notifications"
  type        = string
  default     = null
}

variable "starter_rate_limit_calls" {
  description = "Rate limit calls per minute for Starter product"
  type        = number
  default     = 50
}

variable "starter_quota_calls" {
  description = "Daily quota for Starter product"
  type        = number
  default     = 5000
}

variable "unlimited_rate_limit_calls" {
  description = "Rate limit calls per minute for Unlimited product"
  type        = number
  default     = 1000
}

variable "unlimited_quota_calls" {
  description = "Daily quota for Unlimited product"
  type        = number
  default     = 1000000
}

variable "cache_duration_seconds" {
  description = "Cache duration in seconds for GET requests"
  type        = number
  default     = 300
}

variable "backend_request_timeout" {
  description = "Backend request timeout in seconds"
  type        = number
  default     = 30
}

variable "api_revision" {
  description = "API revision number"
  type        = string
  default     = "1"
}

variable "enable_http2" {
  description = "Enable HTTP/2 protocol"
  type        = bool
  default     = true
}

variable "min_api_version" {
  description = "Minimum API version (e.g., 2021-08-01)"
  type        = string
  default     = null
}

variable "zones" {
  description = "Availability zones (Premium tier only)"
  type        = list(string)
  default     = []
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
}

variable "log_analytics_workspace_id" {
  description = "Log Analytics workspace ID for diagnostic logs (optional)"
  type        = string
  default     = null
}

variable "enable_sign_in" {
  description = "Enable sign-in to developer portal"
  type        = bool
  default     = true
}

variable "enable_sign_up" {
  description = "Enable sign-up to developer portal"
  type        = bool
  default     = true
}

variable "terms_of_service" {
  description = "Terms of service configuration"
  type = object({
    consent_required = bool
    enabled          = bool
    text             = string
  })
  default = {
    consent_required = false
    enabled          = false
    text             = ""
  }
}
