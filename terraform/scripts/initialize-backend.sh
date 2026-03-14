#!/usr/bin/bash

export $(grep -v '^#' .env | xargs)

az group create --name $TF_VAR_RESOURCE_GROUP_NAME \
    --location $TF_VAR_LOCATION

az storage account create --name $TF_VAR_STORAGE_ACCOUNT_NAME \
    --resource-group $TF_VAR_RESOURCE_GROUP_NAME \
    --location $TF_VAR_LOCATION \
    --sku Standard_LRS \
    --encryption-services blob

az storage container create --name $TF_VAR_CONTAINER_NAME \
    --account-name $TF_VAR_STORAGE_ACCOUNT_NAME

# Cleanup
# az group delete --name $TF_VAR_RESOURCE_GROUP_NAME --yes --no-wait