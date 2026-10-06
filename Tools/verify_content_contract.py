from __future__ import annotations

from pathlib import Path
from verify_boss_summons import verify as verify_boss_summons
from verify_cultivation_weapons import verify as verify_cultivation_weapons
from verify_pill_quality import main as verify_pill_quality
from verify_inscriptions import verify as verify_inscriptions


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
        "public partial class BossArrayFieldProjectile",
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
        "Localization/progression/zh-Hans.hjson",
        "ArtifactAwakeningReady",
        "ArtifactAwakeningLocked",
        "BossPrerequisiteRequired",
        "BossSummonSiteRequired",
        "BossSummonNightRequired",
        "TribulationComprehensionGained",
    )
    require_text(
        "Localization/progression/en-US.hjson",
        "ArtifactAwakeningReady",
        "ArtifactAwakeningLocked",
        "BossPrerequisiteRequired",
        "BossSummonSiteRequired",
        "BossSummonNightRequired",
        "TribulationComprehensionGained",
    )
    require_text(
        "Localization/guides/zh-Hans.hjson",
        "Comprehension",
    )
    require_text(
        "Localization/guides/en-US.hjson",
        "Comprehension",
    )
    require_text(
        "Localization/zh-Hans.hjson",
        "Commission",
        "AlreadyClaimed",
        "HerbSectApprentice",
    )
    require_text(
        "Localization/guides/zh-Hans.hjson",
        "CommissionHerbReady",
        "CommissionNoneReady",
        "TribulationCloud",
        "StarWomb",
        "MoonboneImmortal",
        "OldHeavenCore",
    )
    require_text(
        "Localization/guides/en-US.hjson",
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

    verify_boss_summons()
    verify_cultivation_weapons()
    verify_pill_quality()
    verify_inscriptions()
    require_text("Content/Items/HandGenerated/ArtifactQuenchingCrystal.cs", "RefinesArtifact => true", "AddIngredient<ArtifactBlankShard>(2)", "AddIngredient<FurnaceSlagIron>(4)")
    require_text("Content/Items/HandGenerated/ArtifactAwakeningSeal.cs", "AwakensArtifact => true", "AddIngredient<TribulationCloudDew>(6)", "TileType<StarPatternCauldronTile>()")
    for sample in ("CloudpiercerFlyingSword", "GreenwoodArrayPlate"):
        require_text(f"Content/Items/Weapons/{sample}.cs", "RefinedArtifact.IsAwakened(Item)")
    require_text("Common/Items/RefinedArtifact.cs", '"CloudpiercerFlyingSword" or "GreenwoodArrayPlate"')
    for language in ("zh-Hans", "en-US"):
        require_text(f"Localization/refinement/{language}.hjson", "ArtifactQuenchingCrystal", "Requirements", "ToolCost", "ChooseTarget", "Selection", "Row", "Success", "ArtifactAwakeningSeal", "AwakeningCost", "AwakeningRequirements", "AwakeningSelection", "AwakeningRow", "AwakeState", "DormantState", "AwakeningSuccess")
    require_text("Content/Items/HandGenerated/P3Equipment.cs", "DefaultToPlaceableTile(ModContent.TileType<global::XianXia.Content.Tiles.CultivatedSpiritHerbTile>()")
    require_text("Content/NPCs/Town/CultivationTownNPCs.cs", "shop.Add<SpiritHerbSeeds>();")
    require_text("Content/Tiles/CultivatedSpiritHerbTile.cs", "ItemType<GreenwoodRoot>()", "ItemType<SpiritHerbSeeds>()")
    require_file("Content/Tiles/CultivatedSpiritHerbTile.png")
    for language in ("zh-Hans", "en-US"):
        require_text(f"Localization/gardening/{language}.hjson", "CultivatedSpiritHerbTile", "MapEntry")
    require_text("Common/Systems/ArtifactSkillTransactions.cs", "IsItemSlotUnlockedAndUsable", "skillRequestCooldown", "activeSkillCooldown")
    require_text("Common/Players/CultivationSnapshot.cs", "SkillCooldown", "WardTimer")
    require_text("Common/Systems/ArtifactKeybindSystem.cs", '"ArtifactSkill"', '"WardSkill"')
    for language in ("zh-Hans", "en-US"):
        require_text(f"Localization/skills/{language}.hjson", "ArtifactSkill", "WardSkill", "Sword:", "Array:", "Ward:", "Ready:", "Cooldown:", "WardStatus:", "Unavailable:", "CoolingDown:", "NeedArray:", "NoHealing:", "NeedEnergy:", "Success:", "Capacity:", "WardTooltip:")
    for boss, material in (("TribulationCloudAvatar", "FoundationSeal"), ("GreenwoodMedicineKingEcho", "MedicineKingWoodHeart"), ("MoonboneImmortal", "StarCalamityCore"), ("BrokenHeavenInspector", "ImperialDecreeItem"), ("HeavenTabletGuardian", "HeavenTabletSeal"), ("OldHeavenDaoCore", "RouteMaterial")):
        require_text(f"Content/NPCs/Bosses/{boss}.cs", f"ItemDropRule.Common(ModContent.ItemType<global::XianXia.Content.Items.HandGenerated.{material}>(), 1)")
    require_text("Content/Items/HandGenerated/ArtifactAwakeningSeal.cs", "AddIngredient<MedicineKingWoodHeart>()")
    require_text("Content/Items/HandGenerated/ArtifactQuenchingCrystal.cs", "CreateRecipe(3).AddIngredient<FoundationSeal>()")
    require_text("Common/Systems/BossMaterialRecipeSystem.cs", "ItemType<MoonboneDharmaSword>()", "AddIngredient<StarCalamityCore>()", "AddIngredient<HeavenTabletSeal>()", "AddIngredient<ImperialDecreeItem>()", "ItemType<BrokenHeavenDecree>()", "TileType<DaoSeveringAltarTile>()", "TileType<HeavenFireFurnaceTile>()")
    require_text("Content/Items/HandGenerated/RouteMaterial.cs", "EndgameRouteUISystem", "consumable = false")
    require_text("Common/Systems/EndgameRouteTransactions.cs", "TryChooseRoute", "NearAltar", "old_heaven_dao_core", "NetworkInitialized")
    require_text("Common/UI/EndgameRouteUIState.cs", 'Text("Warning")', 'Text("Confirm"', "ValidSelection")
    require_text("Common/Items/EndgameRouteReward.cs", "AddIngredient<RouteMaterial>()", "ChosenRoute == Route", "AddCondition")
    for reward in ("RebuiltHeavenHeart", "SeveredHeavenEdge", "StarAbyssPact"):
        require_text(f"Content/Items/HandGenerated/{reward}.cs", f"class {reward} : EndgameRouteReward", "override string Texture", "if (!Active) return")
    for material in ("HeavenTabletSeal", "ImperialDecreeItem", "StarCalamityCore"):
        require_file(f"Content/Items/HandGenerated/{material}.png")
    for language in ("zh-Hans", "en-US"):
        require_text(f"Localization/routes/{language}.hjson", "RebuildHeaven:", "SeverHeaven:", "AcceptStarAbyss:", "Warning:", "Completed:", "AlreadyChosen:", "Confirm:", "ChooseFirst:", "Cancel:", "NeedAltar:", "Requirements:", "RebuildHeavenDescription:", "SeverHeavenDescription:", "AcceptStarAbyssDescription:", "RebuildHeavenCondition:", "SeverHeavenCondition:", "AcceptStarAbyssCondition:", "RebuiltHeavenHeart:", "SeveredHeavenEdge:", "StarAbyssPact:")
    require_text("Content/Items/HandGenerated/DaoTransformationSeal.cs", "TransformsArtifact => true", "AddIngredient<RouteMaterial>()", "NearAltar", "TileType<DaoSeveringAltarTile>()")
    require_text("Common/Items/RefinedArtifact.cs", "TryTransform", 'tag["daoRoute"]', "ActiveDaoRoute", "damage.Base")
    require_text("Common/Systems/DaoArtifactTransactions.cs", "CanTransform", "NearAltar", "TryTransform", "worldRoute", "NetworkInitialized")
    require_text("Common/Systems/InscriptionTransactions.cs", "!material.TransformsArtifact")
    require_text("Common/Systems/ArtifactSkillTransactions.cs", "DaoArtifactRules.SkillCost", "DaoArtifactRules.PulseHeal", "DaoArtifactRules.BurstMultiplier")
    for language in ("zh-Hans", "en-US"):
        require_text(f"Localization/dao-artifacts/{language}.hjson", "DaoTransformationSeal:", "Cost:", "Selection:", "Row:", "Tooltip:", "ChangedInventory:", "NeedAltar:", "Requirements:", "NeedStones:", "Success:")
    station_migrations = {
        "Materials/QiCondensingPill": "EarthClayFurnaceTile",
        "Weapons/CinnabarTalismanFlameItem": "SimpleTalismanTableTile",
        "Weapons/ThunderPatternSwordCase": "ThunderPatternForgeTile",
        "Weapons/ThunderTalismanArrayPlate": "ThunderPatternForgeTile",
        "Weapons/FormlessSwordWheel": "SectTrialAltarTile",
        "Weapons/MoonboneDharmaSword": "DaoSeveringAltarTile",
        "Weapons/BrokenHeavenDecree": "HeavenFireFurnaceTile",
        "Weapons/StarEclipseArbalest": "StarPatternCauldronTile",
        "Accessories/LightningWardJade": "ThunderPatternForgeTile",
        "Accessories/StarAbyssEye": "StarPatternCauldronTile",
        "Accessories/NascentSoulJadeBox": "SectTrialAltarTile",
        "Accessories/BrokenHeavenCrownSeal": "HeavenFireFurnaceTile",
        "Accessories/DaoSeveringRing": "DaoSeveringAltarTile",
    }
    for item, station in station_migrations.items():
        require_text(f"Content/Items/{item}.cs", f".AddTile(ModContent.TileType<global::XianXia.Content.Tiles.Stations.{station}>())")
    require_text("Content/Items/Weapons/OldHeavenDaoScroll.cs", "Item.consumable = true", "CultivationStage.NascentSoul", "CultivationItemTransactions.RequestIfMultiplayer")
    require_file("Wiki/Content/Items/Entries/Old_Heaven_Dao_Scroll.md")
    damage_pairs = {
        "WoodgrainFlyingSword": ("Melee", "WoodgrainSwordProjectile"),
        "CloudpiercerFlyingSword": ("Melee", "CloudpiercerSwordProjectile"),
        "ThunderPatternSwordCase": ("Melee", "ThunderSwordProjectile"),
        "FormlessSwordWheel": ("Melee", "FormlessSwordWheelProjectile"),
        "MoonboneDharmaSword": ("Melee", "MoonboneShardProjectile"),
        "SpiritwoodCrossbow": ("Ranged", "SpiritBoltProjectile"),
        "StarEclipseArbalest": ("Ranged", "StarEclipseSplitBolt"),
        "CinnabarTalismanFlameItem": ("Magic", "CinnabarTalismanFlame"),
        "GreenwoodArrayPlate": ("Magic", "GreenwoodArrayField"),
        "ThunderTalismanArrayPlate": ("Magic", "ThunderTalismanArray"),
        "BrokenHeavenDecree": ("Magic", "DecreeJudgementBeam"),
    }
    for weapon, (damage_class, projectile) in damage_pairs.items():
        require_text(f"Content/Items/Weapons/{weapon}.cs", f"Item.DamageType = DamageClass.{damage_class};")
        require_text(f"Content/Projectiles/{projectile}.cs", f"Projectile.DamageType = DamageClass.{damage_class};")
    require_text("Content/Projectiles/CloudWispProjectile.cs", "DamageClass.Melee")
    require_text("Content/Projectiles/SpiritBolt.cs", "DamageClass.Ranged")
    require_text("Content/Projectiles/MinorThunderboltProjectile.cs", "EntitySource_Parent", "origin.DamageType", "SendExtraAI", "ReceiveExtraAI")
    require_text("Content/Items/HandGenerated/SmallArtifactPendant.cs", "DamageClass.Summon", "slotsMinions+1", "ActiveProjectiles", "originalDamage", "AddBuff", "maxProjectiles")
    require_text("Content/Projectiles/SmallArtifactSpirit.cs", "minionSlots = 1", "DamageClass.Summon", "MinionContactDamage", "MinionSacrificable", "HasMinionAttackTargetNPC", "Collision.CanHitLine", "1600*1600", "netUpdate = true", "Projectile.friendly = target != null")
    require_text("Content/Buffs/SmallArtifactSpiritBuff.cs", "ownedProjectileCounts", "buffNoSave", "DelBuff")
    for language in ("zh-Hans", "en-US"):
        require_text(f"Localization/artifact-spirit/{language}.hjson", "SmallArtifactSpiritBuff", "DisplayName", "Description")
    for enemy in (ROOT / "Content/NPCs/Enemies").glob("*.cs"):
        name = enemy.stem.split(".")[0]
        primary = enemy.with_name(name + ".cs")
        if enemy != primary:
            require_text(str(enemy.relative_to(ROOT)), f"partial class {name}")
            require_text(str(primary.relative_to(ROOT)), f"partial class {name}")
        require_text(str(primary.relative_to(ROOT)), f"EnemySpawnRules.Allows(nameof({name}), Main.hardMode, NPC.downedPlantBoss, NPC.downedGolemBoss, NPC.downedMoonlord)")
    require_text("Common/Items/CultivationWeaponItem.cs", "SetStaticDefaults() => Item.ResearchUnlockCount = 1")
    for source in (ROOT / "Content/Items/Weapons").glob("*.cs"):
        text = source.read_text(encoding="utf-8")
        if "CultivationWeaponItem" in text and "SetStaticDefaults" in text:
            require_text(str(source.relative_to(ROOT)), "Item.ResearchUnlockCount = 1")
    for source in (ROOT / "Content/Items/Accessories").glob("*.cs"):
        require_text(str(source.relative_to(ROOT)), "Item.ResearchUnlockCount = 1")
    require_text("Content/Items/Weapons/OldHeavenDaoScroll.cs", "Item.ResearchUnlockCount = 25")
    print("Content contract verified.")


if __name__ == "__main__":
    main()
