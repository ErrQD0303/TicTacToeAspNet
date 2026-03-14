#!/usr/bin/bash
export $(grep -v '^#' "sp.env" | xargs)
export $(grep -v '^#' ".env" | xargs)

MSYS_NO_PATHCONV=1 az role assignment create \
    --assignee "$ARM_CLIENT_ID" \
    --role "Contributor" \
    --scope "/subscriptions/$ARM_SUBSCRIPTION_ID/resourceGroups/$TF_VAR_WEBAPP_RESOURCE_GROUP_NAME"

MSYS_NO_PATHCONV=1 az role assignment create \
    --assignee "$ARM_CLIENT_ID" \
    --role "Storage Blob Data Contributor" \
    --scope "/subscriptions/$ARM_SUBSCRIPTION_ID/resourceGroups/$TF_VAR_RESOURCE_GROUP_NAME/providers/Microsoft.Storage/storageAccounts/$TF_VAR_STORAGE_ACCOUNT_NAME"

# MSYS_NO_PATHCONV=1  az role assignment create \
#     --assignee "$ARM_CLIENT_ID" \
#     --role "Key Vault Secrets User" \
#     --scope "/subscriptions/$ARM_SUBSCRIPTION_ID/resourceGroups/$TF_VAR_KEYVAULT_RESOURCE_GROUP_NAME/providers/Microsoft.KeyVault/vaults/$TF_VAR_KEYVAULT_NAME"