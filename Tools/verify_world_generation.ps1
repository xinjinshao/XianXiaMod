param(
    [Parameter(Mandatory = $true)][string]$TModLoaderDir,
    [Parameter(Mandatory = $true)][string]$PackagePath,
    [string]$DotNetPath = "dotnet",
    [ValidateSet(1, 2, 3)][int]$WorldSize = 1,
    [string]$Seed = "20261005",
    [ValidateSet(0, 1, 2)][int]$Difficulty = 0,
    [string]$ExistingWorld
)

$ErrorActionPreference = "Stop"
if ($Seed -match "[\r\n]") { throw "Seed must be a single line." }
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$tmlRoot = (Resolve-Path -LiteralPath $TModLoaderDir).Path
$package = (Resolve-Path -LiteralPath $PackagePath).Path
$dotnetExe = (Get-Command $DotNetPath -ErrorAction Stop).Source
$runRoot = Join-Path $repoRoot (".tml-test\world-" + [guid]::NewGuid().ToString("N"))
$saveRoot = Join-Path $runRoot "save"
$mods = Join-Path $saveRoot "Mods"
$world = Join-Path $saveRoot "Worlds\GenerationProbe.wld"
New-Item -ItemType Directory -Force -Path $mods, (Split-Path $world) | Out-Null
Copy-Item -LiteralPath $package -Destination (Join-Path $mods "XianXia.tmod")
'["XianXia"]' | Set-Content -LiteralPath (Join-Path $mods "enabled.json") -Encoding UTF8
if ($ExistingWorld) {
    $existing = (Resolve-Path -LiteralPath $ExistingWorld).Path
    $existingModded = [IO.Path]::ChangeExtension($existing, "twld")
    if (!(Test-Path -LiteralPath $existingModded)) { throw "Missing corresponding TWLD: $existingModded" }
    Copy-Item -LiteralPath $existing -Destination $world
    Copy-Item -LiteralPath $existingModded -Destination ([IO.Path]::ChangeExtension($world, "twld"))
}
$config = Join-Path $runRoot "serverconfig.txt"
$port = Get-Random -Minimum 19000 -Maximum 29000
@(
    "world=$world", "autocreate=$WorldSize", "seed=$Seed", "difficulty=$Difficulty",
    "worldname=GenerationProbe", "maxplayers=1", "ip=127.0.0.1", "port=$port",
    "password=$([guid]::NewGuid().ToString('N'))", "upnp=0", "language=en-US",
    "banlist=$(Join-Path $runRoot 'banlist.txt')", "priority=3"
) | Set-Content -LiteralPath $config -Encoding UTF8
Write-Output "Isolated world-generation run: $runRoot"
Write-Output "Wait for Server started; run xianxia-world-audit to inspect, then exit to save and stop."
Push-Location $tmlRoot
try {
    & $dotnetExe tModLoader.dll -server -nosteam -noupnp -config $config -tmlsavedirectory $saveRoot -modpath $mods
    if ($LASTEXITCODE -ne 0) { throw "World generation/server failed: $LASTEXITCODE" }
} finally { Pop-Location }
if (!(Test-Path -LiteralPath $world) -or (Get-Item -LiteralPath $world).Length -eq 0) {
    throw "No generated world was saved: $world"
}
$runtimeLog = Join-Path $tmlRoot "tModLoader-Logs\server.log"
if (Test-Path -LiteralPath $runtimeLog) { Copy-Item -LiteralPath $runtimeLog -Destination (Join-Path $runRoot "runtime.log") }
Write-Output "Generated world saved: $world"
Write-Output "This proves generation/startup, not biome completeness or playability."
