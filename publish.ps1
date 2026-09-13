<#
    publish.ps1 — empacota o DreamBit para distribuicao (self-contained).

    Gera binarios self-contained (o usuario final NAO precisa ter o .NET instalado)
    em dist/<projeto>/<rid>/. Empacota, opcionalmente, um .zip por saida.

    IMPORTANTE — por que NAO single-file / NAO trim:
      O componente de Script (Roslyn) monta as referencias de compilacao a partir de
      Assembly.Location de cada assembly carregado. Em publish single-file esses
      caminhos ficam vazios e a compilacao de scripts em runtime quebra; o trimming
      pode remover tipos usados por reflexao. Por isso publicamos self-contained em
      PASTA, sem single-file e sem trim.

    Uso:
      pwsh ./publish.ps1                      # editores para o SO atual + player p/ win/linux/osx
      pwsh ./publish.ps1 -Rids win-x64        # so um RID
      pwsh ./publish.ps1 -Zip                 # tambem compacta cada saida em .zip
      pwsh ./publish.ps1 -Configuration Debug
#>
param(
    [string[]] $Rids = @(),
    [string]   $Configuration = "Release",
    [switch]   $Zip
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$dist = Join-Path $root "dist"

# RID padrao do SO atual, para os editores.
function Get-DefaultRid {
    if ($IsWindows -or $env:OS -eq "Windows_NT") { return "win-x64" }
    if ($IsMacOS) { return "osx-x64" }
    return "linux-x64"
}

# Editor e Player sao cross-platform; publica para os tres por padrao (ou o -Rids informado).
$allRids = if ($Rids.Count -gt 0) { $Rids } else { @("win-x64", "linux-x64", "osx-x64") }

$targets = @(
    @{ Name = "DreamBit.Studio.Avalonia";  Proj = "DreamBit.Studio.Avalonia/DreamBit.Studio.Avalonia.csproj"; Rids = $allRids },
    @{ Name = "DreamBit.Player";           Proj = "DreamBit.Player/DreamBit.Player.csproj";                   Rids = $allRids }
)

Write-Host "DreamBit — publish ($Configuration)" -ForegroundColor Cyan

foreach ($t in $targets) {
    foreach ($rid in $t.Rids) {
        if (-not $rid) { continue }
        $out = Join-Path $dist "$($t.Name)/$rid"
        Write-Host "`n==> $($t.Name)  [$rid]" -ForegroundColor Yellow

        dotnet publish $t.Proj `
            -c $Configuration `
            -r $rid `
            --self-contained true `
            -p:PublishSingleFile=false `
            -p:PublishTrimmed=false `
            -o $out
        if ($LASTEXITCODE -ne 0) { throw "Falha ao publicar $($t.Name) [$rid]" }

        if ($Zip) {
            $zipPath = Join-Path $dist "$($t.Name)-$rid.zip"
            if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
            Compress-Archive -Path (Join-Path $out '*') -DestinationPath $zipPath
            Write-Host "    zip: $zipPath" -ForegroundColor DarkGray
        }
    }
}

Write-Host "`nPronto. Saidas em: $dist" -ForegroundColor Green
