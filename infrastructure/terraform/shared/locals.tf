# Shared Local Variables

locals {
  # Resource naming prefix
  resource_prefix = "${var.project_name}-${var.environment}"

  # Common tags applied to all resources
  common_tags = merge(
    {
      Project     = var.project_name
      Environment = var.environment
      ManagedBy   = "Terraform"
      CreatedDate = formatdate("YYYY-MM-DD", timestamp())
    },
    var.tags
  )

  # Network CIDR calculations
  subnet_cidrs = {
    container_apps    = cidrsubnet(var.vnet_address_space[0], 8, 1) # 10.0.1.0/24
    postgresql        = cidrsubnet(var.vnet_address_space[0], 8, 2) # 10.0.2.0/24
    redis             = cidrsubnet(var.vnet_address_space[0], 8, 3) # 10.0.3.0/24
    apim              = cidrsubnet(var.vnet_address_space[0], 8, 4) # 10.0.4.0/24
    private_endpoints = cidrsubnet(var.vnet_address_space[0], 8, 5) # 10.0.5.0/24
  }
}
