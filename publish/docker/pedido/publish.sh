#!/usr/bin/env bash
set -euo pipefail

service_key="pedido"
service_port="6107"
image_name=""

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "${script_dir}/../../.." && pwd)"

common_path="${script_dir}/../common.sh"
if [[ ! -f "${common_path}" ]]; then
  echo "common.sh not found at: ${common_path}" >&2
  exit 1
fi
# shellcheck disable=SC1090
source "${common_path}"

dockerfile_path="${repo_root}/src/pedido/Projeto.Moope.Pedido.Api/Dockerfile"
build_context="${repo_root}"
appsettings_base_path="${repo_root}/src/pedido/Projeto.Moope.Pedido.Api/appsettings.json"

docker_hub_user=""
environment="Production"
version=""
push="false"

usage() {
  cat <<EOF
Usage:
  $(basename "$0") --dockerhub-user <user> [--environment <Development|Staging|Production>] [--version <x.y.z>] [--push]

Notes:
  - Service: ${service_key} (port ${service_port})
  - Builds and tags:
      <user>/${image_name}:<version>
      <user>/${image_name}:latest
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --dockerhub-user)
      docker_hub_user="${2:-}"; shift 2;;
    --environment)
      environment="${2:-}"; shift 2;;
    --version)
      version="${2:-}"; shift 2;;
    --push)
      push="true"; shift 1;;
    -h|--help)
      usage; exit 0;;
    *)
      echo "Unknown argument: $1" >&2
      usage
      exit 2;;
  esac
done

if [[ -z "${docker_hub_user}" ]]; then
  echo "Missing required --dockerhub-user" >&2
  usage
  exit 2
fi

if [[ ! -f "${dockerfile_path}" ]]; then
  echo "Dockerfile not found at: ${dockerfile_path}" >&2
  exit 1
fi

environment_lower="${environment,,}"
if [[ -z "${environment_lower}" ]]; then environment_lower="production"; fi
image_name="moope-${service_key}-${environment_lower}"

if [[ -z "${version}" ]]; then
  env_appsettings="${repo_root}/src/pedido/Projeto.Moope.Pedido.Api/appsettings.${environment}.json"
fi

version="$(resolve_app_version "${service_key}" "${environment}" "${appsettings_base_path}" "${env_appsettings}" "${version}")"
echo "Resolved version: ${version}"

version_tag="${docker_hub_user}/${image_name}:${version}"
latest_tag="${docker_hub_user}/${image_name}:latest"

echo ""
echo "== Moope :: ${service_key} (port ${service_port}) =="
echo "Repo root: ${repo_root}"
echo "Dockerfile: ${dockerfile_path}"
echo "Build context: ${build_context}"
echo "Environment: ${environment}"
echo ""
echo "Building image:"
echo " - ${version_tag}"
echo " - ${latest_tag}"

docker build \
  --file "${dockerfile_path}" \
  --tag "${version_tag}" \
  --tag "${latest_tag}" \
  "${build_context}"

echo ""
echo "Build complete."

if [[ "${push}" == "true" ]]; then
  echo ""
  echo "Pushing to Docker Hub (docker login required)."
  docker login
  docker push "${version_tag}"
  docker push "${latest_tag}"
  echo ""
  echo "Push complete."
else
  echo ""
  echo "Push skipped. Re-run with --push to publish."
fi
