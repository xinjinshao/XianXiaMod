# 美术素材图库

[返回首页](Home.md)

本页由 `Tools/refresh_art_gallery.py` 从 `Content/**/*.png` 自动生成，覆盖当前 mod 实际加载的美术资源。描述优先引用 Wiki 中的美术/Prompt 文本；缺少专门条目的资源使用项目美术约束生成说明。

## 总览图

<img src="../Assets/Final/contact_sheet_v01.png" alt="all assets contact sheet" width="760">

## 多帧规则

- 敌怪与 Boss 主体贴图使用竖向 spritesheet；代码通过 `Main.npcFrameCount[Type]` 与 `FindFrame(int frameHeight)` 切帧。
- 敌怪/Boss 每个主体 6 帧，城镇 NPC 每个主体 4 帧；头像、Buff、物品、Tile 与弹幕保持单帧。
- 多帧由同一母版派生，统一轮廓、配色、透明边距和左上光源，避免帧间变成不同设计。

## Buff

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/Buffs/AlchemyInsightBuff.png" alt="Alchemy Insight Buff" width="48"> | Alchemy Insight Buff | `buff icon` | 32x32 | - | 丹炉温养状态图标：丹炉附近的草木温养感，表现灵气恢复提升。 | `Content/Buffs/AlchemyInsightBuff.png` |
| <img src="../Content/Buffs/ArchiveLockBuff.png" alt="Archive Lock Buff" width="48"> | Archive Lock Buff | `buff icon` | 32x32 | - | 归档锁状态图标：金色环锁与归档线，表现行动受限。 | `Content/Buffs/ArchiveLockBuff.png` |
| <img src="../Content/Buffs/ArtifactResonanceBuff.png" alt="Artifact Resonance Buff" width="48"> | Artifact Resonance Buff | `buff icon` | 32x32 | - | 器胚共鸣状态图标：法器共振与器胚灵光，表现灵气消耗降低。 | `Content/Buffs/ArtifactResonanceBuff.png` |
| <img src="../Content/Buffs/QiGatheringBuff.png" alt="Qi Gathering Buff" width="48"> | Qi Gathering Buff | `buff icon` | 32x32 | - | 聚气状态图标：青玉灵气旋涡，表现灵气恢复与消耗优化。 | `Content/Buffs/QiGatheringBuff.png` |
| <img src="../Content/Buffs/SpiritualPressureDisorderBuff.png" alt="Spiritual Pressure Disorder Buff" width="48"> | Spiritual Pressure Disorder Buff | `buff icon` | 32x32 | - | 灵压紊乱状态图标：青紫裂纹与气旋失衡，表现防御和移动下降。 | `Content/Buffs/SpiritualPressureDisorderBuff.png` |
| <img src="../Content/Buffs/SpringReturnBuff.png" alt="Spring Return Buff" width="48"> | Spring Return Buff | `buff icon` | 32x32 | - | 回春状态图标：春意丹药与青绿生机，表现生命恢复和灵气回补。 | `Content/Buffs/SpringReturnBuff.png` |
| <img src="../Content/Buffs/SpringReturnRegenBuff.png" alt="Spring Return Regen Buff" width="48"> | Spring Return Regen Buff | `buff icon` | 32x32 | - | 回春再生状态图标：玉丹叶纹，表现生命与灵气恢复提升。 | `Content/Buffs/SpringReturnRegenBuff.png` |
| <img src="../Content/Buffs/StarAbyssCorrosionBuff.png" alt="Star Abyss Corrosion Buff" width="48"> | Star Abyss Corrosion Buff | `buff icon` | 32x32 | - | 星渊侵蚀状态图标：暗蓝星眼与紫色裂纹，表现防御下降和灵压增长。 | `Content/Buffs/StarAbyssCorrosionBuff.png` |
| <img src="../Content/Buffs/TribulationPressureBuff.png" alt="Tribulation Pressure Buff" width="48"> | Tribulation Pressure Buff | `buff icon` | 32x32 | - | 劫压临身状态图标：雷云威压与紫蓝电纹，表现天劫锁定。 | `Content/Buffs/TribulationPressureBuff.png` |
| <img src="../Content/Buffs/TribulationResistanceBuff.png" alt="Tribulation Resistance Buff" width="48"> | Tribulation Resistance Buff | `buff icon` | 32x32 | - | 抗劫状态图标：避雷玉符与稳定雷纹，表现伤害减免和灵压平复。 | `Content/Buffs/TribulationResistanceBuff.png` |

