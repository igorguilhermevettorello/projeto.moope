param(
  [Parameter(Mandatory = $true)]
  [string]$dockerHubUser,

  [Parameter(Mandatory = $false)]
  [ValidateSet("Development", "Staging", "Production", "development", "staging", "production")]
  [string]$environment = "Production",

  [Parameter(Mandatory = $false)]
  [string]$version,

  [Parameter(Mandatory = $false)]
  [switch]$push,

  [Parameter(Mandatory = $false)]
  [switch]$continueOnError
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$commonPath = Join-Path $PSScriptRoot "common.ps1"
if (-not (Test-Path $commonPath)) { throw "common.ps1 not found at: $commonPath" }
. $commonPath

$services = @(
  @{
    Key = "bff";      Port = 6100;
    Script = Join-Path $PSScriptRoot "bff\publish.ps1";
    Appsettings = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "bff\Projeto.Moope.Gateways.Api\appsettings.json";
    AppsettingsEnv = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "bff\Projeto.Moope.Gateways.Api\appsettings.$environment.json";
  },
  @{
    Key = "auth";     Port = 6101;
    Script = Join-Path $PSScriptRoot "auth\publish.ps1";
    Appsettings = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\auth\Projeto.Moope.Auth.Api\appsettings.json";
    AppsettingsEnv = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\auth\Projeto.Moope.Auth.Api\appsettings.$environment.json";
  },
  @{
    Key = "cliente";  Port = 6102;
    Script = Join-Path $PSScriptRoot "cliente\publish.ps1";
    Appsettings = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\cliente\Projeto.Moope.Cliente.Api\appsettings.json";
    AppsettingsEnv = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\cliente\Projeto.Moope.Cliente.Api\appsettings.$environment.json";
  },
  @{
    Key = "comodato"; Port = 6103;
    Script = Join-Path $PSScriptRoot "comodato\publish.ps1";
    Appsettings = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\comodato\Projeto.Moope.Comodato.Api\appsettings.json";
    AppsettingsEnv = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\comodato\Projeto.Moope.Comodato.Api\appsettings.$environment.json";
  },
  @{
    Key = "email";    Port = 6104;
    Script = Join-Path $PSScriptRoot "email\publish.ps1";
    Appsettings = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\email\Projeto.Moope.Email.Api\appsettings.json";
    AppsettingsEnv = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\email\Projeto.Moope.Email.Api\appsettings.$environment.json";
  },
  @{
    Key = "endereco"; Port = 6105;
    Script = Join-Path $PSScriptRoot "endereco\publish.ps1";
    Appsettings = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\endereco\Projeto.Moope.Endereco.Api\appsettings.json";
    AppsettingsEnv = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\endereco\Projeto.Moope.Endereco.Api\appsettings.$environment.json";
  },
  @{
    Key = "pagamento"; Port = 6106;
    Script = Join-Path $PSScriptRoot "pagamento\publish.ps1";
    Appsettings = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\pagamento\Projeto.Moope.Pagamento.Api\appsettings.json";
    AppsettingsEnv = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\pagamento\Projeto.Moope.Pagamento.Api\appsettings.$environment.json";
  },
  @{
    Key = "pedido";   Port = 6107;
    Script = Join-Path $PSScriptRoot "pedido\publish.ps1";
    Appsettings = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\pedido\Projeto.Moope.Pedido.Api\appsettings.json";
    AppsettingsEnv = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\pedido\Projeto.Moope.Pedido.Api\appsettings.$environment.json";
  },
  @{
    Key = "plano";    Port = 6108;
    Script = Join-Path $PSScriptRoot "plano\publish.ps1";
    Appsettings = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\plano\Projeto.Moope.Plano.Api\appsettings.json";
    AppsettingsEnv = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\plano\Projeto.Moope.Plano.Api\appsettings.$environment.json";
  },
  @{
    Key = "vendedor"; Port = 6109;
    Script = Join-Path $PSScriptRoot "vendedor\publish.ps1";
    Appsettings = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\vendedor\Projeto.Moope.Vendedor.Api\appsettings.json";
    AppsettingsEnv = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\vendedor\Projeto.Moope.Vendedor.Api\appsettings.$environment.json";
  },
  @{
    Key = "rabbitmq-worker"; Port = 0;
    Script = Join-Path $PSScriptRoot "rabbitmq\publish.ps1";
    Appsettings = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\rabbitmq\Projeto.Moope.RabbitMQ.Worker\appsettings.json";
    AppsettingsEnv = Join-Path (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path "src\rabbitmq\Projeto.Moope.RabbitMQ.Worker\appsettings.$environment.json";
  }
)

$results = @()

Write-Host ""
Write-Host "== Moope :: Docker publish (all services) =="
Write-Host "Environment: $environment"
if (-not [string]::IsNullOrWhiteSpace($version)) { Write-Host "Version override: $version" }
Write-Host "Push: $($push.IsPresent)"
Write-Host "ContinueOnError: $($continueOnError.IsPresent)"

foreach ($svc in $services) {
  $svcKey = $svc.Key
  $svcPort = $svc.Port
  $scriptPath = $svc.Script

  Write-Host ""
  Write-Host "---- $svcKey (port $svcPort) ----"

  try {
    if (-not (Test-Path $scriptPath)) { throw "Publish script not found at: $scriptPath" }

    $resolvedVersion = Resolve-AppVersion `
      -serviceKey $svcKey `
      -environment $environment `
      -appsettingsBasePath $svc.Appsettings `
      -appsettingsEnvPath $svc.AppsettingsEnv `
      -overrideVersion $version

    Write-Host "Resolved version: $resolvedVersion"

    $scriptArgs = @{
      dockerHubUser = $dockerHubUser
      environment = $environment
    }
    if (-not [string]::IsNullOrWhiteSpace($version)) { $scriptArgs.version = $version }
    if ($push.IsPresent) { $scriptArgs.push = $true }

    & $scriptPath @scriptArgs

    $results += [PSCustomObject]@{ Service = $svcKey; Version = $resolvedVersion; Status = "OK" }
  } catch {
    Write-Error $_
    $results += [PSCustomObject]@{ Service = $svcKey; Version = $null; Status = "FAILED" }
    if (-not $continueOnError.IsPresent) { throw }
  }
}

Write-Host ""
Write-Host "== Summary =="
$results | Format-Table -AutoSize
