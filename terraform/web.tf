data "azurerm_resource_group" "rg" {
  name = var.RESOURCE_GROUP_NAME
}

resource "azurerm_service_plan" "asp" {
  name                = "${var.ENVIRONMENT}-asp"
  resource_group_name = data.azurerm_resource_group.rg.name
  location            = data.azurerm_resource_group.rg.location
  os_type             = "Linux"
  sku_name            = "B1"
}

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
      allowed_origins     = [var.CORS_ORIGIN]
      support_credentials = true
    }
  }

  app_settings = {
    "CORS_ORIGIN" = var.CORS_ORIGIN
  }
}