## Boss

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/NPCs/Bosses/AbyssalStarWomb.png" alt="Abyssal Star Womb" width="96"> | Abyssal Star Womb | `animation sheet` | 128x768 | 6 (128px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Bosses/AbyssalStarWomb.png` |
| <img src="../Content/NPCs/Bosses/AbyssalStarWomb_Head_Boss.png" alt="Abyssal Star Womb Head Boss" width="96"> | Abyssal Star Womb Head Boss | `boss head` | 32x32 | - | Boss 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Bosses/AbyssalStarWomb_Head_Boss.png` |
| <img src="../Content/NPCs/Bosses/BlackFurnaceIronGolem.png" alt="Black Furnace Iron Golem" width="96"> | Black Furnace Iron Golem | `animation sheet` | 112x672 | 6 (112px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Bosses/BlackFurnaceIronGolem.png` |
| <img src="../Content/NPCs/Bosses/BlackFurnaceIronGolem_Head_Boss.png" alt="Black Furnace Iron Golem Head Boss" width="96"> | Black Furnace Iron Golem Head Boss | `boss head` | 32x32 | - | Boss 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Bosses/BlackFurnaceIronGolem_Head_Boss.png` |
| <img src="../Content/NPCs/Bosses/BrokenHeavenInspector.png" alt="Broken Heaven Inspector" width="96"> | Broken Heaven Inspector | `animation sheet` | 128x768 | 6 (128px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Bosses/BrokenHeavenInspector.png` |
| <img src="../Content/NPCs/Bosses/BrokenHeavenInspector_Head_Boss.png" alt="Broken Heaven Inspector Head Boss" width="96"> | Broken Heaven Inspector Head Boss | `boss head` | 32x32 | - | Boss 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Bosses/BrokenHeavenInspector_Head_Boss.png` |
| <img src="../Content/NPCs/Bosses/FormlessSwordSoul.png" alt="Formless Sword Soul" width="96"> | Formless Sword Soul | `animation sheet` | 96x576 | 6 (96px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Bosses/FormlessSwordSoul.png` |
| <img src="../Content/NPCs/Bosses/FormlessSwordSoul_Head_Boss.png" alt="Formless Sword Soul Head Boss" width="96"> | Formless Sword Soul Head Boss | `boss head` | 32x32 | - | Boss 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Bosses/FormlessSwordSoul_Head_Boss.png` |
| <img src="../Content/NPCs/Bosses/GardenWarden.png" alt="Garden Warden" width="96"> | Garden Warden | `animation sheet` | 96x576 | 6 (96px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Bosses/GardenWarden.png` |
| <img src="../Content/NPCs/Bosses/GardenWarden_Head_Boss.png" alt="Garden Warden Head Boss" width="96"> | Garden Warden Head Boss | `boss head` | 32x32 | - | Boss 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Bosses/GardenWarden_Head_Boss.png` |
| <img src="../Content/NPCs/Bosses/GreenwoodMedicineKingEcho.png" alt="Greenwood Medicine King Echo" width="96"> | Greenwood Medicine King Echo | `animation sheet` | 112x672 | 6 (112px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Bosses/GreenwoodMedicineKingEcho.png` |
| <img src="../Content/NPCs/Bosses/GreenwoodMedicineKingEcho_Head_Boss.png" alt="Greenwood Medicine King Echo Head Boss" width="96"> | Greenwood Medicine King Echo Head Boss | `boss head` | 32x32 | - | Boss 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Bosses/GreenwoodMedicineKingEcho_Head_Boss.png` |
| <img src="../Content/NPCs/Bosses/HeavenTabletGuardian.png" alt="Heaven Tablet Guardian" width="96"> | Heaven Tablet Guardian | `animation sheet` | 96x960 | 6 (160px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Bosses/HeavenTabletGuardian.png` |
| <img src="../Content/NPCs/Bosses/HeavenTabletGuardian_Head_Boss.png" alt="Heaven Tablet Guardian Head Boss" width="96"> | Heaven Tablet Guardian Head Boss | `boss head` | 32x32 | - | Boss 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Bosses/HeavenTabletGuardian_Head_Boss.png` |
| <img src="../Content/NPCs/Bosses/MoonboneImmortal.png" alt="Moonbone Immortal" width="96"> | Moonbone Immortal | `animation sheet` | 180x1080 | 6 (180px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Bosses/MoonboneImmortal.png` |
| <img src="../Content/NPCs/Bosses/MoonboneImmortal_Head_Boss.png" alt="Moonbone Immortal Head Boss" width="96"> | Moonbone Immortal Head Boss | `boss head` | 48x48 | - | Boss 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Bosses/MoonboneImmortal_Head_Boss.png` |
| <img src="../Content/NPCs/Bosses/OldHeavenDaoCore.png" alt="Old Heaven Dao Core" width="96"> | Old Heaven Dao Core | `animation sheet` | 192x1152 | 6 (192px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Bosses/OldHeavenDaoCore.png` |
| <img src="../Content/NPCs/Bosses/OldHeavenDaoCore_Head_Boss.png" alt="Old Heaven Dao Core Head Boss" width="96"> | Old Heaven Dao Core Head Boss | `boss head` | 48x48 | - | Boss 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Bosses/OldHeavenDaoCore_Head_Boss.png` |
| <img src="../Content/NPCs/Bosses/SpiritVeinWyrm.png" alt="Spirit Vein Wyrm" width="96"> | Spirit Vein Wyrm | `animation sheet` | 96x192 | 6 (32px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Bosses/SpiritVeinWyrm.png` |
| <img src="../Content/NPCs/Bosses/SpiritVeinWyrm_Head_Boss.png" alt="Spirit Vein Wyrm Head Boss" width="96"> | Spirit Vein Wyrm Head Boss | `boss head` | 32x32 | - | Boss 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Bosses/SpiritVeinWyrm_Head_Boss.png` |
| <img src="../Content/NPCs/Bosses/ThunderMarshJiao.png" alt="Thunder Marsh Jiao" width="96"> | Thunder Marsh Jiao | `animation sheet` | 160x576 | 6 (96px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Bosses/ThunderMarshJiao.png` |
| <img src="../Content/NPCs/Bosses/ThunderMarshJiao_Head_Boss.png" alt="Thunder Marsh Jiao Head Boss" width="96"> | Thunder Marsh Jiao Head Boss | `boss head` | 48x48 | - | Boss 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Bosses/ThunderMarshJiao_Head_Boss.png` |
| <img src="../Content/NPCs/Bosses/TribulationCloudAvatar.png" alt="Tribulation Cloud Avatar" width="96"> | Tribulation Cloud Avatar | `animation sheet` | 128x576 | 6 (96px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Bosses/TribulationCloudAvatar.png` |
| <img src="../Content/NPCs/Bosses/TribulationCloudAvatar_Head_Boss.png" alt="Tribulation Cloud Avatar Head Boss" width="96"> | Tribulation Cloud Avatar Head Boss | `boss head` | 32x32 | - | Boss 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Bosses/TribulationCloudAvatar_Head_Boss.png` |

## Enemy

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/NPCs/Enemies/ArchivedImmortalSoul.png" alt="Archived Immortal Soul" width="80"> | Archived Immortal Soul | `animation sheet` | 72x432 | 6 (72px/frame) | 72x72，半透明仙魂，环形归档线，中心空洞；float 6 帧，copy 4 帧 | `Content/NPCs/Enemies/ArchivedImmortalSoul.png` |
| <img src="../Content/NPCs/Enemies/CelestialPuppet.png" alt="Celestial Puppet" width="80"> | Celestial Puppet | `animation sheet` | 64x480 | 6 (80px/frame) | 64x80，白玉傀儡，金线关节，无脸；walk 4 帧，attack 5 帧 | `Content/NPCs/Enemies/CelestialPuppet.png` |
| <img src="../Content/NPCs/Enemies/FurnaceAshGolem.png" alt="Furnace Ash Golem" width="80"> | Furnace Ash Golem | `animation sheet` | 64x384 | 6 (64px/frame) | 64x64，灰黑小傀儡，胸口暗红煤火；walk 4 帧，punch 4 帧 | `Content/NPCs/Enemies/FurnaceAshGolem.png` |
| <img src="../Content/NPCs/Enemies/HeavenTabletGuard.png" alt="Heaven Tablet Guard" width="80"> | Heaven Tablet Guard | `animation sheet` | 64x480 | 6 (80px/frame) | 64x80，碑甲卫士，盾像小天碑；guard 4 帧，blast 4 帧 | `Content/NPCs/Enemies/HeavenTabletGuard.png` |
| <img src="../Content/NPCs/Enemies/HerbGardenVineSpirit.png" alt="Herb Garden Vine Spirit" width="80"> | Herb Garden Vine Spirit | `animation sheet` | 64x384 | 6 (64px/frame) | 64x64，藤蔓人形，叶冠，根须腿；idle 4 帧，whip 5 帧 | `Content/NPCs/Enemies/HerbGardenVineSpirit.png` |
| <img src="../Content/NPCs/Enemies/IronShardSpirit.png" alt="Iron Shard Spirit" width="80"> | Iron Shard Spirit | `animation sheet` | 32x192 | 6 (32px/frame) | 32x32，漂浮铁片和小火光；spin 4 帧 | `Content/NPCs/Enemies/IronShardSpirit.png` |
| <img src="../Content/NPCs/Enemies/MiasmaFlowerMoth.png" alt="Miasma Flower Moth" width="80"> | Miasma Flower Moth | `animation sheet` | 48x288 | 6 (48px/frame) | 48x48，蝶翼带花纹，药绿和淡紫色板；fly 6 帧，miasma 3 帧 | `Content/NPCs/Enemies/MiasmaFlowerMoth.png` |
| <img src="../Content/NPCs/Enemies/MoonboneCultivator.png" alt="Moonbone Cultivator" width="80"> | Moonbone Cultivator | `animation sheet` | 72x480 | 6 (80px/frame) | 72x80，月白骨甲，残月披肩；dash 4 帧，cast 5 帧 | `Content/NPCs/Enemies/MoonboneCultivator.png` |
| <img src="../Content/NPCs/Enemies/ObsessedSwordCultivator.png" alt="Obsessed Sword Cultivator" width="80"> | Obsessed Sword Cultivator | `animation sheet` | 64x480 | 6 (80px/frame) | 64x80，残影剑修，破旧道袍，手持断剑；guard 3 帧，thrust 5 帧 | `Content/NPCs/Enemies/ObsessedSwordCultivator.png` |
| <img src="../Content/NPCs/Enemies/ScriptureArchiveEcho.png" alt="Scripture Archive Echo" width="80"> | Scripture Archive Echo | `animation sheet` | 64x384 | 6 (64px/frame) | 64x64，漂浮书卷和人形残影；cast 6 帧 | `Content/NPCs/Enemies/ScriptureArchiveEcho.png` |
| <img src="../Content/NPCs/Enemies/ShatteredJadeWorm.png" alt="Shattered Jade Worm" width="80"> | Shattered Jade Worm | `animation sheet` | 48x144 | 6 (24px/frame) | 48x24，虫形，玉壳断裂，深绿外轮廓；crawl 6 帧 | `Content/NPCs/Enemies/ShatteredJadeWorm.png` |
| <img src="../Content/NPCs/Enemies/StarAbyssLarva.png" alt="Star Abyss Larva" width="80"> | Star Abyss Larva | `animation sheet` | 48x192 | 6 (32px/frame) | 48x32，深蓝寄生幼体，星点眼；crawl 6 帧，leap 3 帧 | `Content/NPCs/Enemies/StarAbyssLarva.png` |
| <img src="../Content/NPCs/Enemies/StarEclipsedCultivator.png" alt="Star Eclipsed Cultivator" width="80"> | Star Eclipsed Cultivator | `animation sheet` | 64x384 | 6 (64px/frame) | 64x64，人形修士，暗蓝斗篷，星晶侵蚀半身；cast 5 帧 | `Content/NPCs/Enemies/StarEclipsedCultivator.png` |
| <img src="../Content/NPCs/Enemies/TalismanBat.png" alt="Talisman Bat" width="80"> | Talisman Bat | `animation sheet` | 48x192 | 6 (32px/frame) | 48x32，蝙蝠身体像折纸符箓，朱砂眼点；fly 6 帧 | `Content/NPCs/Enemies/TalismanBat.png` |
| <img src="../Content/NPCs/Enemies/ThunderPatternHawk.png" alt="Thunder Pattern Hawk" width="80"> | Thunder Pattern Hawk | `animation sheet` | 64x288 | 6 (48px/frame) | 64x48，鹰形，羽毛有蓝色雷纹；fly 6 帧，dive 3 帧 | `Content/NPCs/Enemies/ThunderPatternHawk.png` |
| <img src="../Content/NPCs/Enemies/TribulationCloudling.png" alt="Tribulation Cloudling" width="80"> | Tribulation Cloudling | `animation sheet` | 48x288 | 6 (48px/frame) | 48x48，紫色小雷云，有玉色面具碎片；float 6 帧 | `Content/NPCs/Enemies/TribulationCloudling.png` |
| <img src="../Content/NPCs/Enemies/WanderingSpiritSlime.png" alt="Wandering Spirit Slime" width="80"> | Wandering Spirit Slime | `animation sheet` | 48x288 | 6 (48px/frame) | 48x48，圆形青绿色史莱姆，体内漂浮小符核；idle 4 帧，hop 4 帧，hit 2 帧 | `Content/NPCs/Enemies/WanderingSpiritSlime.png` |

## Town NPC

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/NPCs/Town/ArchiveScrollSpirit.png" alt="Archive Scroll Spirit" width="72"> | Archive Scroll Spirit | `animation sheet` | 36x192 | 4 (48px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Town/ArchiveScrollSpirit.png` |
| <img src="../Content/NPCs/Town/ArchiveScrollSpirit_Head.png" alt="Archive Scroll Spirit Head" width="72"> | Archive Scroll Spirit Head | `town head` | 32x32 | - | Town NPC 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Town/ArchiveScrollSpirit_Head.png` |
| <img src="../Content/NPCs/Town/FallenHeavenMessenger.png" alt="Fallen Heaven Messenger" width="72"> | Fallen Heaven Messenger | `animation sheet` | 42x240 | 4 (60px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Town/FallenHeavenMessenger.png` |
| <img src="../Content/NPCs/Town/FallenHeavenMessenger_Head.png" alt="Fallen Heaven Messenger Head" width="72"> | Fallen Heaven Messenger Head | `town head` | 32x32 | - | Town NPC 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Town/FallenHeavenMessenger_Head.png` |
| <img src="../Content/NPCs/Town/HerbSectApprentice.png" alt="Herb Sect Apprentice" width="72"> | Herb Sect Apprentice | `animation sheet` | 40x224 | 4 (56px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Town/HerbSectApprentice.png` |
| <img src="../Content/NPCs/Town/HerbSectApprentice_Head.png" alt="Herb Sect Apprentice Head" width="72"> | Herb Sect Apprentice Head | `town head` | 32x32 | - | Town NPC 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Town/HerbSectApprentice_Head.png` |
| <img src="../Content/NPCs/Town/TribulationObserver.png" alt="Tribulation Observer" width="72"> | Tribulation Observer | `animation sheet` | 40x224 | 4 (56px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Town/TribulationObserver.png` |
| <img src="../Content/NPCs/Town/TribulationObserver_Head.png" alt="Tribulation Observer Head" width="72"> | Tribulation Observer Head | `town head` | 32x32 | - | Town NPC 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Town/TribulationObserver_Head.png` |
| <img src="../Content/NPCs/Town/WanderingArtificer.png" alt="Wandering Artificer" width="72"> | Wandering Artificer | `animation sheet` | 42x232 | 4 (58px/frame) | 多帧竖向 spritesheet：由已验收单帧母版派生，保持同一轮廓、同一配色和同一光源，用于 idle/move/attack 节奏表现。 | `Content/NPCs/Town/WanderingArtificer.png` |
| <img src="../Content/NPCs/Town/WanderingArtificer_Head.png" alt="Wandering Artificer Head" width="72"> | Wandering Artificer Head | `town head` | 32x32 | - | Town NPC 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/NPCs/Town/WanderingArtificer_Head.png` |

