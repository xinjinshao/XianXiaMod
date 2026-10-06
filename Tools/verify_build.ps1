param(
    [Parameter(Mandatory = $true)][string]$TModLoaderDir,
    [string]$DotNetPath = "dotnet",
    [string]$SaveDir,
    [string[]]$AdditionalModPaths = @(),
    [hashtable]$AdditionalModDisplayNames = @{},
    [switch]$SkipServerLoad,
    [switch]$ExportEconomyAudit,
    [int]$LoadWaitSeconds = 40
)

$ErrorActionPreference = "Stop"
if ($ExportEconomyAudit -and $SkipServerLoad) { throw "Economy export requires server loading; omit -SkipServerLoad." }
function Read-SharedLog([string]$Path) {
    if (!(Test-Path -LiteralPath $Path)) { return "" }
    $stream = [IO.File]::Open($Path, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
    $reader = [IO.StreamReader]::new($stream)
    try { return $reader.ReadToEnd() } finally { $reader.Dispose() }
}
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$tmlRoot = (Resolve-Path -LiteralPath $TModLoaderDir).Path
$targets = Join-Path $tmlRoot "tMLMod.targets"
if (!(Test-Path -LiteralPath $targets)) { throw "Missing tModLoader targets: $targets" }
$dotnetExe = (Get-Command $DotNetPath -ErrorAction Stop).Source
if (!$SaveDir) { $SaveDir = Join-Path $repoRoot ".tml-test" }
$runRoot = Join-Path ([IO.Path]::GetFullPath($SaveDir)) ("run-" + [guid]::NewGuid().ToString("N"))
$sourceRoot = Join-Path $runRoot "ModSources\XianXia"
$saveRoot = Join-Path $runRoot "save"
New-Item -ItemType Directory -Force -Path $sourceRoot, $saveRoot | Out-Null

# The directory name is the internal mod name. Runtime assets live in these folders;
# art working files, docs, tests and previous build outputs must not enter the package.
foreach ($folder in @("Common", "Content", "Localization")) {
    robocopy (Join-Path $repoRoot $folder) (Join-Path $sourceRoot $folder) /E /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Copy failed: $folder ($LASTEXITCODE)" }
}
foreach ($file in @("XianXia.cs", "XianXia.csproj", "build.txt", "description.txt", "icon.png", "icon_small.png")) {
    $path = Join-Path $repoRoot $file
    if (Test-Path -LiteralPath $path) { Copy-Item -LiteralPath $path -Destination $sourceRoot }
}

Push-Location $repoRoot
try {
    & $dotnetExe build XianXia.csproj "-p:TModLoaderTargets=$targets" "-p:BuildMod=false"
    if ($LASTEXITCODE -ne 0) { throw "Compile failed: $LASTEXITCODE" }
} finally { Pop-Location }

Push-Location $tmlRoot
try {
    & $dotnetExe tModLoader.dll -server -nosteam -build $sourceRoot -eac (Join-Path $repoRoot "bin\Debug\net8.0\XianXia.dll") -tmlsavedirectory $saveRoot
    if ($LASTEXITCODE -ne 0) { throw "Package failed: $LASTEXITCODE" }
} finally { Pop-Location }

$mods = Join-Path $saveRoot "Mods"
$package = Join-Path $mods "XianXia.tmod"
if (!(Test-Path -LiteralPath $package)) { throw "Missing package: $package" }
Write-Output "Package verified: $package"
if ($SkipServerLoad) { exit 0 }
$enabledMods = @("XianXia")
foreach ($additionalPath in $AdditionalModPaths) {
    $resolvedMod = (Resolve-Path -LiteralPath $additionalPath).Path
    if ([IO.Path]::GetExtension($resolvedMod) -ne ".tmod") { throw "Expected .tmod package: $resolvedMod" }
    $modName = [IO.Path]::GetFileNameWithoutExtension($resolvedMod)
    if ($enabledMods -contains $modName) { throw "Duplicate mod package: $modName" }
    Copy-Item -LiteralPath $resolvedMod -Destination (Join-Path $mods ([IO.Path]::GetFileName($resolvedMod)))
    $enabledMods += $modName
}
ConvertTo-Json -InputObject $enabledMods -Compress | Set-Content -LiteralPath (Join-Path $mods "enabled.json") -Encoding UTF8
$stdout = Join-Path $runRoot "server.log"
$stderr = Join-Path $runRoot "server.err.log"
$arguments = @("tModLoader.dll", "-server", "-nosteam", "-tmlsavedirectory", ('"' + $saveRoot + '"'), "-modpath", ('"' + $mods + '"'))
$previousEconomyExport = [Environment]::GetEnvironmentVariable("XIANXIA_EXPORT_ECONOMY", "Process")
try {
    [Environment]::SetEnvironmentVariable("XIANXIA_EXPORT_ECONOMY", $(if ($ExportEconomyAudit) { "1" } else { $null }), "Process")
    $process = Start-Process -FilePath $dotnetExe -ArgumentList $arguments -WorkingDirectory $tmlRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
} finally {
    [Environment]::SetEnvironmentVariable("XIANXIA_EXPORT_ECONOMY", $previousEconomyExport, "Process")
}
$timer = [Diagnostics.Stopwatch]::StartNew()
try {
    do {
        Start-Sleep -Seconds 1
        $log = Read-SharedLog $stdout
        $loaded = $log -match "Adding Content: XianXia" -and $log -match "Finalizing Content: XianXia" -and $log -match "Adding Recipes" -and $log -match "Choose World"
        foreach ($modName in $enabledMods) {
            $displayName = if ($AdditionalModDisplayNames.ContainsKey($modName)) { $AdditionalModDisplayNames[$modName] } else { $modName }
            $escapedName = [regex]::Escape($displayName)
            $loaded = $loaded -and $log -match "Adding Content: $escapedName(?:\s|\()" -and $log -match "Finalizing Content: $escapedName(?:\s|\()"
        }
    } while (!$loaded -and !$process.HasExited -and $timer.Elapsed.TotalSeconds -lt $LoadWaitSeconds)
} finally {
    if (!$process.HasExited) { $process.Kill(); $process.WaitForExit() }
}
$engineLog = Join-Path $tmlRoot "tModLoader-Logs\server.log"
if (Test-Path -LiteralPath $engineLog) {
    Copy-Item -LiteralPath $engineLog -Destination (Join-Path $runRoot "tmodloader-server.log")
}
$errors = Read-SharedLog $stderr
if (!$loaded -or $errors.Trim().Length -gt 0) {
    throw "Server load not verified. Logs: $stdout; $stderr`n$log`n$errors"
}
Write-Output "Dedicated-server load passed. Logs: $stdout"

if ($ExportEconomyAudit) {
    $auditPath = Join-Path $saveRoot "XianXia/economy-audit.json"
    if (!(Test-Path -LiteralPath $auditPath)) { throw "Economy snapshot missing: $auditPath" }
    $audit = Get-Content -LiteralPath $auditPath -Raw | ConvertFrom-Json
    if ($audit.schema -ne 1 -or !$audit.items.Count -or !$audit.recipes.Count -or !$audit.shops.Count -or $audit.prices.Count -ne 3) { throw "Incomplete economy snapshot: $auditPath" }
    Copy-Item -LiteralPath $auditPath -Destination (Join-Path $runRoot "economy-audit.json")
    Write-Output "Economy registrations exported: $(Join-Path $runRoot 'economy-audit.json')"
}
