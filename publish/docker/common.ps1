$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Get-JsonValue {
  param(
    [Parameter(Mandatory = $true)]$json,
    [Parameter(Mandatory = $true)][string[]]$path
  )

  $current = $json
  foreach ($segment in $path) {
    if ($null -eq $current) { return $null }
    if ($current -is [System.Collections.IDictionary] -and $current.Contains($segment)) {
      $current = $current[$segment]
      continue
    }

    if ($current -is [psobject]) {
      $prop = $current.PSObject.Properties[$segment]
      if ($null -ne $prop) {
        $current = $prop.Value
        continue
      }
    }

    return $null
  }
  return $current
}

function Read-VersionFromAppsettingsFile {
  param(
    [Parameter(Mandatory = $true)][string]$path
  )

  if (-not (Test-Path $path)) { return $null }

  try {
    $json = Get-Content -Raw -Path $path | ConvertFrom-Json
  } catch {
    return $null
  }

  foreach ($candidate in @(
    @("Application", "Version"),
    @("App", "Version"),
    @("Version")
  )) {
    $value = Get-JsonValue -json $json -path $candidate
    if ($null -ne $value -and -not [string]::IsNullOrWhiteSpace([string]$value)) {
      return ([string]$value).Trim()
    }
  }

  return $null
}

function Resolve-AppVersion {
  param(
    [Parameter(Mandatory = $true)][string]$serviceKey,
    [Parameter(Mandatory = $true)][string]$environment,
    [Parameter(Mandatory = $true)][string]$appsettingsBasePath,
    [Parameter(Mandatory = $true)][string]$appsettingsEnvPath,
    [Parameter(Mandatory = $false)][string]$overrideVersion
  )

  if (-not (Test-Path $appsettingsBasePath)) {
    throw "[$serviceKey] appsettings.json not found at: $appsettingsBasePath"
  }

  if (-not [string]::IsNullOrWhiteSpace($overrideVersion)) {
    return $overrideVersion.Trim()
  }

  $version = Read-VersionFromAppsettingsFile -path $appsettingsEnvPath
  if ([string]::IsNullOrWhiteSpace($version)) {
    $version = Read-VersionFromAppsettingsFile -path $appsettingsBasePath
  }

  if ([string]::IsNullOrWhiteSpace($version)) {
    $envFileName = [System.IO.Path]::GetFileName($appsettingsEnvPath)
    throw "[$serviceKey] Could not resolve version from '$envFileName' or 'appsettings.json'. Expected keys: Application:Version, App:Version, or Version. Provide -version to override."
  }

  return $version
}

