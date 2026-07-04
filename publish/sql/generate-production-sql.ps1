param(
  [Parameter(Mandatory = $false)]
  [string]$repoRoot = "",

  [Parameter(Mandatory = $false)]
  [string]$outPath = "",

  [Parameter(Mandatory = $false)]
  [switch]$SkipRegenerate,

  [Parameter(Mandatory = $false)]
  [ValidateSet("utf8NoBom", "utf8Bom")]
  [string]$encoding = "utf8NoBom"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$scriptRoot = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($scriptRoot)) {
  $scriptRoot = Split-Path -Parent $PSCommandPath
}

if ([string]::IsNullOrWhiteSpace($repoRoot)) {
  $repoRoot = (Resolve-Path (Join-Path $scriptRoot "..\..")).Path
}

if ([string]::IsNullOrWhiteSpace($outPath)) {
  $outPath = Join-Path $scriptRoot "production.sql"
}

$resolvedRoot = (Resolve-Path -LiteralPath $repoRoot).Path
$tmpDir = Join-Path $scriptRoot ".tmp"

$dbContexts = @(
  @{
    Name           = "Auth Business"
    Context        = "AppAuthContext"
    Project        = "src/auth/Projeto.Moope.Auth.Infrastructure/Projeto.Moope.Auth.Infrastructure.csproj"
    StartupProject = "src/auth/Projeto.Moope.Auth.Api/Projeto.Moope.Auth.Api.csproj"
    ScriptsDir     = "src/auth/Projeto.Moope.Auth.Infrastructure/Scripts"
    LatestScript   = "20260415004544_AjusteColunas.sql"
    Order          = 1
  },
  @{
    Name           = "Auth Identity"
    Context        = "AppIdentityDbContext"
    Project        = "src/auth/Projeto.Moope.Auth.Infrastructure/Projeto.Moope.Auth.Infrastructure.csproj"
    StartupProject = "src/auth/Projeto.Moope.Auth.Api/Projeto.Moope.Auth.Api.csproj"
    ScriptsDir     = "src/auth/Projeto.Moope.Auth.Infrastructure/Scripts"
    LatestScript   = "20260407013320_InitialIdentity.sql"
    Order          = 2
  },
  @{
    Name           = "Plano"
    Context        = "AppPlanoContext"
    Project        = "src/plano/Projeto.Moope.Plano.Infrastructure/Projeto.Moope.Plano.Infrastructure.csproj"
    StartupProject = "src/plano/Projeto.Moope.Plano.Api/Projeto.Moope.Plano.Api.csproj"
    ScriptsDir     = "src/plano/Projeto.Moope.Plano.Infrastructure/Scripts"
    LatestScript   = "20260408003555_Plano.sql"
    Order          = 3
  },
  @{
    Name           = "Vendedor"
    Context        = "AppVendedorContext"
    Project        = "src/vendedor/Projeto.Moope.Vendedor.Infrastructure/Projeto.Moope.Vendedor.Infrastructure.csproj"
    StartupProject = "src/vendedor/Projeto.Moope.Vendedor.Api/Projeto.Moope.Vendedor.Api.csproj"
    ScriptsDir     = "src/vendedor/Projeto.Moope.Vendedor.Infrastructure/Scripts"
    LatestScript   = "20260415005730_AjusteColunas.sql"
    Order          = 4
  },
  @{
    Name           = "Endereco"
    Context        = "AppEnderecoContext"
    Project        = "src/endereco/Projeto.Moope.Endereco.Infrastructure/Projeto.Moope.Endereco.Infrastructure.csproj"
    StartupProject = "src/endereco/Projeto.Moope.Endereco.Api/Projeto.Moope.Endereco.Api.csproj"
    ScriptsDir     = "src/endereco/Projeto.Moope.Endereco.Infrastructure/Scripts"
    LatestScript   = "20260409183729_InitialEndereco.sql"
    Order          = 5
  },
  @{
    Name           = "Cliente"
    Context        = "AppClienteContext"
    Project        = "src/cliente/Projeto.Moope.Cliente.Infrastructure/Projeto.Moope.Cliente.Infrastructure.csproj"
    StartupProject = "src/cliente/Projeto.Moope.Cliente.Api/Projeto.Moope.Cliente.Api.csproj"
    ScriptsDir     = "src/cliente/Projeto.Moope.Cliente.Infrastructure/Scripts"
    LatestScript   = "20260421133910_AjusteColunaGalaxPay.sql"
    Order          = 6
  },
  @{
    Name           = "Pedido"
    Context        = "Projeto.Moope.Pedido.Infrastructure.Data.AppPedidoContext"
    Project        = "src/pedido/Projeto.Moope.Pedido.Infrastructure/Projeto.Moope.Pedido.Infrastructure.csproj"
    StartupProject = "src/pedido/Projeto.Moope.Pedido.Api/Projeto.Moope.Pedido.Api.csproj"
    ScriptsDir     = "src/pedido/Projeto.Moope.Pedido.Infrastructure/Scripts"
    LatestScript   = "20260530003149_AjustePedidoPlataforma.sql"
    Order          = 7
  },
  @{
    Name           = "Pagamento"
    Context        = "Projeto.Moope.Pagamento.Infrastructure.Data.AppPagamentoContext"
    Project        = "src/pagamento/Projeto.Moope.Pagamento.Infrastructure/Projeto.Moope.Pagamento.Infrastructure.csproj"
    StartupProject = "src/pagamento/Projeto.Moope.Pagamento.Api/Projeto.Moope.Pagamento.Api.csproj"
    ScriptsDir     = "src/pagamento/Projeto.Moope.Pagamento.Infrastructure/Scripts"
    LatestScript   = "20260425203031_RemoveUniquePagamentoIndexes.sql"
    Order          = 8
  },
  @{
    Name           = "Comodato"
    Context        = "Projeto.Moope.Comodato.Infrastructure.Data.AppComodatoContext"
    Project        = "src/comodato/Projeto.Moope.Comodato.Infrastructure/Projeto.Moope.Comodato.Infrastructure.csproj"
    StartupProject = "src/comodato/Projeto.Moope.Comodato.Api/Projeto.Moope.Comodato.Api.csproj"
    ScriptsDir     = "src/comodato/Projeto.Moope.Comodato.Infrastructure/Scripts"
    LatestScript   = "20260606120000_InitialComodato.sql"
    Order          = 9
  }
)