## Crafting Station Tile

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/Tiles/Stations/AlchemyCauldronTile.png" alt="Alchemy Cauldron Tile" width="72"> | Alchemy Cauldron Tile | `tile` | 32x32 | - | 64x64 青铜丹炉，青木根缠绕，药绿火焰；用于炼制丹药。 | `Content/Tiles/Stations/AlchemyCauldronTile.png` |
| <img src="../Content/Tiles/Stations/ArtifactForgeTile.png" alt="Artifact Forge Tile" width="72"> | Artifact Forge Tile | `tile` | 32x48 | - | 64x48 黑铁台、迷你炉口、悬浮铭刻线；用于铸造飞剑、阵盘和法器。 | `Content/Tiles/Stations/ArtifactForgeTile.png` |
| <img src="../Content/Tiles/Stations/DaoSeveringAltarTile.png" alt="Dao Severing Altar Tile" width="72"> | Dao Severing Altar Tile | `tile` | 80x48 | - | 80x48 黑白断环石台，中间细小裂隙；用于终局路线装备。 | `Content/Tiles/Stations/DaoSeveringAltarTile.png` |
| <img src="../Content/Tiles/Stations/EarthClayFurnaceTile.png" alt="Earth Clay Furnace Tile" width="72"> | Earth Clay Furnace Tile | `tile` | 48x48 | - | 48x48 小陶炉，暗红炉口，青色药烟硬边；用于早期炼丹。 | `Content/Tiles/Stations/EarthClayFurnaceTile.png` |
| <img src="../Content/Tiles/Stations/HeavenFireFurnaceTile.png" alt="Heaven Fire Furnace Tile" width="72"> | Heaven Fire Furnace Tile | `tile` | 64x64 | - | 64x64 白玉炉，金色天火，破损法旨环绕；用于天道系装备。 | `Content/Tiles/Stations/HeavenFireFurnaceTile.png` |
| <img src="../Content/Tiles/Stations/SectTrialAltarTile.png" alt="Sect Trial Altar Tile" width="72"> | Sect Trial Altar Tile | `tile` | 64x48 | - | 64x48 白石台、插剑、玉牌槽；用于宗门职业装备。 | `Content/Tiles/Stations/SectTrialAltarTile.png` |
| <img src="../Content/Tiles/Stations/SimpleTalismanTableTile.png" alt="Simple Talisman Table Tile" width="72"> | Simple Talisman Table Tile | `tile` | 48x32 | - | 48x32 矮木案、纸张、朱砂碟，无可读文字；用于绘制基础符箓。 | `Content/Tiles/Stations/SimpleTalismanTableTile.png` |
| <img src="../Content/Tiles/Stations/StarPatternCauldronTile.png" alt="Star Pattern Cauldron Tile" width="72"> | Star Pattern Cauldron Tile | `tile` | 64x64 | - | 64x64 暗蓝丹鼎，星晶嵌边，紫黑火焰；用于星渊高阶丹药。 | `Content/Tiles/Stations/StarPatternCauldronTile.png` |
| <img src="../Content/Tiles/Stations/ThunderPatternForgeTile.png" alt="Thunder Pattern Forge Tile" width="72"> | Thunder Pattern Forge Tile | `tile` | 64x48 | - | 64x48 云铁锻台，紫蓝雷纹，小电弧；用于雷系装备。 | `Content/Tiles/Stations/ThunderPatternForgeTile.png` |

