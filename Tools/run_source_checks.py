"""Run portable source checks; native engine/graphical acceptance is separate."""
from __future__ import annotations
import argparse
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
# These harnesses link production source and mock the engine boundary.
PROJECTS = (
    "Networking/Networking.csproj", "Progression/Progression.csproj",
    "Biomes/Biomes.csproj", "Plants/Plants.csproj",
    "Companions/Companions.csproj", "ConstructedBiomes/ConstructedBiomes.csproj",
    "Enemies/Enemies.Tests.csproj", "ExpertRewards/ExpertRewards.Tests.csproj",
    "BossScaling/BossScaling.Tests.csproj", "Worms/Worms.Tests.csproj",
    "Telegraphs/Telegraphs.Tests.csproj", "BossPets/BossPets.Tests.csproj",
    "MasterMonuments/MasterMonuments.Tests.csproj",
    "WorldGeneration/WorldGeneration.Tests.csproj",
    "CultivationStatus/CultivationStatus.Tests.csproj",
    "SpiritTide/SpiritTide.Tests.csproj",
    "Fishing/Fishing.Tests.csproj",
    "BossChecklist/BossChecklist.Tests.csproj",
    "Commissions/Commissions.Tests.csproj",
)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--dotnet", default="dotnet", help=".NET 8 SDK executable")
    args = parser.parse_args()
    commands = [[sys.executable, str(ROOT / "Tools" / script)] for script in (
        "Tests/test_localization_scope.py", "Tests/test_png_native_scope.py", "Tests/test_economy_audit.py", "verify_content_contract.py",
        "verify_localization_keys.py", "verify_png_assets.py",
    )]
    for project in PROJECTS:
        command = [args.dotnet, "run", "--project", str(ROOT / "Tools/Tests" / project), "--configuration", "Release"]
        if project.startswith(("CultivationStatus/", "BossChecklist/", "Commissions/")):
            command += ["--", str(ROOT)]
        commands.append(command)
    for index, command in enumerate(commands, 1):
        print(f"Source check {index}/{len(commands)}: {' '.join(command)}", flush=True)
        result = subprocess.run(command, cwd=ROOT)
        if result.returncode:
            return result.returncode
    print("All portable source checks passed. Native build/load, real world generation and client acceptance remain separate.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