function Resolve-Utf8Encoding {
  param([string]$encodingName)

  switch ($encodingName) {
    "utf8NoBom" { return [System.Text.UTF8Encoding]::new($false) }
    "utf8Bom" { return [System.Text.UTF8Encoding]::new($true) }
    default { throw "Unsupported encoding: $encodingName" }
  }
}

function Ensure-DirectoryExists {
  param([string]$path)

  if (-not (Test-Path -LiteralPath $path)) {
    New-Item -ItemType Directory -Path $path -Force | Out-Null
  }
}

function Remove-SqlComments {
  param([string]$sql)

  $result = [System.Text.RegularExpressions.Regex]::Replace(
    $sql,
    '/\*[\s\S]*?\*/',
    ''
  )
  $lines = $result -split "`r?`n" | ForEach-Object {
    if ($_ -match '--') {
      $_.Substring(0, $_.IndexOf('--'))
    }
    else {
      $_
    }
  }
  return ($lines -join "`n")
}

function Split-SqlStatements {
  param([string]$sql)

  $cleanSql = Remove-SqlComments -sql $sql
  $statements = New-Object System.Collections.Generic.List[string]
  $current = New-Object System.Text.StringBuilder

  foreach ($line in ($cleanSql -split "`r?`n")) {
    $trimmedLine = $line.Trim()
    if ([string]::IsNullOrWhiteSpace($trimmedLine)) {
      continue
    }

    [void]$current.AppendLine($line)

    if ($trimmedLine.EndsWith(';')) {
      $statement = $current.ToString().Trim()
      if (-not [string]::IsNullOrWhiteSpace($statement)) {
        $statements.Add($statement)
      }
      $current = New-Object System.Text.StringBuilder
    }
  }

  $remaining = $current.ToString().Trim()
  if (-not [string]::IsNullOrWhiteSpace($remaining)) {
    $statements.Add($remaining)
  }

  return $statements
}

