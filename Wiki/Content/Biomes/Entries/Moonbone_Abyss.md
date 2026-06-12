# 月骸天渊

[返回生态总览](../Overview.md)

## 概念

月骸天渊是 Post-Moon Lord 的终局生态。它由月亮残骸、星渊污染和旧天道归档空间共同组成。

## 生成

- 阶段：Post-Moon Lord。
- 位置：通过坠天宫阙终端进入的特殊区域，或世界高空/深空入口。
- 结构：月白骨质平台、暗蓝裂隙、归档光线。
- 规则：敌怪强，资源稀有，适合终局装备制作。

## 内容

- 敌怪：月骸修士、归档仙魂。
- Boss：[月骸仙君](../../Bosses/Entries/Moonbone_Immortal.md)、[旧天道核心](../../Bosses/Entries/Old_Heaven_Dao_Core.md)。
- 资源：月骸骨、星灾灵核、斩道尘。

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/moonbone_tile/moonbone_tile__tile__v01.png" alt="月骸骨岩 tile" width="64"> | 月骸骨岩 | `moonbone_tile` | `tile` | 16x16 |
| <img src="../../../../Assets/Final/archive_light_pillar/archive_light_pillar__object__v01.png" alt="归档光柱 object" width="64"> | 归档光柱 | `archive_light_pillar` | `object` | 32x96 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- Tile：`moonbone_tile`，16x16，月白骨质岩、冷蓝裂光。
- 装饰：`archive_light_pillar`，32x96，竖直归档光，不遮挡角色。
- 背景墙：`moon_abyss_wall`，16x16，深空底和骨质纹。
- Prompt 重点：`moon bone abyss terrain, white lunar bone, dark star cracks, crisp Terraria pixel art`。

## 代码实现

- ✅ 世界生成骨架（Tile铺设+物件放置）
- ✅ 敌怪生成池（权重与wiki对齐）
- ✅ 环境效果（星渊污染/雷泽落雷/月骨灵压等）
- ✅ 判定阈值（tile count ≥ wiki指定值）
