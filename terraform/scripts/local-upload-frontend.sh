#!/usr/bin/bash

export $(grep -v '^#' ".env" | xargs)

source scripts/upload-frontend.sh $TF_VAR_ENVIRONMENT "../TicTacToeGame/wwwroot"