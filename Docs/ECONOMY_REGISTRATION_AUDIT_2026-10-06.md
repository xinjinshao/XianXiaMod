# 制作与金币经济注册审计

引擎：2026.8.3.0；模组：0.1.0。

原生注册：196件本模组物品、91条产物配方、5个商店。

数据来自隔离专服实际 PostSetupRecipes 和 GetItemExpectedPrice；没有打开商店或改动玩家库存。
所有条件视为已解锁，制作站只作前置记录；价格为无前缀物品。成本由可购材料及多步配方传播，包含批量产出和配方组最低成本选择。
不模拟 ModifyActiveShop/开店回调、卖回原购品的退款、随机制作奖励/前缀、掉落/种植/钓鱼/微光、旅途复制和其它模组价格钩子。特殊货币及自定义价格条目排除。没有可购路径不等于无法获得，零候选不等于完整经济验收。

## 金币购买—制作—卖回候选

| 原生价格场景 | 可购或可由购料制作的本模组物品 | 正收益候选 | 排除商店条目 |
| --- | --- | --- | --- |
| neutral | 48 | 0 | 37 |
| happy | 48 | 0 | 37 |
| happy_discount | 48 | 0 | 37 |

## 指南物品价格复核

| 物品 | 场景 | 配方材料成本（铜） | 卖价（铜） |
| --- | --- | --- | --- |
| XianXia/SectLedger | neutral | 1500.00 | 400 |
| XianXia/SectLedger | happy | 1125.00 | 533 |
| XianXia/SectLedger | happy_discount | 900.00 | 533 |
| XianXia/TribulationGauge | neutral | 3800.00 | 1000 |
| XianXia/TribulationGauge | happy | 2850.00 | 1333 |
| XianXia/TribulationGauge | happy_discount | 2280.00 | 1333 |

## 原生标为材料的用途

下表仅记录配方用途；突破/事务消耗另有运行逻辑。

