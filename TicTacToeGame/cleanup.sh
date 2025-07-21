#! /usr/bin/env bash

set -e # Exit on error

echo "Cleaning up Kubernetes resources..."

# Delete the configmap
kubectl delete configmap tictactoe-env --ignore-not-found

# Delete the pod or deployment and service (whichever was applied)
kubectl delete -f tictactoegame-pod-config.yaml --ignore-not-found
kubectl delete -f NodePort.yaml --ignore-not-found

echo "Deleting Kind cluster..."
kind delete cluster --name cka-tictactoegame

# echo "Optionally removing local Docker image..."
# docker image rm datvipcrvn/tic-tac-toe-game:latest || echo "Docker image not found, skipping removal."

echo "✔️ Cleanup complete."
