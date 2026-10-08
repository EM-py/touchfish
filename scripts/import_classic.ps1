$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$snapshotFile = Join-Path $taskRoot 'work\hsjson-253932-zhCN.json'
New-Item -ItemType Directory -Force -Path (Split-Path $snapshotFile -Parent) | Out-Null
if (-not (Test-Path -LiteralPath $snapshotFile)) {
    Invoke-WebRequest 'https://api.hearthstonejson.com/v1/253932/zhCN/cards.json' -OutFile $snapshotFile
}
$env:PYTHONIOENCODING = 'utf-8'
python (Join-Path $PSScriptRoot 'import_classic.py') --offline
if ($LASTEXITCODE -ne 0) { throw 'Classic import failed.' }
