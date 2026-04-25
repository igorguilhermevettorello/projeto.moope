param(
  [Parameter(Mandatory = $true)]
  [string]$dockerHubUser,

  [Parameter(Mandatory = $false)]
  [ValidateSet("Development", "Staging", "Production", "development", "staging", "production")]
  [string]$environment = "Production",

  [Parameter(Mandatory = $false)]
  [string]$version,

  [Parameter(Mandatory = $false)]
  [switch]$push
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$commonPath = Join-Path $PSScriptRoot "..\common.ps1"
if (-not (Test-Path $commonPath)) { throw "common.ps1 not found at: $commonPath" }
. $commonPath

$serviceKey = "vendedor"
$servicePort = 6109
$environmentSuffix = ($environment ?? "").Trim().ToLowerInvariant()
if ([string]::IsNullOrWhiteSpace($environmentSuffix)) { $environmentSuffix = "production" }
$imageName = "moope-$serviceKey-$environmentSuffix"
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path
$dockerfilePath = Join-Path $repoRoot "src\vendedor\Projeto.Moope.Vendedor.Api\Dockerfile"
$buildContext = $repoRoot
$appsettingsBasePath = Join-Path $repoRoot "src\vendedor\Projeto.Moope.Vendedor.Api\appsettings.json"
$appsettingsEnvPath = Join-Path $repoRoot "src\vendedor\Projeto.Moope.Vendedor.Api\appsettings.$environment.json"

Write-Host ""
Write-Host "== Moope :: $serviceKey (port $servicePort) =="
Write-Host "Repo root: $repoRoot"
Write-Host "Dockerfile: $dockerfilePath"
Write-Host "Build context: $buildContext"
Write-Host "Environment: $environment"

if (-not (Test-Path $dockerfilePath)) {
  throw "Dockerfile not found at: $dockerfilePath"
}

$version = Resolve-AppVersion `
  -serviceKey $serviceKey `
  -environment $environment `
  -appsettingsBasePath $appsettingsBasePath `
  -appsettingsEnvPath $appsettingsEnvPath `
  -overrideVersion $version

Write-Host "Resolved version: $version"

$versionTag = "$dockerHubUser/$imageName`:$version"
$latestTag = "$dockerHubUser/$imageName`:latest"

Write-Host ""
Write-Host "Building image:"
Write-Host " - $versionTag"
Write-Host " - $latestTag"

docker build `
  --file $dockerfilePath `
  --tag $versionTag `
  --tag $latestTag `
  $buildContext

Write-Host ""
Write-Host "Build complete."

if ($push.IsPresent) {
  Write-Host ""
  Write-Host "Pushing to Docker Hub (docker login required)."
  docker login

  docker push $versionTag
  docker push $latestTag

  Write-Host ""
  Write-Host "Push complete."
} else {
  Write-Host ""
  Write-Host "Push skipped. Re-run with -push to publish."
}
