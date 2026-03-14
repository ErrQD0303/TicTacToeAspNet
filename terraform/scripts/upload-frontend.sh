#!/usr/bin/bash

export $(grep -v '^#' .env | xargs)

az storage blob upload-batch \
    --account-name "${TF_VAR_ENVIRONMENT}ticstorage" \
    --destination '$web' \
    --source '../TicTacToeGame/wwwroot' \
    --overwrite