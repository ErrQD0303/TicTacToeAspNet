#!/usr/bin/bash

export $(grep -v '^#' ".env" | xargs)

az group create --name $TF_VAR_WEBAPP_RESOURCE_GROUP_NAME \
    --location $TF_VAR_LOCATION