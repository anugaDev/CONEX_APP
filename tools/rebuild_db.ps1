# Reconstruye conex_app.db desde cero a partir de Conex_bs.mdb.
#  1. Genera tools/import_data.sql (import_data_mdb.ps1)
#  2. Crea una BD nueva con sqlite3 ".read" (lee el fichero como UTF-8, sin pasar por el pipe de PowerShell)
#  3. Conserva la configuracion (AppSettings) de la BD anterior
#  4. Hace copia de seguridad de las BD existentes y las sustituye (raiz del proyecto + bin\Debug)
#
# Uso: powershell -ExecutionPolicy Bypass -File tools\rebuild_db.ps1 [-MdbPath <ruta>] [-Sqlite <ruta sqlite3.exe>]
param(
    [string]$MdbPath = "F:\CONEX - PROGRAMA PC\CONEX - PROGRAMA PC\Conex_bs.mdb",
    [string]$Sqlite = "sqlite3"
)
$ErrorActionPreference = "Stop"

$root = Split-Path $PSScriptRoot -Parent
$sqlFile = Join-Path $PSScriptRoot "import_data.sql"
$targets = @(
    (Join-Path $root "conex_app.db"),
    (Join-Path $root "bin\Debug\net8.0-windows\conex_app.db")
) | Where-Object { Test-Path (Split-Path $_ -Parent) }

& powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot "import_data_mdb.ps1") -MdbPath $MdbPath -OutPath $sqlFile
if ($LASTEXITCODE -ne 0) { throw "Error generando el SQL" }

$newDb = Join-Path $env:TEMP ("conex_app_" + [guid]::NewGuid().ToString("N") + ".db")
& $Sqlite $newDb ".read '$($sqlFile.Replace('\','/'))'"
if ($LASTEXITCODE -ne 0) { throw "Error ejecutando el SQL en sqlite3" }

# Conservar configuracion de la BD que usa la app (la de bin si existe, si no la de la raiz)
$previous = $targets | Where-Object { Test-Path $_ } | Sort-Object { (Get-Item $_).LastWriteTime } -Descending | Select-Object -First 1
if ($previous) {
    $prevPath = $previous.Replace('\','/')
    & $Sqlite $newDb "ATTACH '$prevPath' AS old; INSERT OR REPLACE INTO main.AppSettings SELECT Id, RenewalPeriodMonths, RenewalCost, ClassCost FROM old.AppSettings;" 2>$null
}

$stamp = Get-Date -Format "yyyyMMdd_HHmmss"
foreach ($t in $targets) {
    if (Test-Path $t) { Copy-Item $t "$t.bak_$stamp" }
    Copy-Item $newDb $t -Force
    Write-Host "BD actualizada: $t" -ForegroundColor Green
}
Remove-Item $newDb

# Evita que la app vuelva a ejecutar import_data.sql sobre la BD nueva
Set-Content -Path "$sqlFile.imported" -Value ("Imported on " + (Get-Date -Format "yyyy-MM-dd HH:mm:ss"))
