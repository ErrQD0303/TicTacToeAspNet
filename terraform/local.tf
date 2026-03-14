locals {
  allowed_environments = ["dev", "staging", "prod"]
  allowed_locations    = ["japaneast", "japanwest"]
  cors = {
    allowed_origins     = [trim(azurerm_storage_account.frontend.primary_web_endpoint, "/")]
    support_credentials = true
  }
}
