from __future__ import annotations

from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]


def read(path: str) -> str:
    return (ROOT / path).read_text(encoding="utf-8")


def require_file(path: str) -> None:
    if not (ROOT / path).exists():
        raise SystemExit(f"Missing required file: {path}")


def require_text(path: str, *patterns: str) -> None:
    text = read(path)
    missing = [pattern for pattern in patterns if pattern not in text]
    if missing:
        joined = ", ".join(missing)
        raise SystemExit(f"{path} missing expected text: {joined}")


def main() -> None:
    required_files = [
        "Content/Projectiles/BossArrayFieldProjectile.cs",
        "Common/Systems/BossSummonRules.cs",
        "Content/Projectiles/Generated/ThunderTalismanArray.png",
        "Content/NPCs/Bosses/Generated/GeneratedBosses.cs",
        "Content/Items/Generated/GeneratedItems.cs",
        "Content/NPCs/Town/CultivationTownNPCs.cs",
        "Common/Systems/DownedBossSystem.cs",
        "Wiki/Design_Status.md",
    ]
    for path in required_files:
        require_file(path)

    require_text(
        "Content/Projectiles/BossArrayFieldProjectile.cs",
        "public class BossArrayFieldProjectile",
        "SpiritualPressureDisorderBuff",
        "CanDamage()",
    )
    require_text(
        "Common/Systems/BossSummonRules.cs",
        "CanUseGeneratedBossSummon",
        "BossSummonSiteRequired",
        "BossSummonNightRequired",
        "GreenwoodHerbGardenBiome",
        "MoonboneAbyssBiome",
    )
    require_text(
        "Content/NPCs/Bosses/Generated/GeneratedBosses.cs",
        "BossArrayFieldProjectile",
        "TribulationWarningLineProjectile",
        "patternInterval",
    )
    require_text(
        "Tools/generate_tmod_content.py",
        "def boss_pattern_code",
        "BossArrayFieldProjectile",
        "field_ids",
        "BOSS_UNLOCK_REQUIREMENTS",
        "def projectile_behavior_code",
        "CloudpiercerSwordProjectile",
        "StarEclipseSplitBolt",
        "def enemy_behavior_code",
        "tribulation_cloudling",
        "archived_immortal_soul",
    )
    require_text(
        "Content/NPCs/Enemies/Generated/GeneratedEnemies.cs",
        "PostAI()",
        "BuffID.Poisoned",
        "BuffID.OnFire3",
        "TribulationWarningLineProjectile",
        "BossSpiritBoltProjectile",
        "SafeNormalize",
        "Main.ActivePlayers",
    )
    require_text(
        "Content/Projectiles/Generated/GeneratedProjectiles.cs",
        "CloudWispProjectile",
        "MinorThunderboltProjectile",
        "localNPCHitCooldown",
        "RestoreSpiritualEnergy",
        "BuffID.OnFire3",
        "StarEclipseSplitBolt",
    )
    require_text(
        "Common/Players/XianXiaPlayer.cs",
        "BossPrerequisiteRequired",
        "requiredDownedBoss",
        "DownedBossSystem.DownedBosses.Contains",
    )
    require_text(
        "Content/Items/BossSummons/Generated/GeneratedBossSummons.cs",
        "spirit_vein_wyrm",
        "moonbone_immortal",
        "CanUseBossSummon",
        "CanUseGeneratedBossSummon",
    )
    require_text(
        "Content/NPCs/Town/CultivationTownNPCs.cs",
        "SetChatButtons",
        "TryClaimCommission",
        "ClaimCommission",
    )
    require_text(
        "Content/Items/Guides/SectLedger.cs",
        "GetCommissionGuidance",
        "CanClaimCommission",
        "ClaimedCommissions",
        "CommissionHerbReady",
    )
    require_text(
        "Common/Systems/DownedBossSystem.cs",
        "ClaimedCommissions",
        "ReputationByCommission",
        "TryClaimCommission",
    )
    require_text(
        "Content/Items/Generated/GeneratedItems.cs",
        "AlchemyInsightBuff",
        "ReduceSpiritPressure",
        "SpiritualPressureDisorderBuff",
        "spiritualEnergyCostMultiplier *= 1.08f",
        "HasArtifactAwakening",
        "ArtifactAwakeningReady",
        "DownedBossSystem.SectReputation",
    )
    require_text(
        "Common/Players/XianXiaPlayer.cs",
        "tribulationComprehension",
        "clearedTribulationStages",
        "TribulationComprehensionGained",
    )
    require_text(
        "Content/Items/Guides/TribulationGauge.cs",
        "Comprehension",
        "tribulationComprehension",
    )
    require_text(
        "Tools/generate_tmod_content.py",
        "awakening_thresholds",
        "HasArtifactAwakening",
        "ArtifactAwakeningLocked",
    )
    require_text(
        "Localization/progression.zh-Hans.hjson",
        "ArtifactAwakeningReady",
        "ArtifactAwakeningLocked",
        "BossPrerequisiteRequired",
        "BossSummonSiteRequired",
        "BossSummonNightRequired",
        "TribulationComprehensionGained",
    )
    require_text(
        "Localization/progression.en-US.hjson",
        "ArtifactAwakeningReady",
        "ArtifactAwakeningLocked",
        "BossPrerequisiteRequired",
        "BossSummonSiteRequired",
        "BossSummonNightRequired",
        "TribulationComprehensionGained",
    )
    require_text(
        "Localization/guides.zh-Hans.hjson",
        "Comprehension",
    )
    require_text(
        "Localization/guides.en-US.hjson",
        "Comprehension",
    )
    require_text(
        "Localization/zh-Hans.hjson",
        "Commission",
        "AlreadyClaimed",
        "HerbSectApprentice",
    )
    require_text(
        "Localization/guides.zh-Hans.hjson",
        "CommissionHerbReady",
        "CommissionNoneReady",
    )
    require_text(
        "Localization/guides.en-US.hjson",
        "CommissionHerbReady",
        "CommissionNoneReady",
    )
    require_text(
        "Localization/en-US.hjson",
        "Commission",
        "AlreadyClaimed",
        "HerbSectApprentice",
    )

    boss_field_refs = read("Content/NPCs/Bosses/Generated/GeneratedBosses.cs").count("BossArrayFieldProjectile")
    if boss_field_refs < 6:
        raise SystemExit(f"Expected multiple boss arena field references, found {boss_field_refs}.")

    print("Content contract verified.")


if __name__ == "__main__":
    main()
