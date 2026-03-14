#!/usr/bin/bash

export $(grep -v '^#' ".env" | xargs)

SUBSCRIPTION_ID=$(az account show --query id -o tsv)
# SCOPES="/subscriptions/${SUBSCRIPTION_ID}"
SERVICE_PINCIPAL_FILE="service_principal.json"

az ad sp create-for-rbac \
    --name "$SERVICE_PRINCIPAL_NAME" \
    --skip-assignment \
    --output json > $SERVICE_PINCIPAL_FILE

rm -f sp.env
touch sp.env

CLIENT_ID=$(grep -oP '(?<="appId": ")[^"]*' $SERVICE_PINCIPAL_FILE)
CLIENT_SECRET=$(grep -oP '(?<="password": ")[^"]*' $SERVICE_PINCIPAL_FILE)
TENANT_ID=$(grep -oP '(?<="tenant": ")[^"]*' $SERVICE_PINCIPAL_FILE)

cat >> sp.env << EOF
ARM_CLIENT_ID=$CLIENT_ID
ARM_CLIENT_SECRET=$CLIENT_SECRET
ARM_TENANT_ID=$TENANT_ID
ARM_SUBSCRIPTION_ID=$SUBSCRIPTION_ID
EOF