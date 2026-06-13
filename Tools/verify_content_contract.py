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
        "Content/Projectiles/ThunderTalismanArray.png",
        "Content/NPCs/Bosses/AbyssalStarWomb.cs",
        "Content/Items/Weapons/ThunderTalismanArrayPlate.cs",
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
    require_text("Content/NPCs/Bosses/AbyssalStarWomb.cs", "BossArrayFieldProjectile", "patternInterval")
    require_text("Content/NPCs/Bosses/TribulationCloudAvatar.cs", "TribulationWarningLineProjectile", "patternInterval")
    require_text(
        "Tools/generate_tmod_content.py",
        "def boss_pattern_code",
        "BossArrayFieldProjectile",
        "global::XianXia.Content.Projectiles.BossArrayFieldProjectile",
        "BOSS_UNLOCK_REQUIREMENTS",
        "def projectile_behavior_code",
        "CloudpiercerSwordProjectile",
        "StarEclipseSplitBolt",
        "def enemy_behavior_code",
        "tribulation_cloudling",
        "archived_immortal_soul",
    )
    require_text("Content/NPCs/Enemies/MiasmaFlowerMoth.cs", "PostAI()", "BuffID.Poisoned", "Main.ActivePlayers")
    require_text("Content/NPCs/Enemies/FurnaceAshGolem.cs", "PostAI()", "BuffID.OnFire3")
    require_text("Content/NPCs/Enemies/TribulationCloudling.cs", "TribulationWarningLineProjectile")
    require_text("Content/NPCs/Enemies/ArchivedImmortalSoul.cs", "BossSpiritBoltProjectile", "SafeNormalize")
    require_text(
        "Content/Projectiles/CloudWispProjectile.cs",
        "CloudWispProjectile",
    )
    require_text(
        "Content/Projectiles/GreenwoodArrayField.cs",
        "RestoreSpiritualEnergy",
    )
    require_text(
        "Content/Projectiles/MinorThunderboltProjectile.cs",
        "MinorThunderboltProjectile",
    )
    require_text(
        "Content/Projectiles/DecreeJudgementBeam.cs",
        "localNPCHitCooldown",
    )
    require_text(
        "Content/Projectiles/CinnabarTalismanFlame.cs",
        "BuffID.OnFire3",
    )
    require_text(
        "Content/Projectiles/StarEclipseSplitBolt.cs",
        "StarEclipseSplitBolt",
    )
    require_text(
        "Common/Players/XianXiaPlayer.cs",
        "BossPrerequisiteRequired",
        "requiredDownedBoss",
        "DownedBossSystem.DownedBosses.Contains",
    )
    require_text("Content/Items/BossSummons/SummonMoonboneRitualTalisman.cs", "moonbone_immortal", "CanUseBossSummon", "CanUseGeneratedBossSummon")
    require_text("Content/Items/BossSummons/SummonGardenBrokenKey.cs", "spirit_vein_wyrm", "CanUseBossSummon", "CanUseGeneratedBossSummon")
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
        "TribulationCloud",
        "StarWomb",
        "MoonboneImmortal",
        "OldHeavenCore",
    )
    require_text(
        "Common/Systems/DownedBossSystem.cs",
        "ClaimedCommissions",
        "ReputationByCommission",
        "TryClaimCommission",
    )
    require_text("Content/Items/Materials/FoundationPill.cs", "AlchemyInsightBuff", "ReduceSpiritPressure")
    require_text("Content/Items/Accessories/StarAbyssEye.cs", "spiritualEnergyCostMultiplier *= 1.08f")
    require_text("Content/Items/Weapons/ThunderTalismanArrayPlate.cs", "HasArtifactAwakening", "ArtifactAwakeningReady", "DownedBossSystem.SectReputation")
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
        "TribulationCloud",
        "StarWomb",
        "MoonboneImmortal",
        "OldHeavenCore",
    )
    require_text(
        "Localization/guides.en-US.hjson",
        "CommissionHerbReady",
        "CommissionNoneReady",
        "TribulationCloud",
        "StarWomb",
        "MoonboneImmortal",
        "OldHeavenCore",
    )
    require_text(
        "Localization/en-US.hjson",
        "Commission",
        "AlreadyClaimed",
        "HerbSectApprentice",
    )

    boss_field_refs = sum(path.read_text(encoding="utf-8").count("BossArrayFieldProjectile") for path in (ROOT / "Content/NPCs/Bosses").glob("*.cs"))
    if boss_field_refs < 6:
        raise SystemExit(f"Expected multiple boss arena field references, found {boss_field_refs}.")

    print("Content contract verified.")


if __name__ == "__main__":
    main()
