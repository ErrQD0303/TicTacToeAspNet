data "azurerm_resource_group" "rg" {
  name = var.WEBAPP_RESOURCE_GROUP_NAME
}

resource "azurerm_service_plan" "asp" {
  name                = "${var.ENVIRONMENT}-asp"
  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = "B1"
}

resource "azurerm_storage_account" "frontend" {
  name                     = "${var.ENVIRONMENT}ticstorage"
  resource_group_name      = data.azurerm_resource_group.rg.name
  location                 = data.azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"

  static_website {
    index_document     = "index.html"
    error_404_document = "404.html"
  }
}

# resource "azurerm_storage_blob" "index_html" {
#   name                   = "index.html"
#   storage_account_name   = azure.storage_account.frontend.name
#   storage_container_name = "$web"
#   type                   = "Block"
#   source                 = "../TicTacToeGame/wwwroot/index.html"
#   # We need to set content type other while the browser will try to download the file instead of rendering it
#   content_type = "text/html"
# }

resource "azurerm_linux_web_app" "web" {
  name                = "${var.ENVIRONMENT}-tictactoe-web"
  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location
  service_plan_id     = azurerm_service_plan.asp.id

  site_config {
    application_stack {
      docker_image_name   = "datvipcrvn/tic-tac-toe-game:latest"
      docker_registry_url = "https://index.docker.io"
    }

    cors {
      allowed_origins     = [local.cors.allowed_origins]
      support_credentials = local.cors.support_credentials
    }
  }

  app_settings = {
    "CORS_ORIGIN" = local.cors.allowed_origins
  }
}
