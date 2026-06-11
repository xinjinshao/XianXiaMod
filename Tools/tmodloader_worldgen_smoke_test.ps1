param(
    [string]$TModLoaderDir = "D:\Program Files (x86)\Steam\steamapps\common\tModLoader",
    [string]$SaveDir = "E:\XianXia\.tml-test-worldgen",
    [int]$LoadWaitSeconds = 90
)

$ErrorActionPreference = "Stop"

$repo = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$modSourcesRoot = Join-Path $env:USERPROFILE "Documents\My Games\Terraria\tModLoader\ModSources"
$modSource = Join-Path $modSourcesRoot "XianXia"
$modsDir = Join-Path $env:USERPROFILE "Documents\My Games\Terraria\tModLoader\Mods"
$tmod = Join-Path $modsDir "XianXia.tmod"

if (!(Test-Path $TModLoaderDir)) {
    throw "tModLoader directory not found: $TModLoaderDir"
}

New-Item -ItemType Directory -Force -Path $modSource | Out-Null
robocopy $repo $modSource /MIR /XD .git bin obj .vs .idea .tml-test .tml-test-worldgen /XF *.user | Out-Null
if ($LASTEXITCODE -ge 8) {
    throw "robocopy failed with exit code $LASTEXITCODE"
}

Push-Location $modSource
try {
    dotnet build XianXia.csproj
}
finally {
    Pop-Location
}

if (!(Test-Path $tmod)) {
    throw "Build did not produce installed tmod: $tmod"
}

$testMods = Join-Path $SaveDir "Mods"
$worldDir = Join-Path $SaveDir "Worlds"
New-Item -ItemType Directory -Force -Path $testMods, $worldDir | Out-Null
Copy-Item $tmod (Join-Path $testMods "XianXia.tmod") -Force
'["XianXia"]' | Set-Content -Path (Join-Path $testMods "enabled.json") -Encoding UTF8

$worldPath = Join-Path $worldDir "XianXiaWorldgenSmoke.wld"
Remove-Item $worldPath, ($worldPath + ".bak") -ErrorAction SilentlyContinue

$stdout = Join-Path $SaveDir "worldgen-smoke.log"
$stderr = Join-Path $SaveDir "worldgen-smoke.err.log"
Remove-Item $stdout, $stderr -ErrorAction SilentlyContinue

$dotnet = Join-Path $TModLoaderDir "dotnet\dotnet.exe"
$args = @(
    "tModLoader.dll",
    "-server",
    "-nosteam",
    "-tmlsavedirectory",
    $SaveDir,
    "-modpath",
    $testMods,
    "-world",
    $worldPath,
    "-autocreate",
    "1",
    "-worldname",
    "XianXiaWorldgenSmoke",
    "-seed",
    "XianXiaSmokeSeed"
)

$process = Start-Process -FilePath $dotnet -ArgumentList $args -WorkingDirectory $TModLoaderDir -NoNewWindow -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
Start-Sleep -Seconds $LoadWaitSeconds
if (!$process.HasExited) {
    $process.Kill()
    $process.WaitForExit()
}

$outText = if (Test-Path $stdout) { [string](Get-Content $stdout -Raw) } else { "" }
$errText = if (Test-Path $stderr) { [string](Get-Content $stderr -Raw) } else { "" }
if ($null -eq $outText) { $outText = "" }
if ($null -eq $errText) { $errText = "" }

if ($errText.Trim().Length -gt 0) {
    throw "tModLoader worldgen smoke test wrote stderr:`n$errText"
}

if (!(Test-Path $worldPath)) {
    throw "tModLoader worldgen smoke test did not create world. Output:`n$outText"
}

if ($outText -match "Exception|Error|failed|Failed") {
    throw "tModLoader worldgen smoke test output contained an error marker. Output:`n$outText"
}

Write-Output "tModLoader worldgen smoke test passed: world generated with XianXia loaded."