## Crafting Station Item

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/Items/Stations/AlchemyCauldron.png" alt="Alchemy Cauldron" width="48"> | Alchemy Cauldron | `item icon` | 32x32 | - | 64x64 青铜丹炉，青木根缠绕，药绿火焰；用于炼制丹药。 | `Content/Items/Stations/AlchemyCauldron.png` |
| <img src="../Content/Items/Stations/ArtifactForge.png" alt="Artifact Forge" width="48"> | Artifact Forge | `item icon` | 32x48 | - | 64x48 黑铁台、迷你炉口、悬浮铭刻线；用于铸造飞剑、阵盘和法器。 | `Content/Items/Stations/ArtifactForge.png` |
| <img src="../Content/Items/Stations/DaoSeveringAltar.png" alt="Dao Severing Altar" width="48"> | Dao Severing Altar | `item icon` | 32x32 | - | 80x48 黑白断环石台，中间细小裂隙；用于终局路线装备。 | `Content/Items/Stations/DaoSeveringAltar.png` |
| <img src="../Content/Items/Stations/EarthClayFurnace.png" alt="Earth Clay Furnace" width="48"> | Earth Clay Furnace | `item icon` | 32x32 | - | 48x48 小陶炉，暗红炉口，青色药烟硬边；用于早期炼丹。 | `Content/Items/Stations/EarthClayFurnace.png` |
| <img src="../Content/Items/Stations/HeavenFireFurnace.png" alt="Heaven Fire Furnace" width="48"> | Heaven Fire Furnace | `item icon` | 32x32 | - | 64x64 白玉炉，金色天火，破损法旨环绕；用于天道系装备。 | `Content/Items/Stations/HeavenFireFurnace.png` |
| <img src="../Content/Items/Stations/SectTrialAltar.png" alt="Sect Trial Altar" width="48"> | Sect Trial Altar | `item icon` | 32x32 | - | 64x48 白石台、插剑、玉牌槽；用于宗门职业装备。 | `Content/Items/Stations/SectTrialAltar.png` |
| <img src="../Content/Items/Stations/SimpleTalismanTable.png" alt="Simple Talisman Table" width="48"> | Simple Talisman Table | `item icon` | 32x32 | - | 48x32 矮木案、纸张、朱砂碟，无可读文字；用于绘制基础符箓。 | `Content/Items/Stations/SimpleTalismanTable.png` |
| <img src="../Content/Items/Stations/StarPatternCauldron.png" alt="Star Pattern Cauldron" width="48"> | Star Pattern Cauldron | `item icon` | 32x32 | - | 64x64 暗蓝丹鼎，星晶嵌边，紫黑火焰；用于星渊高阶丹药。 | `Content/Items/Stations/StarPatternCauldron.png` |
| <img src="../Content/Items/Stations/ThunderPatternForge.png" alt="Thunder Pattern Forge" width="48"> | Thunder Pattern Forge | `item icon` | 32x32 | - | 64x48 云铁锻台，紫蓝雷纹，小电弧；用于雷系装备。 | `Content/Items/Stations/ThunderPatternForge.png` |

## Tile / Object

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/Tiles/ArchiveLightPillarTile.png" alt="Archive Light Pillar Tile" width="64"> | Archive Light Pillar Tile | `tile/object` | 32x96 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/ArchiveLightPillarTile.png` |
| <img src="../Content/Tiles/BlackFurnaceWall.png" alt="Black Furnace Wall" width="64"> | Black Furnace Wall | `tile/object` | 16x16 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/BlackFurnaceWall.png` |
| <img src="../Content/Tiles/BrokenHeavenTabletTile.png" alt="Broken Heaven Tablet Tile" width="64"> | Broken Heaven Tablet Tile | `tile/object` | 32x64 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/BrokenHeavenTabletTile.png` |
| <img src="../Content/Tiles/FallenHeavenJadeTile.png" alt="Fallen Heaven Jade Tile" width="64"> | Fallen Heaven Jade Tile | `tile/object` | 16x16 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/FallenHeavenJadeTile.png` |
| <img src="../Content/Tiles/FurnaceSlagTile.png" alt="Furnace Slag Tile" width="64"> | Furnace Slag Tile | `tile/object` | 16x16 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/FurnaceSlagTile.png` |
| <img src="../Content/Tiles/GreenwoodSoilTile.png" alt="Greenwood Soil Tile" width="64"> | Greenwood Soil Tile | `tile/object` | 16x16 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/GreenwoodSoilTile.png` |
| <img src="../Content/Tiles/MoonboneTile.png" alt="Moonbone Tile" width="64"> | Moonbone Tile | `tile/object` | 16x16 | - | 32x32，月白骨片，冷蓝裂纹 | `Content/Tiles/MoonboneTile.png` |
| <img src="../Content/Tiles/RiftMembraneTile.png" alt="Rift Membrane Tile" width="64"> | Rift Membrane Tile | `tile/object` | 32x32 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/RiftMembraneTile.png` |
| <img src="../Content/Tiles/SectRuinBrickTile.png" alt="Sect Ruin Brick Tile" width="64"> | Sect Ruin Brick Tile | `tile/object` | 16x16 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/SectRuinBrickTile.png` |
| <img src="../Content/Tiles/SingingThunderStoneTile.png" alt="Singing Thunder Stone Tile" width="64"> | Singing Thunder Stone Tile | `tile/object` | 24x32 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/SingingThunderStoneTile.png` |
| <img src="../Content/Tiles/SpiritHerbTile.png" alt="Spirit Herb Tile" width="64"> | Spirit Herb Tile | `tile/object` | 16x24 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/SpiritHerbTile.png` |
| <img src="../Content/Tiles/SpiritMossTile.png" alt="Spirit Moss Tile" width="64"> | Spirit Moss Tile | `tile/object` | 16x16 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/SpiritMossTile.png` |
| <img src="../Content/Tiles/SpiritOreTile.png" alt="Spirit Ore Tile" width="64"> | Spirit Ore Tile | `tile/object` | 16x16 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/SpiritOreTile.png` |
| <img src="../Content/Tiles/StarAbyssCrystalTile.png" alt="Star Abyss Crystal Tile" width="64"> | Star Abyss Crystal Tile | `tile/object` | 16x16 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/StarAbyssCrystalTile.png` |
| <img src="../Content/Tiles/SwordTabletTile.png" alt="Sword Tablet Tile" width="64"> | Sword Tablet Tile | `tile/object` | 32x48 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/SwordTabletTile.png` |
| <img src="../Content/Tiles/ThunderCloudTile.png" alt="Thunder Cloud Tile" width="64"> | Thunder Cloud Tile | `tile/object` | 16x16 | - | Tile / Object 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Tiles/ThunderCloudTile.png` |

## Projectile

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/Projectiles/BossArrayFieldProjectile.png" alt="Boss Array Field Projectile" width="64"> | Boss Array Field Projectile | `projectile` | 28x28 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/BossArrayFieldProjectile.png` |
| <img src="../Content/Projectiles/BossSpiritBoltProjectile.png" alt="Boss Spirit Bolt Projectile" width="64"> | Boss Spirit Bolt Projectile | `projectile` | 18x18 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/BossSpiritBoltProjectile.png` |
| <img src="../Content/Projectiles/CinnabarTalismanFlame.png" alt="Cinnabar Talisman Flame" width="64"> | Cinnabar Talisman Flame | `projectile` | 24x24 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/CinnabarTalismanFlame.png` |
| <img src="../Content/Projectiles/CloudpiercerSwordProjectile.png" alt="Cloudpiercer Sword Projectile" width="64"> | Cloudpiercer Sword Projectile | `projectile` | 48x16 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/CloudpiercerSwordProjectile.png` |
| <img src="../Content/Projectiles/CloudWispProjectile.png" alt="Cloud Wisp Projectile" width="64"> | Cloud Wisp Projectile | `projectile` | 24x16 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/CloudWispProjectile.png` |
| <img src="../Content/Projectiles/DecreeJudgementBeam.png" alt="Decree Judgement Beam" width="64"> | Decree Judgement Beam | `projectile` | 32x128 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/DecreeJudgementBeam.png` |
| <img src="../Content/Projectiles/FormlessSwordWheelProjectile.png" alt="Formless Sword Wheel Projectile" width="64"> | Formless Sword Wheel Projectile | `projectile` | 64x64 | - | 64x64，环形剑轮，中心空洞，青白剑影；projectile 64x64 | `Content/Projectiles/FormlessSwordWheelProjectile.png` |
| <img src="../Content/Projectiles/GreenwoodArrayField.png" alt="Greenwood Array Field" width="64"> | Greenwood Array Field | `projectile` | 96x96 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/GreenwoodArrayField.png` |
| <img src="../Content/Projectiles/MinorThunderboltProjectile.png" alt="Minor Thunderbolt Projectile" width="64"> | Minor Thunderbolt Projectile | `projectile` | 16x64 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/MinorThunderboltProjectile.png` |
| <img src="../Content/Projectiles/MoonboneShardProjectile.png" alt="Moonbone Shard Projectile" width="64"> | Moonbone Shard Projectile | `projectile` | 24x16 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/MoonboneShardProjectile.png` |
| <img src="../Content/Projectiles/SpiritBolt.png" alt="Spirit Bolt" width="64"> | Spirit Bolt | `projectile` | 16x8 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/SpiritBolt.png` |
| <img src="../Content/Projectiles/SpiritBoltProjectile.png" alt="Spirit Bolt Projectile" width="64"> | Spirit Bolt Projectile | `projectile` | 16x8 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/SpiritBoltProjectile.png` |
| <img src="../Content/Projectiles/StarEclipseSplitBolt.png" alt="Star Eclipse Split Bolt" width="64"> | Star Eclipse Split Bolt | `projectile` | 24x24 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/StarEclipseSplitBolt.png` |
| <img src="../Content/Projectiles/ThunderSwordProjectile.png" alt="Thunder Sword Projectile" width="64"> | Thunder Sword Projectile | `projectile` | 48x16 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/ThunderSwordProjectile.png` |
| <img src="../Content/Projectiles/ThunderTalismanArray.png" alt="Thunder Talisman Array" width="64"> | Thunder Talisman Array | `projectile` | 96x96 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/ThunderTalismanArray.png` |
| <img src="../Content/Projectiles/TribulationLightningProjectile.png" alt="Tribulation Lightning Projectile" width="64"> | Tribulation Lightning Projectile | `projectile` | 16x48 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/TribulationLightningProjectile.png` |
| <img src="../Content/Projectiles/TribulationWarningLineProjectile.png" alt="Tribulation Warning Line Projectile" width="64"> | Tribulation Warning Line Projectile | `projectile` | 16x4 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/TribulationWarningLineProjectile.png` |
| <img src="../Content/Projectiles/WoodgrainSwordProjectile.png" alt="Woodgrain Sword Projectile" width="64"> | Woodgrain Sword Projectile | `projectile` | 32x16 | - | Projectile 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Projectiles/WoodgrainSwordProjectile.png` |

