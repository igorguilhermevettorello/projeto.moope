#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "${script_dir}/../.." && pwd)"

common_path="${script_dir}/common.sh"
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
continue_on_error="false"

usage() {
  cat <<EOF
Usage:
  $(basename "$0") --dockerhub-user <user> [--environment <Development|Staging|Production>] [--version <x.y.z>] [--push] [--continue-on-error]
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
    --continue-on-error)
      continue_on_error="true"; shift 1;;
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

echo ""
echo "== Moope :: Docker publish (all services) =="
echo "Environment: ${environment}"
if [[ -n "${version}" ]]; then echo "Version override: ${version}"; fi
echo "Push: ${push}"
echo "ContinueOnError: ${continue_on_error}"

services=(
  "bff|6100|${script_dir}/bff/publish.sh|${repo_root}/bff/Projeto.Moope.Gateways.Api/appsettings.json|${repo_root}/bff/Projeto.Moope.Gateways.Api/appsettings.${environment}.json"
  "auth|6101|${script_dir}/auth/publish.sh|${repo_root}/src/auth/Projeto.Moope.Auth.Api/appsettings.json|${repo_root}/src/auth/Projeto.Moope.Auth.Api/appsettings.${environment}.json"
  "cliente|6102|${script_dir}/cliente/publish.sh|${repo_root}/src/cliente/Projeto.Moope.Cliente.Api/appsettings.json|${repo_root}/src/cliente/Projeto.Moope.Cliente.Api/appsettings.${environment}.json"
  "comodato|6103|${script_dir}/comodato/publish.sh|${repo_root}/src/comodato/Projeto.Moope.Comodato.Api/appsettings.json|${repo_root}/src/comodato/Projeto.Moope.Comodato.Api/appsettings.${environment}.json"
  "email|6104|${script_dir}/email/publish.sh|${repo_root}/src/email/Projeto.Moope.Email.Api/appsettings.json|${repo_root}/src/email/Projeto.Moope.Email.Api/appsettings.${environment}.json"
  "endereco|6105|${script_dir}/endereco/publish.sh|${repo_root}/src/endereco/Projeto.Moope.Endereco.Api/appsettings.json|${repo_root}/src/endereco/Projeto.Moope.Endereco.Api/appsettings.${environment}.json"
  "pagamento|6106|${script_dir}/pagamento/publish.sh|${repo_root}/src/pagamento/Projeto.Moope.Pagamento.Api/appsettings.json|${repo_root}/src/pagamento/Projeto.Moope.Pagamento.Api/appsettings.${environment}.json"
  "pedido|6107|${script_dir}/pedido/publish.sh|${repo_root}/src/pedido/Projeto.Moope.Pedido.Api/appsettings.json|${repo_root}/src/pedido/Projeto.Moope.Pedido.Api/appsettings.${environment}.json"
  "plano|6108|${script_dir}/plano/publish.sh|${repo_root}/src/plano/Projeto.Moope.Plano.Api/appsettings.json|${repo_root}/src/plano/Projeto.Moope.Plano.Api/appsettings.${environment}.json"
  "vendedor|6109|${script_dir}/vendedor/publish.sh|${repo_root}/src/vendedor/Projeto.Moope.Vendedor.Api/appsettings.json|${repo_root}/src/vendedor/Projeto.Moope.Vendedor.Api/appsettings.${environment}.json"
  "rabbitmq-worker|0|${script_dir}/rabbitmq/publish.sh|${repo_root}/src/rabbitmq/Projeto.Moope.RabbitMQ.Worker/appsettings.json|${repo_root}/src/rabbitmq/Projeto.Moope.RabbitMQ.Worker/appsettings.${environment}.json"
)

summary_lines=()

for entry in "${services[@]}"; do
  IFS='|' read -r service_key service_port child_script appsettings_base appsettings_env <<< "${entry}"
  echo ""
  echo "---- ${service_key} (port ${service_port}) ----"

  if [[ ! -f "${child_script}" ]]; then
    echo "Publish script not found at: ${child_script}" >&2
    summary_lines+=("${service_key}|-|FAILED")
    if [[ "${continue_on_error}" != "true" ]]; then exit 1; fi
    continue
  fi

  if ! resolved_version="$(resolve_app_version "${service_key}" "${environment}" "${appsettings_base}" "${appsettings_env}" "${version}")"; then
    summary_lines+=("${service_key}|-|FAILED")
    if [[ "${continue_on_error}" != "true" ]]; then exit 1; fi
    continue
  fi

  echo "Resolved version: ${resolved_version}"

  args=( --dockerhub-user "${docker_hub_user}" --environment "${environment}" )
  if [[ -n "${version}" ]]; then args+=( --version "${version}" ); fi
  if [[ "${push}" == "true" ]]; then args+=( --push ); fi

  if "${child_script}" "${args[@]}"; then
    summary_lines+=("${service_key}|${resolved_version}|OK")
  else
    summary_lines+=("${service_key}|${resolved_version}|FAILED")
    if [[ "${continue_on_error}" != "true" ]]; then exit 1; fi
  fi
done

echo ""
echo "== Summary =="
printf "%-12s %-12s %-8s\n" "service" "version" "status"
for line in "${summary_lines[@]}"; do
  IFS='|' read -r s v st <<< "${line}"
  printf "%-12s %-12s %-8s\n" "${s}" "${v}" "${st}"
done
