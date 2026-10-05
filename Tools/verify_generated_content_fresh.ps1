param([string]$PythonPath = "python")

$ErrorActionPreference = "Stop"
$pythonExe = (Get-Command $PythonPath -ErrorAction Stop).Source
if ($pythonExe -like "*\Microsoft\WindowsApps\*") {
    throw "Python resolves to a WindowsApps alias. Pass -PythonPath with an actual Python executable."
}
& $pythonExe (Join-Path $PSScriptRoot "verify_generated_localization.py")
if ($LASTEXITCODE -ne 0) {
    throw "Generated localization verification failed: $LASTEXITCODE"
}
