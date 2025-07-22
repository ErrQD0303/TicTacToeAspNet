#! /usr/bin/bash
set -e # Exit on error
# For debugging add `set -x` to see which command is being executed

# Ensure the kind local registry is running
reg_name='kind-registry'
reg_port='5001'
if [ "$(docker inspect -f '{{.State.Running}}' "${reg_name}" 2>/dev/null || true)" != 'true' ]; then
  docker run \
    -d --restart=always -p "127.0.0.1:${reg_port}:5000" --network bridge --name "${reg_name}" \
    registry:2
fi

if [ "$(docker inspect -f='{{json .NetworkSettings.Networks.kind}}' "${reg_name}")" = 'null' ]; then
  docker network connect "kind" "${reg_name}"
fi

# Deploy script for the Tic Tac Toe game clone
kind create cluster --config cluster-config.yaml --image kindest/node:v1.33.1@sha256:050072256b9a903bd914c0b2866828150cb229cea0efe5892e2b644d5dd3b34f --name cka-tictactoe-cluster

kubectl apply -f local-registry-configmap.yaml

command kubectl create configmap tictactoe-env --from-env-file=.env

kubectl apply -f pod-config.yaml
kubectl apply -f export-port-service.yaml

# To output the execution logs run this command:
# ./deploy.sh 2>&1 | tee deploy.log