## Equipment

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/Items/Weapons/BrokenHeavenDecree.png" alt="Broken Heaven Decree" width="64"> | Broken Heaven Decree | `item icon` | 48x48 | - | Equipment 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Weapons/BrokenHeavenDecree.png` |
| <img src="../Content/Items/Weapons/CinnabarTalismanFlameItem.png" alt="Cinnabar Talisman Flame Item" width="64"> | Cinnabar Talisman Flame Item | `item icon` | 32x32 | - | Equipment 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Weapons/CinnabarTalismanFlameItem.png` |
| <img src="../Content/Items/Weapons/CloudpiercerFlyingSword.png" alt="Cloudpiercer Flying Sword" width="64"> | Cloudpiercer Flying Sword | `item icon` | 64x64 | - | 64x64 item icon，细长银剑，云形剑格，淡青尾光；projectile 48x16 | `Content/Items/Weapons/CloudpiercerFlyingSword.png` |
| <img src="../Content/Items/Weapons/FormlessSwordWheel.png" alt="Formless Sword Wheel" width="64"> | Formless Sword Wheel | `item icon` | 64x64 | - | 64x64，环形剑轮，中心空洞，青白剑影；projectile 64x64 | `Content/Items/Weapons/FormlessSwordWheel.png` |
| <img src="../Content/Items/Weapons/GreenwoodArrayPlate.png" alt="Greenwood Array Plate" width="64"> | Greenwood Array Plate | `item icon` | 48x48 | - | Equipment 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Weapons/GreenwoodArrayPlate.png` |
| <img src="../Content/Items/Weapons/MoonboneDharmaSword.png" alt="Moonbone Dharma Sword" width="64"> | Moonbone Dharma Sword | `item icon` | 64x64 | - | 64x64，月白骨剑，暗蓝核心，残月护手；projectile 56x20 | `Content/Items/Weapons/MoonboneDharmaSword.png` |
| <img src="../Content/Items/Weapons/OldHeavenDaoScroll.png" alt="Old Heaven Dao Scroll" width="64"> | Old Heaven Dao Scroll | `item icon` | 64x64 | - | Equipment 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Weapons/OldHeavenDaoScroll.png` |
| <img src="../Content/Items/Weapons/SpiritwoodCrossbow.png" alt="Spiritwood Crossbow" width="64"> | Spiritwood Crossbow | `item icon` | 48x48 | - | Equipment 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Weapons/SpiritwoodCrossbow.png` |
| <img src="../Content/Items/Weapons/StarEclipseArbalest.png" alt="Star Eclipse Arbalest" width="64"> | Star Eclipse Arbalest | `item icon` | 64x64 | - | Equipment 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Weapons/StarEclipseArbalest.png` |
| <img src="../Content/Items/Weapons/ThunderPatternSwordCase.png" alt="Thunder Pattern Sword Case" width="64"> | Thunder Pattern Sword Case | `item icon` | 64x64 | - | 64x64，半开剑匣，三把小雷剑，紫蓝雷纹；projectiles 48x16 | `Content/Items/Weapons/ThunderPatternSwordCase.png` |
| <img src="../Content/Items/Weapons/ThunderTalismanArrayPlate.png" alt="Thunder Talisman Array Plate" width="64"> | Thunder Talisman Array Plate | `item icon` | 48x48 | - | Equipment 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Weapons/ThunderTalismanArrayPlate.png` |
| <img src="../Content/Items/Weapons/WoodgrainFlyingSword.png" alt="Woodgrain Flying Sword" width="64"> | Woodgrain Flying Sword | `item icon` | 48x48 | - | 48x48 item icon，木质剑身，青色灵纹；projectile 32x16 | `Content/Items/Weapons/WoodgrainFlyingSword.png` |

## Accessory

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/Items/Accessories/BrokenHeavenCrownSeal.png" alt="Broken Heaven Crown Seal" width="56"> | Broken Heaven Crown Seal | `item icon` | 32x32 | - | Accessory 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Accessories/BrokenHeavenCrownSeal.png` |
| <img src="../Content/Items/Accessories/DaoSeveringRing.png" alt="Dao Severing Ring" width="56"> | Dao Severing Ring | `item icon` | 32x32 | - | Accessory 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Accessories/DaoSeveringRing.png` |
| <img src="../Content/Items/Accessories/FurnaceHeartRing.png" alt="Furnace Heart Ring" width="56"> | Furnace Heart Ring | `item icon` | 32x32 | - | Accessory 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Accessories/FurnaceHeartRing.png` |
| <img src="../Content/Items/Accessories/LightningWardJade.png" alt="Lightning Ward Jade" width="56"> | Lightning Ward Jade | `item icon` | 32x32 | - | Accessory 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Accessories/LightningWardJade.png` |
| <img src="../Content/Items/Accessories/NascentSoulJadeBox.png" alt="Nascent Soul Jade Box" width="56"> | Nascent Soul Jade Box | `item icon` | 32x32 | - | Accessory 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Accessories/NascentSoulJadeBox.png` |
| <img src="../Content/Items/Accessories/QiGatheringPendant.png" alt="Qi Gathering Pendant" width="56"> | Qi Gathering Pendant | `item icon` | 32x32 | - | Accessory 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Accessories/QiGatheringPendant.png` |
| <img src="../Content/Items/Accessories/SpiritwoodCharm.png" alt="Spiritwood Charm" width="56"> | Spiritwood Charm | `item icon` | 32x32 | - | Accessory 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Accessories/SpiritwoodCharm.png` |
| <img src="../Content/Items/Accessories/StarAbyssEye.png" alt="Star Abyss Eye" width="56"> | Star Abyss Eye | `item icon` | 32x32 | - | Accessory 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Accessories/StarAbyssEye.png` |

## Boss Summon

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/Items/BossSummons/SpiritVeinIncense.png" alt="Spirit Vein Incense" width="48"> | Spirit Vein Incense | `item icon` | 32x32 | - | Boss Summon 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/BossSummons/SpiritVeinIncense.png` |
| <img src="../Content/Items/BossSummons/SummonGardenBrokenKey.png" alt="Summon Garden Broken Key" width="48"> | Summon Garden Broken Key | `item icon` | 32x32 | - | Boss Summon 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/BossSummons/SummonGardenBrokenKey.png` |
| <img src="../Content/Items/BossSummons/SummonHeavenTabletRubbing.png" alt="Summon Heaven Tablet Rubbing" width="48"> | Summon Heaven Tablet Rubbing | `item icon` | 32x32 | - | Boss Summon 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/BossSummons/SummonHeavenTabletRubbing.png` |
| <img src="../Content/Items/BossSummons/SummonHeavenTabletRubbingBrokenHeavenInspector.png" alt="Summon Heaven Tablet Rubbing Broken Heaven Inspector" width="48"> | Summon Heaven Tablet Rubbing Broken Heaven Inspector | `item icon` | 32x32 | - | Boss Summon 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/BossSummons/SummonHeavenTabletRubbingBrokenHeavenInspector.png` |
| <img src="../Content/Items/BossSummons/SummonMoonboneRitualTalisman.png" alt="Summon Moonbone Ritual Talisman" width="48"> | Summon Moonbone Ritual Talisman | `item icon` | 32x32 | - | Boss Summon 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/BossSummons/SummonMoonboneRitualTalisman.png` |
| <img src="../Content/Items/BossSummons/SummonMoonboneRitualTalismanOldHeavenDaoCore.png" alt="Summon Moonbone Ritual Talisman Old Heaven Dao Core" width="48"> | Summon Moonbone Ritual Talisman Old Heaven Dao Core | `item icon` | 32x32 | - | Boss Summon 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/BossSummons/SummonMoonboneRitualTalismanOldHeavenDaoCore.png` |
| <img src="../Content/Items/BossSummons/SummonOldFurnaceEmber.png" alt="Summon Old Furnace Ember" width="48"> | Summon Old Furnace Ember | `item icon` | 24x24 | - | Boss Summon 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/BossSummons/SummonOldFurnaceEmber.png` |
| <img src="../Content/Items/BossSummons/SummonSectTrialToken.png" alt="Summon Sect Trial Token" width="48"> | Summon Sect Trial Token | `item icon` | 32x32 | - | Boss Summon 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/BossSummons/SummonSectTrialToken.png` |
| <img src="../Content/Items/BossSummons/SummonSectTrialTokenGreenwoodMedicineKingEcho.png" alt="Summon Sect Trial Token Greenwood Medicine King Echo" width="48"> | Summon Sect Trial Token Greenwood Medicine King Echo | `item icon` | 32x32 | - | Boss Summon 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/BossSummons/SummonSectTrialTokenGreenwoodMedicineKingEcho.png` |
| <img src="../Content/Items/BossSummons/SummonStarAbyssMembrane.png" alt="Summon Star Abyss Membrane" width="48"> | Summon Star Abyss Membrane | `item icon` | 32x32 | - | Boss Summon 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/BossSummons/SummonStarAbyssMembrane.png` |
| <img src="../Content/Items/BossSummons/SummonThunderCallingJade.png" alt="Summon Thunder Calling Jade" width="48"> | Summon Thunder Calling Jade | `item icon` | 32x32 | - | Boss Summon 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/BossSummons/SummonThunderCallingJade.png` |
| <img src="../Content/Items/BossSummons/SummonThunderCallingJadeThunderMarshJiao.png" alt="Summon Thunder Calling Jade Thunder Marsh Jiao" width="48"> | Summon Thunder Calling Jade Thunder Marsh Jiao | `item icon` | 32x32 | - | Boss Summon 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/BossSummons/SummonThunderCallingJadeThunderMarshJiao.png` |

## Consumable

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/Items/Consumables/QiDrawingTalisman.png" alt="Qi Drawing Talisman" width="48"> | Qi Drawing Talisman | `item icon` | 32x32 | - | Consumable 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Consumables/QiDrawingTalisman.png` |