function Normalize-SqlStatement {
  param([string]$statement)

  $normalized = $statement.Trim().TrimEnd(';').Trim()
  $normalized = [System.Text.RegularExpressions.Regex]::Replace($normalized, '\s+', ' ')
  return $normalized.ToLowerInvariant()
}

function Test-IsTransactionControl {
  param([string]$normalizedStatement)

  return $normalizedStatement -eq 'start transaction' -or $normalizedStatement -eq 'commit'
}

function Test-IsMigrationsHistoryInsert {
  param([string]$normalizedStatement)

  return $normalizedStatement -like 'insert into `__efmigrationshistory`*'
}

function Test-IsMigrationsHistoryTable {
  param([string]$normalizedStatement)

  return $normalizedStatement -like 'create table if not exists `__efmigrationshistory`*' `
    -or $normalizedStatement -like 'create table `__efmigrationshistory`*'
}

function Test-IsAlterDatabaseCharset {
  param([string]$normalizedStatement)

  return $normalizedStatement -eq 'alter database character set utf8mb4'
}

function Get-ExistingContextScript {
  param([hashtable]$contextConfig)

  $fullDir = Join-Path $resolvedRoot ($contextConfig.ScriptsDir -replace '/', '\')
  if (-not (Test-Path -LiteralPath $fullDir)) {
    throw "Scripts directory not found: $fullDir"
  }

  $scriptFile = Join-Path $fullDir $contextConfig.LatestScript
  if (-not (Test-Path -LiteralPath $scriptFile)) {
    throw "Script file not found: $scriptFile"
  }

  return $scriptFile
}

function Invoke-EfMigrationScript {
  param(
    [hashtable]$contextConfig,
    [string]$outputFile
  )

  $projectPath = Join-Path $resolvedRoot ($contextConfig.Project -replace '/', '\')
  $startupPath = Join-Path $resolvedRoot ($contextConfig.StartupProject -replace '/', '\')

  if (-not (Test-Path -LiteralPath $projectPath)) {
    throw "Project not found: $projectPath"
  }
  if (-not (Test-Path -LiteralPath $startupPath)) {
    throw "Startup project not found: $startupPath"
  }

  Ensure-DirectoryExists -path (Split-Path -Parent $outputFile)

  $arguments = @(
    "ef", "migrations", "script",
    "--project", $projectPath,
    "--startup-project", $startupPath,
    "--context", $contextConfig.Context,
    "--output", $outputFile
  )

  Write-Host "Generating script for $($contextConfig.Name) ($($contextConfig.Context))..."
  & dotnet @arguments
  if ($LASTEXITCODE -ne 0) {
    throw "dotnet ef migrations script failed for $($contextConfig.Context) (exit code: $LASTEXITCODE)"
  }
}

function Merge-SqlScripts {
  param(
    [string[]]$sourceFiles,
    [string[]]$sourceLabels
  )

  $seenKeys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
  $seenHistoryTables = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
  $mergedStatements = New-Object System.Collections.Generic.List[string]
  $migrationInserts = New-Object System.Collections.Generic.List[string]
  $alterDatabaseAdded = $false

  for ($i = 0; $i -lt $sourceFiles.Count; $i++) {
    $filePath = $sourceFiles[$i]
    $label = $sourceLabels[$i]
    $content = Get-Content -Raw -LiteralPath $filePath

    if ([string]::IsNullOrWhiteSpace($content)) {
      continue
    }

    [void]$mergedStatements.Add("")
    [void]$mergedStatements.Add("-- $($label)")

    foreach ($statement in (Split-SqlStatements -sql $content)) {
      $normalized = Normalize-SqlStatement -statement $statement

      if ([string]::IsNullOrWhiteSpace($normalized)) {
        continue
      }

      if (Test-IsTransactionControl -normalizedStatement $normalized) {
        continue
      }

      if (Test-IsMigrationsHistoryTable -normalizedStatement $normalized) {
        if (-not $seenHistoryTables.Add($normalized)) {
          continue
        }
        [void]$mergedStatements.Add($statement.Trim())
        continue
      }

      if (Test-IsAlterDatabaseCharset -normalizedStatement $normalized) {
        if (-not $alterDatabaseAdded) {
          $alterDatabaseAdded = $true
          [void]$mergedStatements.Add($statement.Trim())
        }
        continue
      }

      if (Test-IsMigrationsHistoryInsert -normalizedStatement $normalized) {
        if (-not $seenKeys.Add($normalized)) {
          continue
        }
        $migrationInserts.Add($statement.Trim())
        continue
      }

      if (-not $seenKeys.Add($normalized)) {
        continue
      }

      [void]$mergedStatements.Add($statement.Trim())
    }
  }

  foreach ($insert in $migrationInserts) {
    [void]$mergedStatements.Add($insert)
  }

  return $mergedStatements
}

Ensure-DirectoryExists -path (Split-Path -Parent $outPath)
Ensure-DirectoryExists -path $tmpDir

$sourceFiles = New-Object System.Collections.Generic.List[string]
$sourceLabels = New-Object System.Collections.Generic.List[string]

$sortedContexts = @($dbContexts | Sort-Object { $_.Order })

foreach ($ctx in $sortedContexts) {
  if ($SkipRegenerate) {
    $sourcePath = Get-ExistingContextScript -contextConfig $ctx
    Write-Host "Using existing script for $($ctx.Name): $sourcePath"
  }
  else {
    $safeName = ($ctx.Context -replace '[^a-zA-Z0-9_]', '_')
    $sourcePath = Join-Path $tmpDir "$safeName.sql"
    Invoke-EfMigrationScript -contextConfig $ctx -outputFile $sourcePath
  }

  $sourceFiles.Add($sourcePath)
  $sourceLabels.Add("$($ctx.Name) ($($ctx.Context))")
}

$mergedStatements = Merge-SqlScripts -sourceFiles $sourceFiles.ToArray() -sourceLabels $sourceLabels.ToArray()
$migrationCount = @($mergedStatements | Where-Object { $_ -match 'INSERT INTO `__EFMigrationsHistory' }).Count

$header = @(
  "/*",
  "  Moope - Schema completo para producao (greenfield)",
  "  Gerado por: publish/sql/generate-production-sql.ps1",
  "  Repo root: $resolvedRoot",
  "  Gerado em: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')",
  "  DbContexts: $($sortedContexts.Count)",
  "  Migrations registradas: $migrationCount",
  "  Executar: mysql -u USER -p plataforma_moope < publish/sql/production.sql",
  "  ATENCAO: Executar primeiro em staging/backup antes de producao.",
  "*/"
) -join "`n"

$output = New-Object System.Text.StringBuilder
[void]$output.AppendLine($header)
[void]$output.AppendLine("")
[void]$output.AppendLine("START TRANSACTION;")
[void]$output.AppendLine("")

foreach ($statement in $mergedStatements) {
  if ([string]::IsNullOrWhiteSpace($statement)) {
    [void]$output.AppendLine("")
    continue
  }

  [void]$output.AppendLine($statement)

  $trimmedStatement = $statement.Trim()
  if ($trimmedStatement.StartsWith('--')) {
    [void]$output.AppendLine("")
    continue
  }

  if (-not $trimmedStatement.EndsWith(';')) {
    [void]$output.AppendLine(';')
  }

  [void]$output.AppendLine("")
}

[void]$output.AppendLine("COMMIT;")

$utf8 = Resolve-Utf8Encoding -encodingName $encoding
[System.IO.File]::WriteAllText($outPath, $output.ToString(), $utf8)

Write-Host ""
Write-Host "Script gerado: $outPath"
Write-Host "Migrations registradas: $migrationCount"
Write-Host "Statements unicos: $($mergedStatements.Count)"

if ($migrationCount -lt 20) {
  Write-Warning "Esperado 20 INSERTs em __EFMigrationsHistory, encontrado $migrationCount."
}

if (-not $SkipRegenerate) {
  Write-Host "Arquivos temporarios em: $tmpDir"
}