| 材料 | 产物用途 | 注册商店 |
| --- | --- | --- |
| XianXia/ArchiveRemnantLight | XianXia/ArchiveStarCodex, XianXia/ArchivedImmortalSoulContract | 无；另查掉落/制作来源 |
| XianXia/ArtifactBlankShard | XianXia/ArtifactAwakeningSeal, XianXia/ArtifactQuenchingCrystal, XianXia/BrokenHeavenDecree, XianXia/CinnabarTalismanFlameItem, XianXia/CloudpiercerFlyingSword, XianXia/FormlessSwordWheel, XianXia/GreenwoodArrayPlate, XianXia/MoonboneDharmaSword, XianXia/SectMechanismCrossbow, XianXia/SectTrialAltar, XianXia/StarEclipseArbalest, XianXia/TalismanCrossbow, XianXia/ThunderPatternSwordCase, XianXia/ThunderTalismanArrayPlate | XianXia/ArchiveScrollSpirit/Shop, XianXia/WanderingArtificer/Shop |
| XianXia/BrokenSwordIntent | XianXia/FormlessSwordWheel, XianXia/NascentSoulCloneTalisman | 无；另查掉落/制作来源 |
| XianXia/CinnabarPowder | XianXia/CinnabarTalismanArrow | 无；另查掉落/制作来源 |
| XianXia/DaoSeveringDust | XianXia/DaoSeveringAltar, XianXia/DaoSeveringRing | 无；另查掉落/制作来源 |
| XianXia/FoundationSeal | XianXia/ArtifactQuenchingCrystal | 无；另查掉落/制作来源 |
| XianXia/FurnaceSlagIron | XianXia/ArtifactForge, XianXia/ArtifactQuenchingCrystal, XianXia/BlackFurnaceWarhammer, XianXia/CinnabarTalismanFlameItem, XianXia/FoundationPill, XianXia/FurnaceAshSpiritContract, XianXia/FurnaceGuardPill, XianXia/FurnaceHeartRing, XianXia/TalismanCrossbow, XianXia/ThunderBurstPill | XianXia/WanderingArtificer/Shop |
| XianXia/GardenBrokenKey | XianXia/SummonGardenBrokenKey | 无；另查掉落/制作来源 |
| XianXia/GreenwoodRoot | XianXia/AlchemyCauldron, XianXia/ArtifactAwakeningSeal, XianXia/CloudpiercerFlyingSword, XianXia/FoundationPill, XianXia/FurnaceGuardPill, XianXia/GreenwoodArrayPlate, XianXia/GreenwoodMedicineCauldron, XianXia/QiCondensingPill, XianXia/QiGatheringPendant, XianXia/QiRecoveryPill, XianXia/SpiritwoodCharm, XianXia/SpringReturnPill, XianXia/TribulationResistingPill, XianXia/WindStepPill | XianXia/HerbSectApprentice/Shop |
| XianXia/HeavenDaoFragment | XianXia/BrokenHeavenCrownSeal, XianXia/BrokenHeavenDecree, XianXia/CelestialPuppetToken, XianXia/DaoTransformationSeal, XianXia/HeavenFireFurnace, XianXia/HeavenTabletWardSeal, XianXia/RebuiltHeavenHeart | XianXia/FallenHeavenMessenger/Shop |
| XianXia/HeavenTabletRubbing | XianXia/HeavenLawArbalest, XianXia/HeavenTabletWardSeal, XianXia/SummonHeavenTabletRubbing, XianXia/SummonHeavenTabletRubbingBrokenHeavenInspector | 无；另查掉落/制作来源 |
| XianXia/HeavenTabletSeal | XianXia/BrokenHeavenDecree, XianXia/CelestialPuppetToken, XianXia/HeavenLawArbalest, XianXia/HeavenTabletWardSeal, XianXia/RebuiltHeavenHeart | 无；另查掉落/制作来源 |
| XianXia/ImperialDecreeItem | XianXia/ArchiveStarCodex, XianXia/BrokenHeavenDecree, XianXia/SeveredHeavenEdge | 无；另查掉落/制作来源 |
| XianXia/LowGradeSpiritStone | XianXia/AlchemyCauldron, XianXia/ArchiveLightPillarPlaceable, XianXia/ArchiveStarCodex, XianXia/ArchivedImmortalSoulContract, XianXia/ArtifactForge, XianXia/BlackFurnaceWarhammer, XianXia/BrokenHeavenCrownSeal, XianXia/BrokenHeavenDecree, XianXia/BrokenHeavenTabletPlaceable, XianXia/CelestialPuppetToken, XianXia/CinnabarTalismanFlameItem, XianXia/CloudpiercerFlyingSword, XianXia/DaoSeveringRing, XianXia/FallenHeavenBiomeBlock, XianXia/FormlessSwordWheel, XianXia/FoundationPill, XianXia/FurnaceAshSpiritContract, XianXia/FurnaceBiomeBlock, XianXia/FurnaceGuardPill, XianXia/FurnaceHeartRing, XianXia/GreenwoodArrayPlate, XianXia/GreenwoodBiomeBlock, XianXia/GreenwoodMedicineCauldron, XianXia/HeavenLawArbalest, XianXia/HeavenTabletWardSeal, XianXia/LightningWardJade, XianXia/MoonboneBiomeBlock, XianXia/MoonboneDharmaSword, XianXia/NascentSoulCloneTalisman, XianXia/NascentSoulJadeBox, XianXia/QiCondensingPill, XianXia/QiDrawingTalisman, XianXia/QiGatheringPendant, XianXia/QiRecoveryPill, XianXia/RiftMembranePlaceable, XianXia/SectLedger, XianXia/SectMechanismCrossbow, XianXia/SectRuinBiomeBlock, XianXia/SingingThunderStonePlaceable, XianXia/SmallArtifactPendant, XianXia/SpiritVeinIncense, XianXia/SpiritwoodCharm, XianXia/SpiritwoodCrossbow, XianXia/StarAbyssBiomeBlock, XianXia/StarAbyssEye, XianXia/StarAbyssForbiddenTalisman, XianXia/StarAbyssLarvaContract, XianXia/StarCalamityMechanismCase, XianXia/StarEclipseArbalest, XianXia/SummonGardenBrokenKey, XianXia/SummonHeavenTabletRubbing, XianXia/SummonHeavenTabletRubbingBrokenHeavenInspector, XianXia/SummonMoonboneRitualTalisman, XianXia/SummonMoonboneRitualTalismanOldHeavenDaoCore, XianXia/SummonOldFurnaceEmber, XianXia/SummonSectTrialToken, XianXia/SummonSectTrialTokenGreenwoodMedicineKingEcho, XianXia/SummonStarAbyssMembrane, XianXia/SummonThunderCallingJade, XianXia/SummonThunderCallingJadeThunderMarshJiao, XianXia/SwordTabletPlaceable, XianXia/TalismanCrossbow, XianXia/ThunderBiomeBlock, XianXia/ThunderBurstPill, XianXia/ThunderPatternSwordCase, XianXia/ThunderTalismanArrayPlate, XianXia/TribulationGauge, XianXia/TribulationTrainingToken, XianXia/WindStepPill, XianXia/WoodgrainFlyingSword | XianXia/ArchiveScrollSpirit/Shop, XianXia/FallenHeavenMessenger/Shop, XianXia/HerbSectApprentice/Shop, XianXia/TribulationObserver/Shop, XianXia/WanderingArtificer/Shop |
| XianXia/MedicineKingWoodHeart | XianXia/ArtifactAwakeningSeal, XianXia/GreenwoodMedicineCauldron | 无；另查掉落/制作来源 |
| XianXia/Moonbone | XianXia/ArchiveStarCodex, XianXia/ArchivedImmortalSoulContract, XianXia/DaoSeveringAltar, XianXia/DaoTransformationSeal, XianXia/MoonboneDharmaSword, XianXia/SeveredHeavenEdge, XianXia/StarCalamityMechanismCase | 无；另查掉落/制作来源 |
| XianXia/MoonboneRitualTalisman | XianXia/SummonMoonboneRitualTalisman, XianXia/SummonMoonboneRitualTalismanOldHeavenDaoCore | 无；另查掉落/制作来源 |
| XianXia/OldFurnaceEmber | XianXia/BlackFurnaceWarhammer, XianXia/FurnaceAshSpiritContract, XianXia/SummonOldFurnaceEmber | 无；另查掉落/制作来源 |
| XianXia/RouteMaterial | XianXia/DaoTransformationSeal, XianXia/RebuiltHeavenHeart, XianXia/SeveredHeavenEdge, XianXia/StarAbyssPact | 无；另查掉落/制作来源 |
| XianXia/SectTrialToken | XianXia/NascentSoulJadeBox, XianXia/SectLedger, XianXia/SectTrialAltar, XianXia/SummonSectTrialToken, XianXia/SummonSectTrialTokenGreenwoodMedicineKingEcho | XianXia/ArchiveScrollSpirit/Shop |
| XianXia/SpiritGel | XianXia/QiDrawingTalisman, XianXia/SpiritVeinIncense, XianXia/TribulationTrainingToken, XianXia/WoodgrainFlyingSword | 无；另查掉落/制作来源 |
| XianXia/StarAbyssMembrane | XianXia/StarAbyssForbiddenTalisman, XianXia/StarAbyssLarvaContract, XianXia/SummonStarAbyssMembrane | 无；另查掉落/制作来源 |
| XianXia/StarCalamityCore | XianXia/MoonboneDharmaSword, XianXia/StarAbyssLarvaContract, XianXia/StarAbyssPact, XianXia/StarCalamityMechanismCase | 无；另查掉落/制作来源 |
| XianXia/StarEclipseCrystal | XianXia/StarAbyssEye, XianXia/StarAbyssForbiddenTalisman, XianXia/StarAbyssPact, XianXia/StarEclipseArbalest, XianXia/StarPatternCauldron | 无；另查掉落/制作来源 |
| XianXia/ThunderCallingJade | XianXia/SummonThunderCallingJade, XianXia/SummonThunderCallingJadeThunderMarshJiao | 无；另查掉落/制作来源 |
| XianXia/ThunderPatternFeather | XianXia/ThunderPatternSwordCase | 无；另查掉落/制作来源 |
| XianXia/TornScrollPage | XianXia/NascentSoulCloneTalisman, XianXia/SectMechanismCrossbow | 无；另查掉落/制作来源 |
| XianXia/TornTalismanPaper | XianXia/CinnabarTalismanArrow, XianXia/TalismanCrossbow | 无；另查掉落/制作来源 |
| XianXia/TribulationCloudDew | XianXia/ArtifactAwakeningSeal, XianXia/LightningWardJade, XianXia/ThunderBurstPill, XianXia/ThunderPatternForge, XianXia/ThunderTalismanArrayPlate, XianXia/TribulationGauge, XianXia/TribulationResistingPill | XianXia/TribulationObserver/Shop |

