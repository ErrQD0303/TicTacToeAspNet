variable "RESOURCE_GROUP_NAME" {
  description = "The name of the Azure Resource Group where resources will be created."
  type        = string

  validation {
    condition     = var.RESOURCE_GROUP_NAME != null || var.RESOURCE_GROUP_NAME != ""
    error_message = "RESOURCE_GROUP_NAME cannot be null or empty. Please provide a valid resource group name."
  }
}

variable "STORAGE_ACCOUNT_NAME" {
  description = "The name of the Azure Storage Account used for Terraform state storage."
  type        = string

  validation {
    condition     = var.STORAGE_ACCOUNT_NAME != null || var.STORAGE_ACCOUNT_NAME != ""
    error_message = "STORAGE_ACCOUNT_NAME cannot be null or empty. Please provide a valid storage account name."
  }
}

variable "CONTAINER_NAME" {
  description = "The name of the Azure Storage Container used for Terraform state storage."
  type        = string

  validation {
    condition     = var.CONTAINER_NAME != null || var.CONTAINER_NAME != ""
    error_message = "CONTAINER_NAME cannot be null or empty. Please provide a valid container name."
  }
}

variable "ENVIRONMENT" {
  description = "The environment for which the Terraform configuration is being applied ."
  type        = string
  default     = "dev"

  validation {
    condition     = contains(local.allowed_environments, var.ENVIRONMENT)
    error_message = "Invalid environment. Allowed values are: ${join(", ", local.allowed_environments)}."
  }
}

variable "LOCATION" {
  description = "The Azure region where resources will be created."
  type        = string
  default     = "japaneast"

  validation {
    condition     = contains(local.allowed_locations, var.LOCATION)
    error_message = "Invalid location. Allowed values are: ${join(", ", local.allowed_locations)}."
  }
}

variable "WEBAPP_RESOURCE_GROUP_NAME" {
  description = "The name of the Azure Resource Group where the Web App will be created."
  type        = string

  validation {
    condition     = var.WEBAPP_RESOURCE_GROUP_NAME != null || var.WEBAPP_RESOURCE_GROUP_NAME != ""
    error_message = "WEBAPP_RESOURCE_GROUP_NAME cannot be null or empty. Please provide a valid resource group name for the Web App."
  }
}

# variable "CORS_ORIGIN" {
#   description = "The allowed origins for CORS configuration in the Web App."
#   type        = string
#   default     = ""

#   validation {
#     condition     = var.CORS_ORIGIN != null || var.CORS_ORIGIN != ""
#     error_message = "CORS_ORIGIN cannot be null or empty. Please provide valid CORS origins."
#   }
# }
