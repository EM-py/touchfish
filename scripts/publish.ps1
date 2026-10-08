param([string]$Tag=('v'+(Get-Content -LiteralPath (Join-Path $PSScriptRoot '..\VERSION') -Raw).Trim()))
$ErrorActionPreference='Stop'
$taskRoot=Split-Path -Parent $PSScriptRoot
if($Tag -notmatch '^v\d+\.\d+\.\d+$'){throw 'Use a stable version tag such as v0.2.0.'}
if((Get-Content -LiteralPath (Join-Path $taskRoot 'VERSION') -Raw).Trim() -ne $Tag.Substring(1)){throw 'Tag and VERSION differ.'}
gh release view $Tag --repo YuziPlus/touchfish 2>$null
if($LASTEXITCODE -eq 0){throw 'Release already exists; publish a new version instead of replacing it.'}
& (Join-Path $taskRoot 'build.ps1')
$taskPreview=Join-Path $taskRoot 'previews'
$taskProcess=Start-Process -FilePath (Join-Path $taskRoot 'Touchfish.exe') -ArgumentList '--preview',$taskPreview -WindowStyle Hidden -PassThru
$taskProcess.WaitForExit()
if($taskProcess.ExitCode -ne 0){throw 'Preview and integration verification failed.'}
$taskAssets=Join-Path $taskRoot 'work\release-assets'
python (Join-Path $taskRoot 'scripts\make_release.py') --version $Tag --out $taskAssets
if($LASTEXITCODE -ne 0){throw 'Package build failed.'}
$taskNotes=Join-Path $taskAssets 'release-notes.md'
Get-Content -LiteralPath (Join-Path $taskRoot 'CHANGELOG.md') -Raw | Set-Content -LiteralPath $taskNotes -Encoding utf8
gh release create $Tag --repo YuziPlus/touchfish --target main --draft --title "Touchfish $Tag" --notes-file $taskNotes (Join-Path $taskAssets "Touchfish-$Tag-win.zip") (Join-Path $taskAssets 'Touchfish-update.json') (Join-Path $taskAssets "Touchfish-$Tag-win.zip.sha256")
if($LASTEXITCODE -ne 0){throw 'Draft release upload failed.'}
gh release edit $Tag --repo YuziPlus/touchfish --draft=false --latest
if($LASTEXITCODE -ne 0){throw 'Release remains draft; inspect GitHub.'}
