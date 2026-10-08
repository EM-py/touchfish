$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$referenceRoot = Join-Path $taskRoot 'work\reference\hearthstone'
New-Item -ItemType Directory -Force -Path $referenceRoot | Out-Null
[System.IO.File]::WriteAllText((Join-Path $referenceRoot '__init__.py'), '')
Invoke-WebRequest 'https://raw.githubusercontent.com/HearthSim/python-hearthstone/master/hearthstone/deckstrings.py' -OutFile (Join-Path $referenceRoot 'deckstrings.py')
Invoke-WebRequest 'https://raw.githubusercontent.com/HearthSim/python-hearthstone/master/hearthstone/enums.py' -OutFile (Join-Path $referenceRoot 'enums.py')
Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $referenceRoot 'deckstrings.py'),(Join-Path $referenceRoot 'enums.py') | Select-Object Path,Hash