## 实际配方及制作站

| 产物 | 数量 | 材料 | 制作站ID | 条件键 |
| --- | --- | --- | --- | --- |
| XianXia/AlchemyCauldron | 1 | XianXia/LowGradeSpiritStone ×8, XianXia/GreenwoodRoot ×6, Terraria/Bottle ×1 | 18 | 无 |
| XianXia/ArchiveLightPillarPlaceable | 1 | Terraria/StoneBlock ×60, Terraria/FallenStar ×10, XianXia/LowGradeSpiritStone ×5 | 18 | Conditions.DownedMoonLord |
| XianXia/ArchiveStarCodex | 1 | XianXia/Moonbone ×12, XianXia/ArchiveRemnantLight ×6, XianXia/ImperialDecreeItem ×1, XianXia/LowGradeSpiritStone ×24 | 740 | Conditions.DownedMoonLord |
| XianXia/ArchivedImmortalSoulContract | 1 | XianXia/Moonbone ×20, XianXia/ArchiveRemnantLight ×8, XianXia/LowGradeSpiritStone ×36 | 740 | Conditions.DownedMoonLord |
| XianXia/ArtifactAwakeningSeal | 1 | XianXia/ArtifactBlankShard ×2, XianXia/TribulationCloudDew ×6, XianXia/GreenwoodRoot ×4, XianXia/MedicineKingWoodHeart ×1 | 745 | 无 |
| XianXia/ArtifactForge | 1 | XianXia/LowGradeSpiritStone ×10, XianXia/FurnaceSlagIron ×6, Terraria/IronAnvil ×1 | 18 | 无 |
| XianXia/ArtifactQuenchingCrystal | 1 | XianXia/ArtifactBlankShard ×2, XianXia/FurnaceSlagIron ×4 | 739 | 无 |
| XianXia/ArtifactQuenchingCrystal | 3 | XianXia/FoundationSeal ×1, XianXia/FurnaceSlagIron ×4 | 739 | 无 |
| XianXia/BlackFurnaceWarhammer | 1 | XianXia/OldFurnaceEmber ×4, XianXia/FurnaceSlagIron ×12, XianXia/LowGradeSpiritStone ×12 | 739 | 无 |
| XianXia/BrokenHeavenCrownSeal | 1 | XianXia/HeavenDaoFragment ×5, XianXia/LowGradeSpiritStone ×8 | 742 | 无 |
| XianXia/BrokenHeavenDecree | 1 | XianXia/HeavenTabletSeal ×1, XianXia/HeavenDaoFragment ×4, XianXia/LowGradeSpiritStone ×8 | 742 | 无 |
| XianXia/BrokenHeavenDecree | 1 | XianXia/ImperialDecreeItem ×1, XianXia/HeavenDaoFragment ×2, XianXia/LowGradeSpiritStone ×6 | 742 | 无 |
| XianXia/BrokenHeavenDecree | 1 | XianXia/ArtifactBlankShard ×2, XianXia/HeavenDaoFragment ×6, XianXia/LowGradeSpiritStone ×12 | 742 | 无 |
| XianXia/BrokenHeavenTabletPlaceable | 1 | Terraria/StoneBlock ×40, Terraria/GoldBar ×10, XianXia/LowGradeSpiritStone ×5 | 18 | Conditions.DownedGolem |
| XianXia/CelestialPuppetToken | 1 | XianXia/HeavenTabletSeal ×1, XianXia/HeavenDaoFragment ×12, XianXia/LowGradeSpiritStone ×24 | 742 | Conditions.DownedGolem |
| XianXia/CinnabarTalismanArrow | 50 | Terraria/WoodenArrow ×50, XianXia/TornTalismanPaper ×2, XianXia/CinnabarPowder ×1 | 744 | 无 |
| XianXia/CinnabarTalismanFlameItem | 1 | XianXia/ArtifactBlankShard ×2, XianXia/FurnaceSlagIron ×6, XianXia/LowGradeSpiritStone ×12 | 744 | 无 |
| XianXia/CloudpiercerFlyingSword | 1 | XianXia/ArtifactBlankShard ×2, XianXia/GreenwoodRoot ×6, XianXia/LowGradeSpiritStone ×12 | 739 | 无 |
| XianXia/DaoSeveringAltar | 1 | XianXia/DaoSeveringDust ×12, XianXia/Moonbone ×8 | 742 | 无 |
| XianXia/DaoSeveringRing | 1 | XianXia/DaoSeveringDust ×5, XianXia/LowGradeSpiritStone ×8 | 740 | 无 |
| XianXia/DaoTransformationSeal | 1 | XianXia/RouteMaterial ×1, XianXia/HeavenDaoFragment ×8, XianXia/Moonbone ×8 | 740 | 无 |
| XianXia/EarthClayFurnace | 1 | Terraria/ClayBlock ×20, Terraria/StoneBlock ×15, Terraria/Wood ×10 | 18 | 无 |
| XianXia/FallenHeavenBiomeBlock | 100 | Terraria/StoneBlock ×100, XianXia/LowGradeSpiritStone ×1 | 18 | Conditions.DownedGolem |
| XianXia/FormlessSwordWheel | 1 | XianXia/ArtifactBlankShard ×2, XianXia/BrokenSwordIntent ×12, XianXia/LowGradeSpiritStone ×12 | 743 | 无 |
| XianXia/FoundationPill | 1 | XianXia/GreenwoodRoot ×4, XianXia/FurnaceSlagIron ×4, XianXia/LowGradeSpiritStone ×10 | 738 | Mods.XianXia.PillQuality.EmptyCursor |
| XianXia/FurnaceAshSpiritContract | 1 | XianXia/OldFurnaceEmber ×2, XianXia/FurnaceSlagIron ×8, XianXia/LowGradeSpiritStone ×12 | 739 | 无 |
| XianXia/FurnaceBiomeBlock | 100 | Terraria/AshBlock ×100, XianXia/LowGradeSpiritStone ×1 | 18 | 无 |
| XianXia/FurnaceGuardPill | 2 | XianXia/FurnaceSlagIron ×4, XianXia/GreenwoodRoot ×2, XianXia/LowGradeSpiritStone ×6, Terraria/BottledWater ×1 | 738 | Mods.XianXia.PillQuality.EmptyCursor |
| XianXia/FurnaceHeartRing | 1 | XianXia/FurnaceSlagIron ×5, XianXia/LowGradeSpiritStone ×8 | 739 | 无 |
| XianXia/GreenwoodArrayPlate | 1 | XianXia/ArtifactBlankShard ×2, XianXia/GreenwoodRoot ×6, XianXia/LowGradeSpiritStone ×12 | 739 | 无 |
| XianXia/GreenwoodBiomeBlock | 100 | Terraria/DirtBlock ×100, XianXia/LowGradeSpiritStone ×1 | 18 | 无 |
| XianXia/GreenwoodMedicineCauldron | 1 | XianXia/MedicineKingWoodHeart ×1, XianXia/GreenwoodRoot ×16, XianXia/LowGradeSpiritStone ×24 | 743 | Conditions.DownedPlantera |
| XianXia/HeavenFireFurnace | 1 | XianXia/HeavenDaoFragment ×12, Terraria/AdamantiteForge ×1 | 134 | 无 |
| XianXia/HeavenLawArbalest | 1 | XianXia/HeavenTabletSeal ×1, XianXia/HeavenTabletRubbing ×8, XianXia/LowGradeSpiritStone ×24 | 742 | Conditions.DownedGolem |
| XianXia/HeavenTabletWardSeal | 1 | XianXia/HeavenTabletSeal ×1, XianXia/HeavenDaoFragment ×12, XianXia/HeavenTabletRubbing ×8, XianXia/LowGradeSpiritStone ×24 | 742 | Conditions.DownedGolem |
| XianXia/LightningWardJade | 1 | XianXia/TribulationCloudDew ×5, XianXia/LowGradeSpiritStone ×8 | 746 | 无 |
| XianXia/MoonboneBiomeBlock | 100 | Terraria/StoneBlock ×100, XianXia/LowGradeSpiritStone ×1 | 18 | Conditions.DownedMoonLord |
| XianXia/MoonboneDharmaSword | 1 | XianXia/StarCalamityCore ×1, XianXia/Moonbone ×6, XianXia/LowGradeSpiritStone ×6 | 740 | 无 |
| XianXia/MoonboneDharmaSword | 1 | XianXia/ArtifactBlankShard ×2, XianXia/Moonbone ×20, XianXia/LowGradeSpiritStone ×12 | 740 | 无 |
| XianXia/NascentSoulCloneTalisman | 1 | XianXia/BrokenSwordIntent ×8, XianXia/TornScrollPage ×12, XianXia/LowGradeSpiritStone ×20 | 743 | Conditions.DownedPlantera |
| XianXia/NascentSoulJadeBox | 1 | XianXia/SectTrialToken ×5, XianXia/LowGradeSpiritStone ×8 | 743 | 无 |
| XianXia/QiCondensingPill | 1 | XianXia/GreenwoodRoot ×3, XianXia/LowGradeSpiritStone ×5 | 741 | Mods.XianXia.PillQuality.EmptyCursor |
| XianXia/QiDrawingTalisman | 1 | XianXia/LowGradeSpiritStone ×3, XianXia/SpiritGel ×2 | 18 | 无 |
| XianXia/QiGatheringPendant | 1 | XianXia/GreenwoodRoot ×5, XianXia/LowGradeSpiritStone ×8 | 739 | 无 |
| XianXia/QiRecoveryPill | 2 | XianXia/GreenwoodRoot ×2, XianXia/LowGradeSpiritStone ×4, Terraria/BottledWater ×1 | 738 | Mods.XianXia.PillQuality.EmptyCursor |
| XianXia/RebuiltHeavenHeart | 1 | XianXia/RouteMaterial ×1, XianXia/HeavenTabletSeal ×1, XianXia/HeavenDaoFragment ×8 | 740 | Mods.XianXia.Routes.RebuildHeavenCondition |
| XianXia/RiftMembranePlaceable | 1 | Terraria/StoneBlock ×20, Terraria/FallenStar ×5, XianXia/LowGradeSpiritStone ×5 | 18 | Conditions.InHardmode |
| XianXia/SectLedger | 1 | XianXia/SectTrialToken ×1, XianXia/LowGradeSpiritStone ×5 | 739 | 无 |
| XianXia/SectMechanismCrossbow | 1 | XianXia/ArtifactBlankShard ×12, XianXia/TornScrollPage ×8, XianXia/LowGradeSpiritStone ×16 | 743 | Conditions.DownedPlantera |
| XianXia/SectRuinBiomeBlock | 100 | Terraria/StoneBlock ×100, XianXia/LowGradeSpiritStone ×1 | 18 | Conditions.DownedPlantera |
| XianXia/SectTrialAltar | 1 | XianXia/SectTrialToken ×4, XianXia/ArtifactBlankShard ×8 | 739 | 无 |
| XianXia/SeveredHeavenEdge | 1 | XianXia/RouteMaterial ×1, XianXia/ImperialDecreeItem ×1, XianXia/Moonbone ×8 | 740 | Mods.XianXia.Routes.SeverHeavenCondition |
| XianXia/SimpleTalismanTable | 1 | Terraria/Wood ×15, Terraria/Book ×1 | 18 | 无 |
| XianXia/SingingThunderStonePlaceable | 1 | Terraria/StoneBlock ×20, Terraria/FallenStar ×5, XianXia/LowGradeSpiritStone ×5 | 18 | Conditions.InHardmode |
| XianXia/SmallArtifactPendant | 1 | XianXia/LowGradeSpiritStone ×8, Terraria/FallenStar ×3 | 739 | 无 |
| XianXia/SpiritVeinBiomeBlock | 40 | Terraria/StoneBlock ×40, Terraria/FallenStar ×1 | 18 | 无 |
| XianXia/SpiritVeinIncense | 1 | XianXia/LowGradeSpiritStone ×8, XianXia/SpiritGel ×6 | 18 | 无 |
| XianXia/SpiritwoodCharm | 1 | XianXia/GreenwoodRoot ×5, XianXia/LowGradeSpiritStone ×8 | 739 | 无 |
| XianXia/SpiritwoodCrossbow | 1 | Terraria/Wood ×16, XianXia/LowGradeSpiritStone ×4 | 739 | 无 |
| XianXia/SpringReturnPill | 3 | XianXia/GreenwoodRoot ×2, Terraria/BottledWater ×1 | 738 | Mods.XianXia.PillQuality.EmptyCursor |
| XianXia/StarAbyssBiomeBlock | 100 | Terraria/StoneBlock ×100, XianXia/LowGradeSpiritStone ×1 | 18 | Conditions.InHardmode |
| XianXia/StarAbyssEye | 1 | XianXia/StarEclipseCrystal ×5, XianXia/LowGradeSpiritStone ×8 | 745 | 无 |
| XianXia/StarAbyssForbiddenTalisman | 1 | XianXia/StarEclipseCrystal ×6, XianXia/StarAbyssMembrane ×2, XianXia/LowGradeSpiritStone ×12 | 26 | 无 |
| XianXia/StarAbyssLarvaContract | 1 | XianXia/StarCalamityCore ×1, XianXia/StarAbyssMembrane ×8, XianXia/LowGradeSpiritStone ×16 | 745 | Conditions.InHardmode |
| XianXia/StarAbyssPact | 1 | XianXia/RouteMaterial ×1, XianXia/StarCalamityCore ×1, XianXia/StarEclipseCrystal ×12 | 740 | Mods.XianXia.Routes.AcceptStarAbyssCondition |
| XianXia/StarCalamityMechanismCase | 1 | XianXia/StarCalamityCore ×1, XianXia/Moonbone ×16, XianXia/LowGradeSpiritStone ×32 | 740 | Conditions.DownedMoonLord |
| XianXia/StarEclipseArbalest | 1 | XianXia/ArtifactBlankShard ×2, XianXia/StarEclipseCrystal ×15, XianXia/LowGradeSpiritStone ×12 | 745 | 无 |
| XianXia/StarPatternCauldron | 1 | XianXia/StarEclipseCrystal ×8, Terraria/HellstoneBar ×10 | 738 | 无 |
| XianXia/SummonGardenBrokenKey | 1 | XianXia/GardenBrokenKey ×1, XianXia/LowGradeSpiritStone ×10 | 18 | 无 |
| XianXia/SummonHeavenTabletRubbing | 1 | XianXia/HeavenTabletRubbing ×1, XianXia/LowGradeSpiritStone ×31 | 26 | 无 |
| XianXia/SummonHeavenTabletRubbingBrokenHeavenInspector | 1 | XianXia/HeavenTabletRubbing ×1, XianXia/LowGradeSpiritStone ×34 | 26 | 无 |
| XianXia/SummonMoonboneRitualTalisman | 1 | XianXia/MoonboneRitualTalisman ×1, XianXia/LowGradeSpiritStone ×37 | 26 | 无 |
| XianXia/SummonMoonboneRitualTalismanOldHeavenDaoCore | 1 | XianXia/MoonboneRitualTalisman ×1, XianXia/LowGradeSpiritStone ×40 | 26 | 无 |
| XianXia/SummonOldFurnaceEmber | 1 | XianXia/OldFurnaceEmber ×1, XianXia/LowGradeSpiritStone ×13 | 18 | 无 |
| XianXia/SummonSectTrialToken | 1 | XianXia/SectTrialToken ×1, XianXia/LowGradeSpiritStone ×25 | 26 | 无 |
| XianXia/SummonSectTrialTokenGreenwoodMedicineKingEcho | 1 | XianXia/SectTrialToken ×1, XianXia/LowGradeSpiritStone ×28 | 26 | 无 |
| XianXia/SummonStarAbyssMembrane | 1 | XianXia/StarAbyssMembrane ×1, XianXia/LowGradeSpiritStone ×22 | 26 | 无 |
| XianXia/SummonThunderCallingJade | 1 | XianXia/ThunderCallingJade ×1, XianXia/LowGradeSpiritStone ×16 | 18 | 无 |
| XianXia/SummonThunderCallingJadeThunderMarshJiao | 1 | XianXia/ThunderCallingJade ×1, XianXia/LowGradeSpiritStone ×19 | 26 | 无 |
| XianXia/SwordTabletPlaceable | 1 | Terraria/StoneBlock ×20, Terraria/Bone ×5, XianXia/LowGradeSpiritStone ×5 | 18 | Conditions.DownedPlantera |
| XianXia/TalismanCrossbow | 1 | XianXia/ArtifactBlankShard ×4, XianXia/FurnaceSlagIron ×8, XianXia/TornTalismanPaper ×6, XianXia/LowGradeSpiritStone ×8 | 739 | 无 |
| XianXia/ThunderBiomeBlock | 100 | Terraria/Cloud ×100, XianXia/LowGradeSpiritStone ×1 | 18 | Conditions.InHardmode |
| XianXia/ThunderBurstPill | 2 | XianXia/TribulationCloudDew ×2, XianXia/FurnaceSlagIron ×2, XianXia/LowGradeSpiritStone ×8, Terraria/BottledWater ×1 | 738 | Mods.XianXia.PillQuality.EmptyCursor |
| XianXia/ThunderPatternForge | 1 | XianXia/TribulationCloudDew ×8, Terraria/MythrilAnvil ×1 | 134 | 无 |
| XianXia/ThunderPatternSwordCase | 1 | XianXia/ArtifactBlankShard ×2, XianXia/ThunderPatternFeather ×12, XianXia/LowGradeSpiritStone ×12 | 746 | 无 |
| XianXia/ThunderTalismanArrayPlate | 1 | XianXia/ArtifactBlankShard ×2, XianXia/TribulationCloudDew ×6, XianXia/LowGradeSpiritStone ×12 | 746 | 无 |
| XianXia/TribulationGauge | 1 | XianXia/TribulationCloudDew ×3, XianXia/LowGradeSpiritStone ×8 | 18 | 无 |
| XianXia/TribulationResistingPill | 2 | XianXia/TribulationCloudDew ×3, XianXia/GreenwoodRoot ×2, Terraria/BottledWater ×1 | 738 | Mods.XianXia.PillQuality.EmptyCursor |
| XianXia/TribulationTrainingToken | 1 | XianXia/LowGradeSpiritStone ×6, XianXia/SpiritGel ×4 | 18 | 无 |
| XianXia/WindStepPill | 2 | Terraria/Feather ×2, XianXia/GreenwoodRoot ×2, XianXia/LowGradeSpiritStone ×4, Terraria/BottledWater ×1 | 738 | Mods.XianXia.PillQuality.EmptyCursor |
| XianXia/WoodgrainFlyingSword | 1 | Terraria/Wood ×12, XianXia/LowGradeSpiritStone ×6, XianXia/SpiritGel ×10 | 18 | 无 |