## Guide Item

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/Items/Guides/SectLedger.png" alt="Sect Ledger" width="48"> | Sect Ledger | `item icon` | 32x32 | - | Guide Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Guides/SectLedger.png` |
| <img src="../Content/Items/Guides/TribulationGauge.png" alt="Tribulation Gauge" width="48"> | Tribulation Gauge | `item icon` | 32x32 | - | Guide Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Guides/TribulationGauge.png` |

## Material

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/Items/Materials/ArtifactBlankShard.png" alt="Artifact Blank Shard" width="48"> | Artifact Blank Shard | `item icon` | 24x24 | - | 24x24，银灰碎片，边缘有未完成铭纹 | `Content/Items/Materials/ArtifactBlankShard.png` |
| <img src="../Content/Items/Materials/BrokenHeavenCrownSeal.png" alt="Broken Heaven Crown Seal" width="48"> | Broken Heaven Crown Seal | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/BrokenHeavenCrownSeal.png` |
| <img src="../Content/Items/Materials/BrokenHeavenDecree.png" alt="Broken Heaven Decree" width="48"> | Broken Heaven Decree | `item icon` | 48x48 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/BrokenHeavenDecree.png` |
| <img src="../Content/Items/Materials/CinnabarTalismanFlameItem.png" alt="Cinnabar Talisman Flame Item" width="48"> | Cinnabar Talisman Flame Item | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/CinnabarTalismanFlameItem.png` |
| <img src="../Content/Items/Materials/CloudpiercerFlyingSword.png" alt="Cloudpiercer Flying Sword" width="48"> | Cloudpiercer Flying Sword | `item icon` | 64x64 | - | 64x64 item icon，细长银剑，云形剑格，淡青尾光；projectile 48x16 | `Content/Items/Materials/CloudpiercerFlyingSword.png` |
| <img src="../Content/Items/Materials/DaoSeveringDust.png" alt="Dao Severing Dust" width="48"> | Dao Severing Dust | `item icon` | 32x32 | - | 32x32，黑白尘粒围绕小裂隙，边缘清楚 | `Content/Items/Materials/DaoSeveringDust.png` |
| <img src="../Content/Items/Materials/DaoSeveringRing.png" alt="Dao Severing Ring" width="48"> | Dao Severing Ring | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/DaoSeveringRing.png` |
| <img src="../Content/Items/Materials/FormlessSwordWheel.png" alt="Formless Sword Wheel" width="48"> | Formless Sword Wheel | `item icon` | 64x64 | - | 64x64，环形剑轮，中心空洞，青白剑影；projectile 64x64 | `Content/Items/Materials/FormlessSwordWheel.png` |
| <img src="../Content/Items/Materials/FoundationPill.png" alt="Foundation Pill" width="48"> | Foundation Pill | `item icon` | 24x24 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/FoundationPill.png` |
| <img src="../Content/Items/Materials/FurnaceHeartRing.png" alt="Furnace Heart Ring" width="48"> | Furnace Heart Ring | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/FurnaceHeartRing.png` |
| <img src="../Content/Items/Materials/FurnaceSlagIron.png" alt="Furnace Slag Iron" width="48"> | Furnace Slag Iron | `item icon` | 24x24 | - | 24x24，暗铁矿渣，橙红裂纹 | `Content/Items/Materials/FurnaceSlagIron.png` |
| <img src="../Content/Items/Materials/GardenBrokenKey.png" alt="Garden Broken Key" width="48"> | Garden Broken Key | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/GardenBrokenKey.png` |
| <img src="../Content/Items/Materials/GreenwoodArrayPlate.png" alt="Greenwood Array Plate" width="48"> | Greenwood Array Plate | `item icon` | 48x48 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/GreenwoodArrayPlate.png` |
| <img src="../Content/Items/Materials/GreenwoodRoot.png" alt="Greenwood Root" width="48"> | Greenwood Root | `item icon` | 24x24 | - | 24x24，盘结青根，叶脉光点 | `Content/Items/Materials/GreenwoodRoot.png` |
| <img src="../Content/Items/Materials/HeavenDaoFragment.png" alt="Heaven Dao Fragment" width="48"> | Heaven Dao Fragment | `item icon` | 32x32 | - | 32x32，白玉碎片，残金线条，冷白光 | `Content/Items/Materials/HeavenDaoFragment.png` |
| <img src="../Content/Items/Materials/HeavenTabletRubbing.png" alt="Heaven Tablet Rubbing" width="48"> | Heaven Tablet Rubbing | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/HeavenTabletRubbing.png` |
| <img src="../Content/Items/Materials/LightningWardJade.png" alt="Lightning Ward Jade" width="48"> | Lightning Ward Jade | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/LightningWardJade.png` |
| <img src="../Content/Items/Materials/LowGradeSpiritStone.png" alt="Low Grade Spirit Stone" width="48"> | Low Grade Spirit Stone | `item icon` | 24x24 | - | 16x16 或 24x24，青玉小晶体，中心浅青发光，深绿描边 | `Content/Items/Materials/LowGradeSpiritStone.png` |
| <img src="../Content/Items/Materials/Moonbone.png" alt="Moonbone" width="48"> | Moonbone | `item icon` | 32x32 | - | 32x32，月白骨片，冷蓝裂纹 | `Content/Items/Materials/Moonbone.png` |
| <img src="../Content/Items/Materials/MoonboneDharmaSword.png" alt="Moonbone Dharma Sword" width="48"> | Moonbone Dharma Sword | `item icon` | 64x64 | - | 64x64，月白骨剑，暗蓝核心，残月护手；projectile 56x20 | `Content/Items/Materials/MoonboneDharmaSword.png` |
| <img src="../Content/Items/Materials/MoonboneRitualTalisman.png" alt="Moonbone Ritual Talisman" width="48"> | Moonbone Ritual Talisman | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/MoonboneRitualTalisman.png` |
| <img src="../Content/Items/Materials/NascentSoulJadeBox.png" alt="Nascent Soul Jade Box" width="48"> | Nascent Soul Jade Box | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/NascentSoulJadeBox.png` |
| <img src="../Content/Items/Materials/OldFurnaceEmber.png" alt="Old Furnace Ember" width="48"> | Old Furnace Ember | `item icon` | 24x24 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/OldFurnaceEmber.png` |
| <img src="../Content/Items/Materials/OldHeavenDaoScroll.png" alt="Old Heaven Dao Scroll" width="48"> | Old Heaven Dao Scroll | `item icon` | 64x64 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/OldHeavenDaoScroll.png` |
| <img src="../Content/Items/Materials/QiCondensingPill.png" alt="Qi Condensing Pill" width="48"> | Qi Condensing Pill | `item icon` | 24x24 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/QiCondensingPill.png` |
| <img src="../Content/Items/Materials/QiGatheringPendant.png" alt="Qi Gathering Pendant" width="48"> | Qi Gathering Pendant | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/QiGatheringPendant.png` |
| <img src="../Content/Items/Materials/SectTrialToken.png" alt="Sect Trial Token" width="48"> | Sect Trial Token | `item icon` | 32x32 | - | 32x32，玉牌，断裂挂绳，金色简化纹 | `Content/Items/Materials/SectTrialToken.png` |
| <img src="../Content/Items/Materials/SpiritGel.png" alt="Spirit Gel" width="48"> | Spirit Gel | `item icon` | 16x16 | - | 16x16，半透明感用硬边高光表达，不能糊 | `Content/Items/Materials/SpiritGel.png` |
| <img src="../Content/Items/Materials/SpiritwoodCharm.png" alt="Spiritwood Charm" width="48"> | Spiritwood Charm | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/SpiritwoodCharm.png` |
| <img src="../Content/Items/Materials/SpringReturnPill.png" alt="Spring Return Pill" width="48"> | Spring Return Pill | `item icon` | 16x16 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/SpringReturnPill.png` |
| <img src="../Content/Items/Materials/StarAbyssEye.png" alt="Star Abyss Eye" width="48"> | Star Abyss Eye | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/StarAbyssEye.png` |
| <img src="../Content/Items/Materials/StarAbyssForbiddenTalisman.png" alt="Star Abyss Forbidden Talisman" width="48"> | Star Abyss Forbidden Talisman | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/StarAbyssForbiddenTalisman.png` |
| <img src="../Content/Items/Materials/StarAbyssMembrane.png" alt="Star Abyss Membrane" width="48"> | Star Abyss Membrane | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/StarAbyssMembrane.png` |
| <img src="../Content/Items/Materials/StarEclipseArbalest.png" alt="Star Eclipse Arbalest" width="48"> | Star Eclipse Arbalest | `item icon` | 64x64 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/StarEclipseArbalest.png` |
| <img src="../Content/Items/Materials/StarEclipseCrystal.png" alt="Star Eclipse Crystal" width="48"> | Star Eclipse Crystal | `item icon` | 24x24 | - | 24x24，深蓝晶体，紫黑外缘，白色星点 | `Content/Items/Materials/StarEclipseCrystal.png` |
| <img src="../Content/Items/Materials/ThunderCallingJade.png" alt="Thunder Calling Jade" width="48"> | Thunder Calling Jade | `item icon` | 32x32 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/ThunderCallingJade.png` |
| <img src="../Content/Items/Materials/ThunderPatternSwordCase.png" alt="Thunder Pattern Sword Case" width="48"> | Thunder Pattern Sword Case | `item icon` | 64x64 | - | 64x64，半开剑匣，三把小雷剑，紫蓝雷纹；projectiles 48x16 | `Content/Items/Materials/ThunderPatternSwordCase.png` |
| <img src="../Content/Items/Materials/ThunderTalismanArrayPlate.png" alt="Thunder Talisman Array Plate" width="48"> | Thunder Talisman Array Plate | `item icon` | 48x48 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/ThunderTalismanArrayPlate.png` |
| <img src="../Content/Items/Materials/TribulationCloudDew.png" alt="Tribulation Cloud Dew" width="48"> | Tribulation Cloud Dew | `item icon` | 24x24 | - | 24x24，紫蓝水滴，内部有小闪电 | `Content/Items/Materials/TribulationCloudDew.png` |
| <img src="../Content/Items/Materials/TribulationResistingPill.png" alt="Tribulation Resisting Pill" width="48"> | Tribulation Resisting Pill | `item icon` | 24x24 | - | Material 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/Materials/TribulationResistingPill.png` |

