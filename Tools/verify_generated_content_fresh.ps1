$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    python Tools\generate_tmod_content.py | Out-Host
    $diff = git status --short -- `
        Content\Items\Generated `
        Content\Items\BossSummons\Generated `
        Content\Projectiles\Generated `
        Content\Tiles\Generated `
        Content\NPCs\Enemies\Generated `
        Content\NPCs\Bosses\Generated `
        Localization\generated.en-US.hjson `
        Localization\generated.zh-Hans.hjson `
        Localization\generated_bestiary.en-US.hjson `
        Localization\generated_bestiary.zh-Hans.hjson

    if ($diff) {
        Write-Host "Generated content is stale:"
        Write-Host $diff
        exit 1
    }

    Write-Host "Generated content verified fresh."
}
finally {
    Pop-Location
}
