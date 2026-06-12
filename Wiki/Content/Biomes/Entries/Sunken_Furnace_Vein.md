

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/furnace_slag_tile/furnace_slag_tile__tile__v01.png" alt="炉渣石 tile" width="64"> | 炉渣石 | `furnace_slag_tile` | `tile` | 16x16 |
| <img src="../../../../Assets/Final/black_furnace_wall/black_furnace_wall__wall__v01.png" alt="玄炉墙 wall" width="64"> | 玄炉墙 | `black_furnace_wall` | `wall` | 16x16 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- Tile：`furnace_slag_tile`，16x16，暗灰矿渣、橙红裂光。
- 装饰：`broken_furnace`，48x48，残破炉口和冷却炉心。
- 背景墙：`black_furnace_wall`，16x16，烟熏石砖与金属铆钉。
- Prompt 重点：`sunken forge vein, black iron slag, ember cracks, Terraria cave tile`。

# 沉炉矿脉

[返回生态总览](../Overview.md)

## 概念

沉炉矿脉是玄炉器盟的废弃锻造区。它承担炼器、重器和器灵召唤线的早期入口。

## 生成

- 阶段：Pre-Hardmode。
- 位置：洞穴层和接近熔岩区域。
- 结构：断裂炉台、铁链、矿渣堆、半埋器胚。
- 危险：火星陷阱和高防御敌怪。

## 内容

- 敌怪：炉灰傀、铁屑灵。
- Boss：[玄炉铁傀](../../Bosses/Entries/Black_Furnace_Iron_Golem.md)。
- 资源：炉渣铁、玄炉炭、器胚碎片。
- NPC：[游方器师](../../NPCs/Entries/Wandering_Artificer.md)。

## 代码实现

- ✅ 世界生成骨架（Tile铺设+物件放置）
- ✅ 敌怪生成池（权重与wiki对齐）
- ✅ 环境效果（星渊污染/雷泽落雷/月骨灵压等）
- ✅ 判定阈值（tile count ≥ wiki指定值）
