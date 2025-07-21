# usr/bin/env bash
set -e # Exit on error

# Can only be run in a VM or local environment
kind create cluster --name cka-tictactoegame --image kindest/node:v1.29.4@sha256:3abb816a5b1061fb15c6e9e60856ec40d56b7b52bcea5f5f1350bc6e2320b6f8 --config kind-config.yaml

command kubectl create configmap tictactoe-env --from-env-file=.env

kubectl apply -f tictactoegame-pod-config.yaml
kubectl apply -f NodePort.yaml
