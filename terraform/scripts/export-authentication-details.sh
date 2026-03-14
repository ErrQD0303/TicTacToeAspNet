#!/usr/bin/bash

export $(grep -v '^#' "sp.env" | xargs)