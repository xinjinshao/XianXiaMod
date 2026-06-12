param(
    [string]$TModLoaderDir = "D:\Program Files (x86)\Steam\steamapps\common\tModLoader",
    [string]$SaveDir = "E:\XianXia\.tml-client-test",
    [int]$LoadWaitSeconds = 90
)

$ErrorActionPreference = "Stop"

$modsSourceDir = Join-Path $env:USERPROFILE "Documents\My Games\Terraria\tModLoader\Mods"
$installedMod = Join-Path $modsSourceDir "XianXia.tmod"
$testMods = Join-Path $SaveDir "Mods"
$clientLog = Join-Path $TModLoaderDir "tModLoader-Logs\client.log"

if (!(Test-Path $TModLoaderDir)) {
    throw "tModLoader directory not found: $TModLoaderDir"
}

if (!(Test-Path $installedMod)) {
    throw "Installed XianXia.tmod not found. Run Tools\tmodloader_smoke_test.ps1 first."
}

Get-CimInstance Win32_Process |
    Where-Object { $_.CommandLine -like "*tModLoader.dll*" } |
    ForEach-Object { Stop-Process -Id $_.ProcessId -Force }

New-Item -ItemType Directory -Force -Path $testMods | Out-Null
Copy-Item $installedMod (Join-Path $testMods "XianXia.tmod") -Force
'["XianXia"]' | Set-Content -Path (Join-Path $testMods "enabled.json") -Encoding UTF8
Remove-Item $clientLog -ErrorAction SilentlyContinue

$dotnet = Join-Path $TModLoaderDir "dotnet\dotnet.exe"
$args = @(
    "tModLoader.dll",
    "-nosteam",
    "-tmlsavedirectory",
    $SaveDir,
    "-modpath",
    $testMods
)

$process = Start-Process -FilePath $dotnet -ArgumentList $args -WorkingDirectory $TModLoaderDir -WindowStyle Hidden -PassThru
Start-Sleep -Seconds $LoadWaitSeconds
if (!$process.HasExited) {
    $process.Kill()
    $process.WaitForExit()
}

if (!(Test-Path $clientLog)) {
    throw "tModLoader client log not found: $clientLog"
}

$logText = [string](Get-Content $clientLog -Raw)
if ($logText -match "MissingResourceException|Asset could not be found|An error occurred while loading XianXia") {
    throw "tModLoader client smoke test detected a XianXia load error:`n$logText"
}

if ($logText -notmatch "Adding Content: XianXia" -or $logText -notmatch "Finalizing Content: XianXia" -or $logText -notmatch "Adding Recipes" -or $logText -notmatch "Mod Load Completed") {
    throw "tModLoader client smoke test did not confirm XianXia load. Output:`n$logText"
}

Write-Output "tModLoader client smoke test passed: XianXia loaded successfully."
