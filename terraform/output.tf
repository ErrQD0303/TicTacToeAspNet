output "resource_group_name" {
  value       = var.RESOURCE_GROUP_NAME
  description = "Backend Resource Group Name"
}

output "storage_account_name" {
  value       = var.STORAGE_ACCOUNT_NAME
  description = "Backend Storage Account Name"
}

output "container_name" {
  value       = var.CONTAINER_NAME
  description = "Backend Container Name"
}

output "location" {
  value       = var.LOCATION
  description = "Azure Region for Resource Creation"
}

output "environment" {
  value       = var.ENVIRONMENT
  description = "Deployment Environment"
}

output "webapp_resource_group_name" {
  value       = var.WEBAPP_RESOURCE_GROUP_NAME
  description = "Web App Resource Group Name"
}

output "static_website_endpoint" {
  value       = azurerm_storage_account.frontend.primary_web_endpoint
  description = "Static Website Endpoint for the Storage Account"
}
