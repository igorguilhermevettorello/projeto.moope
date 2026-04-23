#!/usr/bin/env bash
set -euo pipefail

read_version_from_appsettings() {
  local path="$1"
  if [[ ! -f "$path" ]]; then
    return 1
  fi

  if command -v jq >/dev/null 2>&1; then
    local v
    v="$(jq -r '.Application.Version // .App.Version // .Version // empty' "$path" 2>/dev/null || true)"
    if [[ -n "${v}" ]]; then
      printf "%s" "${v}"
      return 0
    fi
    return 1
  fi

  echo "ERROR: 'jq' is required to read version from JSON (${path}). Install jq or pass --version." >&2
  return 2
}

resolve_app_version() {
  local service_key="$1"
  local environment="$2"
  local appsettings_base_path="$3"
  local appsettings_env_path="$4"
  local override_version="${5:-}"

  if [[ ! -f "${appsettings_base_path}" ]]; then
    echo "[${service_key}] appsettings.json not found at: ${appsettings_base_path}" >&2
    return 1
  fi

  if [[ -n "${override_version}" ]]; then
    printf "%s" "${override_version}"
    return 0
  fi

  local version=""
  if [[ -f "${appsettings_env_path}" ]]; then
    version="$(read_version_from_appsettings "${appsettings_env_path}" || true)"
  fi

  if [[ -z "${version}" ]]; then
    version="$(read_version_from_appsettings "${appsettings_base_path}" || true)"
  fi

  if [[ -z "${version}" ]]; then
    echo "[${service_key}] Could not resolve version from appsettings. Expected keys: Application.Version, App.Version, or Version. Provide --version to override." >&2
    return 1
  fi

  printf "%s" "${version}"
  return 0
}