## Hand Generated Item

| 素材 | 名称 | 类型 | 尺寸 | 帧 | 描述 | 路径 |
| --- | --- | --- | --- | --- | --- | --- |
| <img src="../Content/Items/HandGenerated/AbyssalStarWombLamp.png" alt="Abyssal Star Womb Lamp" width="48"> | Abyssal Star Womb Lamp | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/AbyssalStarWombLamp.png` |
| <img src="../Content/Items/HandGenerated/AbyssDust.png" alt="Abyss Dust" width="48"> | Abyss Dust | `item icon` | 32x32 | - | 渊尘：星渊裂隙粉尘，暗蓝紫色星点颗粒。 | `Content/Items/HandGenerated/AbyssDust.png` |
| <img src="../Content/Items/HandGenerated/ArchivedImmortalSoulContract.png" alt="Archived Immortal Soul Contract" width="48"> | Archived Immortal Soul Contract | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/ArchivedImmortalSoulContract.png` |
| <img src="../Content/Items/HandGenerated/ArchiveRemnantLight.png" alt="Archive Remnant Light" width="48"> | Archive Remnant Light | `item icon` | 32x32 | - | 归档残光：归档仙魂残留的环形光点。 | `Content/Items/HandGenerated/ArchiveRemnantLight.png` |
| <img src="../Content/Items/HandGenerated/BlackFurnaceIronGolemPet.png" alt="Black Furnace Iron Golem Pet" width="48"> | Black Furnace Iron Golem Pet | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/BlackFurnaceIronGolemPet.png` |
| <img src="../Content/Items/HandGenerated/BlankSectScroll.png" alt="Blank Sect Scroll" width="48"> | Blank Sect Scroll | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/BlankSectScroll.png` |
| <img src="../Content/Items/HandGenerated/BrokenDecreeItem.png" alt="Broken Decree Item" width="48"> | Broken Decree Item | `item icon` | 32x32 | - | 破损法旨：残天法旨碎片，旧金边与墨灰封印。 | `Content/Items/HandGenerated/BrokenDecreeItem.png` |
| <img src="../Content/Items/HandGenerated/BrokenHeavenInscriptionNeedle.png" alt="Broken Heaven Inscription Needle" width="48"> | Broken Heaven Inscription Needle | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/BrokenHeavenInscriptionNeedle.png` |
| <img src="../Content/Items/HandGenerated/BrokenHeavenJade.png" alt="Broken Heaven Jade" width="48"> | Broken Heaven Jade | `item icon` | 32x32 | - | 残天玉：坠天宫阙玉片，白玉裂缝与残金纹。 | `Content/Items/HandGenerated/BrokenHeavenJade.png` |
| <img src="../Content/Items/HandGenerated/BrokenSwordIntent.png" alt="Broken Sword Intent" width="48"> | Broken Sword Intent | `item icon` | 32x32 | - | 断剑残意：剑气碎片，银白裂刃与冷色尾光。 | `Content/Items/HandGenerated/BrokenSwordIntent.png` |
| <img src="../Content/Items/HandGenerated/CelestialPuppetToken.png" alt="Celestial Puppet Token" width="48"> | Celestial Puppet Token | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/CelestialPuppetToken.png` |
| <img src="../Content/Items/HandGenerated/CinnabarPowder.png" alt="Cinnabar Powder" width="48"> | Cinnabar Powder | `item icon` | 32x32 | - | 朱砂粉：符箓与丹药材料，使用朱砂红粉末轮廓。 | `Content/Items/HandGenerated/CinnabarPowder.png` |
| <img src="../Content/Items/HandGenerated/ColdMoonDust.png" alt="Cold Moon Dust" width="48"> | Cold Moon Dust | `item icon` | 32x32 | - | 冷月尘：月骨天渊粉尘，月白骨粉与冷蓝微光。 | `Content/Items/HandGenerated/ColdMoonDust.png` |
| <img src="../Content/Items/HandGenerated/DarkBlueSpiritFluid.png" alt="Dark Blue Spirit Fluid" width="48"> | Dark Blue Spirit Fluid | `item icon` | 32x32 | - | 暗蓝灵液：星渊流体材料，深蓝瓶滴与冷白高光。 | `Content/Items/HandGenerated/DarkBlueSpiritFluid.png` |
| <img src="../Content/Items/HandGenerated/EndgameRouteFrame.png" alt="Endgame Route Frame" width="48"> | Endgame Route Frame | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/EndgameRouteFrame.png` |
| <img src="../Content/Items/HandGenerated/FormlessSwordSoulCostume.png" alt="Formless Sword Soul Costume" width="48"> | Formless Sword Soul Costume | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/FormlessSwordSoulCostume.png` |
| <img src="../Content/Items/HandGenerated/FurnaceAshSpiritContract.png" alt="Furnace Ash Spirit Contract" width="48"> | Furnace Ash Spirit Contract | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/FurnaceAshSpiritContract.png` |
| <img src="../Content/Items/HandGenerated/FurnaceCharcoal.png" alt="Furnace Charcoal" width="48"> | Furnace Charcoal | `item icon` | 32x32 | - | 炉炭：沉炉矿脉余烬材料，黑灰炭块带暗红火点。 | `Content/Items/HandGenerated/FurnaceCharcoal.png` |
| <img src="../Content/Items/HandGenerated/FurnaceInscriptionNeedle.png" alt="Furnace Inscription Needle" width="48"> | Furnace Inscription Needle | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/FurnaceInscriptionNeedle.png` |
| <img src="../Content/Items/HandGenerated/GardenWardenMask.png" alt="Garden Warden Mask" width="48"> | Garden Warden Mask | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/GardenWardenMask.png` |
| <img src="../Content/Items/HandGenerated/GreenwoodInscriptionNeedle.png" alt="Greenwood Inscription Needle" width="48"> | Greenwood Inscription Needle | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/GreenwoodInscriptionNeedle.png` |
| <img src="../Content/Items/HandGenerated/HeavenDaoRouteHint.png" alt="Heaven Dao Route Hint" width="48"> | Heaven Dao Route Hint | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/HeavenDaoRouteHint.png` |
| <img src="../Content/Items/HandGenerated/HerbDew.png" alt="Herb Dew" width="48"> | Herb Dew | `item icon` | 32x32 | - | 药露：药园生机凝露，青绿水滴与草木高光。 | `Content/Items/HandGenerated/HerbDew.png` |
| <img src="../Content/Items/HandGenerated/InscriptionRemovalStone.png" alt="Inscription Removal Stone" width="48"> | Inscription Removal Stone | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/InscriptionRemovalStone.png` |
| <img src="../Content/Items/HandGenerated/InspectorMask.png" alt="Inspector Mask" width="48"> | Inspector Mask | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/InspectorMask.png` |
| <img src="../Content/Items/HandGenerated/LightningAvoidanceRune.png" alt="Lightning Avoidance Rune" width="48"> | Lightning Avoidance Rune | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/LightningAvoidanceRune.png` |
| <img src="../Content/Items/HandGenerated/LowGradeSpiritCore.png" alt="Low Grade Spirit Core" width="48"> | Low Grade Spirit Core | `item icon` | 32x32 | - | 低阶灵核：灵脉早期核心，青玉内光。 | `Content/Items/HandGenerated/LowGradeSpiritCore.png` |
| <img src="../Content/Items/HandGenerated/MedicineKingCauldronDecoration.png" alt="Medicine King Cauldron Decoration" width="48"> | Medicine King Cauldron Decoration | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/MedicineKingCauldronDecoration.png` |
| <img src="../Content/Items/HandGenerated/MoonboneImmortalWingAccessory.png" alt="Moonbone Immortal Wing Accessory" width="48"> | Moonbone Immortal Wing Accessory | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/MoonboneImmortalWingAccessory.png` |
| <img src="../Content/Items/HandGenerated/NascentSoulCloneTalisman.png" alt="Nascent Soul Clone Talisman" width="48"> | Nascent Soul Clone Talisman | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/NascentSoulCloneTalisman.png` |
| <img src="../Content/Items/HandGenerated/SectTrialHint.png" alt="Sect Trial Hint" width="48"> | Sect Trial Hint | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/SectTrialHint.png` |
| <img src="../Content/Items/HandGenerated/ShatteredJadeShell.png" alt="Shattered Jade Shell" width="48"> | Shattered Jade Shell | `item icon` | 32x32 | - | 碎玉虫掉落物：玉壳断裂、深绿外轮廓，作为浅层灵脉材料。 | `Content/Items/HandGenerated/ShatteredJadeShell.png` |
| <img src="../Content/Items/HandGenerated/SilentTabletDecoration.png" alt="Silent Tablet Decoration" width="48"> | Silent Tablet Decoration | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/SilentTabletDecoration.png` |
| <img src="../Content/Items/HandGenerated/SingingThunderStoneItem.png" alt="Singing Thunder Stone Item" width="48"> | Singing Thunder Stone Item | `item icon` | 32x32 | - | 鸣雷石物品形态：小雷石与电弧，呼应雷泽云层生态物件。 | `Content/Items/HandGenerated/SingingThunderStoneItem.png` |
| <img src="../Content/Items/HandGenerated/SmallArtifactPendant.png" alt="Small Artifact Pendant" width="48"> | Small Artifact Pendant | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/SmallArtifactPendant.png` |
| <img src="../Content/Items/HandGenerated/SmallTabletPet.png" alt="Small Tablet Pet" width="48"> | Small Tablet Pet | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/SmallTabletPet.png` |
| <img src="../Content/Items/HandGenerated/SpiritHerbSeeds.png" alt="Spirit Herb Seeds" width="48"> | Spirit Herb Seeds | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/SpiritHerbSeeds.png` |
| <img src="../Content/Items/HandGenerated/SpiritVeinScale.png" alt="Spirit Vein Scale" width="48"> | Spirit Vein Scale | `item icon` | 32x32 | - | 灵脉鳞：灵脉蠕虫鳞片，青玉鳞纹。 | `Content/Items/HandGenerated/SpiritVeinScale.png` |
| <img src="../Content/Items/HandGenerated/SpiritVeinWyrmTrophy.png" alt="Spirit Vein Wyrm Trophy" width="48"> | Spirit Vein Wyrm Trophy | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/SpiritVeinWyrmTrophy.png` |
| <img src="../Content/Items/HandGenerated/StarAbyssInscriptionNeedle.png" alt="Star Abyss Inscription Needle" width="48"> | Star Abyss Inscription Needle | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/StarAbyssInscriptionNeedle.png` |
| <img src="../Content/Items/HandGenerated/StarAbyssLarvaContract.png" alt="Star Abyss Larva Contract" width="48"> | Star Abyss Larva Contract | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/StarAbyssLarvaContract.png` |
| <img src="../Content/Items/HandGenerated/ThunderInscriptionNeedle.png" alt="Thunder Inscription Needle" width="48"> | Thunder Inscription Needle | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/ThunderInscriptionNeedle.png` |
| <img src="../Content/Items/HandGenerated/ThunderMarshJiaoWing.png" alt="Thunder Marsh Jiao Wing" width="48"> | Thunder Marsh Jiao Wing | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/ThunderMarshJiaoWing.png` |
| <img src="../Content/Items/HandGenerated/ThunderPatternFeather.png" alt="Thunder Pattern Feather" width="48"> | Thunder Pattern Feather | `item icon` | 32x32 | - | 雷纹羽：雷纹鹰羽毛，蓝紫雷纹沿羽轴延伸。 | `Content/Items/HandGenerated/ThunderPatternFeather.png` |
| <img src="../Content/Items/HandGenerated/TornScrollPage.png" alt="Torn Scroll Page" width="48"> | Torn Scroll Page | `item icon` | 32x32 | - | 残卷页：旧宗门书页，无可读文字，墨色边缘。 | `Content/Items/HandGenerated/TornScrollPage.png` |
| <img src="../Content/Items/HandGenerated/TornTalismanPaper.png" alt="Torn Talisman Paper" width="48"> | Torn Talisman Paper | `item icon` | 32x32 | - | 符纸蝠掉落物：旧符纸碎片与朱砂痕，无可读文字。 | `Content/Items/HandGenerated/TornTalismanPaper.png` |
| <img src="../Content/Items/HandGenerated/TribulationCloudBottle.png" alt="Tribulation Cloud Bottle" width="48"> | Tribulation Cloud Bottle | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/TribulationCloudBottle.png` |
| <img src="../Content/Items/HandGenerated/TribulationTrainingToken.png" alt="Tribulation Training Token" width="48"> | Tribulation Training Token | `item icon` | 32x32 | - | Hand Generated Item 资源：透明背景、强轮廓、有限色板，遵循 ART_ASSET_GENERATION_PLAN.md。 | `Content/Items/HandGenerated/TribulationTrainingToken.png` |
