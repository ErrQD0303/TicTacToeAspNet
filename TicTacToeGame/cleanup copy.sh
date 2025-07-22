#! /usr/bin/env bash
set -ex # Exit on error

echo "Cleaning up Kubernetes resources..."

echo "Deleting configMap..."
command kubectl delete configMap tictactoe-env --ignore-not-found

echo "Deleting Kubernetes resources..."
kubectl delete -f pod-config.yaml --ignore-not-found
kubectl delete -f export-port-service.yaml --ignore-not-found

echo "Deleting Kind cluster..."
kind delete cluster --name cka-tictactoe-cluster

echo "✔️ Cleanup complete."