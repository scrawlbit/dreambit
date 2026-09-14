# Launcher da galeria de exemplos do DreamBit.
# Uso:
#   .\run.ps1                 -> menu interativo
#   .\run.ps1 05-lights       -> roda um exemplo direto
#   .\run.ps1 -List           -> lista os exemplos
param([string]$Name, [switch]$List)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot            # raiz do repo (Examples/ fica na raiz)
$proj = Join-Path $root "DreamBit.Player\DreamBit.Player.csproj"
$exe  = Join-Path $root "DreamBit.Player\bin\Debug\net8.0\DreamBit.Player.exe"

if (-not (Test-Path $exe)) {
    Write-Host "Compilando o Player..." -ForegroundColor Yellow
    $env:DOTNET_SYSTEM_NET_DISABLEIPV6 = "1"
    dotnet build $proj -c Debug --nologo | Out-Null
}

if ($List) { & $exe --examples-list; return }

if ($Name) { & $exe --example $Name; return }

# Menu interativo
$items = & $exe --examples-list
Write-Host ""
Write-Host "  DreamBit - Galeria de Exemplos" -ForegroundColor Cyan
Write-Host "  ------------------------------"
$i = 1
$names = @()
foreach ($line in $items) {
    $n = ($line -split '\s+')[0]
    $names += $n
    "{0,2}) {1}" -f $i, $line | Write-Host
    $i++
}
Write-Host ""
$sel = Read-Host "Escolha um numero (ou Enter para sair)"
if ($sel -and ($sel -as [int]) -ge 1 -and ($sel -as [int]) -le $names.Count) {
    & $exe --example $names[[int]$sel - 1]
}
