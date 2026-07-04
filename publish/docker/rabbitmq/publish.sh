#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "${script_dir}/../../.." && pwd)"

common_path="${script_dir}/../common.sh"
if [[ ! -f "${common_path}" ]]; then
  echo "common.sh not found at: ${common_path}" >&2
  exit 1
fi
# shellcheck disable=SC1090
source "${common_path}"

docker_hub_user=""
environment="Production"
version=""
push="false"

usage() {
  cat <<EOF
Usage:
  $(basename "$0") --dockerhub-user <user> [--environment <Development|Staging|Production>] [--version <x.y.z>] [--push]
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

service_key="rabbitmq-worker"
environment_lower="$(echo "${environment}" | tr '[:upper:]' '[:lower:]')"
if [[ -z "${environment_lower}" ]]; then environment_lower="production"; fi

image_name="moope-${service_key}-${environment_lower}"
dockerfile_path="${repo_root}/src/rabbitmq/Projeto.Moope.RabbitMQ.Worker/Dockerfile"
build_context="${repo_root}"

appsettings_base="${repo_root}/src/rabbitmq/Projeto.Moope.RabbitMQ.Worker/appsettings.json"
appsettings_env="${repo_root}/src/rabbitmq/Projeto.Moope.RabbitMQ.Worker/appsettings.${environment}.json"

echo ""
echo "== Moope :: ${service_key} =="
echo "Repo root: ${repo_root}"
echo "Dockerfile: ${dockerfile_path}"
echo "Build context: ${build_context}"
echo "Environment: ${environment}"

if [[ ! -f "${dockerfile_path}" ]]; then
  echo "Dockerfile not found at: ${dockerfile_path}" >&2
  exit 1
fi

if [[ ! -f "${appsettings_env}" ]]; then
  echo "[${service_key}] appsettings for environment '${environment}' not found at: ${appsettings_env}" >&2
  exit 1
fi

resolved_version="$(resolve_app_version "${service_key}" "${environment}" "${appsettings_base}" "${appsettings_env}" "${version}")"
echo "Resolved version: ${resolved_version}"

version_tag="${docker_hub_user}/${image_name}:${resolved_version}"
latest_tag="${docker_hub_user}/${image_name}:latest"

echo ""
echo "Building image:"
echo " - ${version_tag}"
echo " - ${latest_tag}"

docker build \
  --file "${dockerfile_path}" \
  --build-arg "BUILD_ENVIRONMENT=${environment}" \
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

