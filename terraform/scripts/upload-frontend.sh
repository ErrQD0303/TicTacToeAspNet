#!/usr/bin/bash

ENVIRONMENT=$1
FRONTEND_PATH=$2

az storage blob upload-batch \
    --account-name "${ENVIRONMENT}ticstorage" \
    --destination '$web' \
    --source "${FRONTEND_PATH}" \
    --overwrite