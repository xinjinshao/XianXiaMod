

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/thunder_cloud_tile/thunder_cloud_tile__tile__v05.png" alt="雷云块 tile" width="64"> | 雷云块 | `thunder_cloud_tile` | `tile` | 16x16 |
| <img src="../../../../Assets/Final/singing_thunder_stone/singing_thunder_stone__object__v05.png" alt="鸣雷石 object" width="64"> | 鸣雷石 | `singing_thunder_stone` | `object` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- Tile：`thunder_cloud_tile`，16x16，深紫云块、闪蓝裂纹，边缘像素清楚。
- 装饰：`singing_thunder_stone`，24x32，竖立雷石。
- 背景：远雷云，低对比，不干扰弹幕。
- Prompt 重点：`storm cloud platform tile, purple thunder veins, crisp Terraria pixel art`。

# 雷泽云层

[返回生态总览](../Overview.md)

## 概念

雷泽云层是 Hardmode 后出现在高空的雷系生态。它提供抗劫材料和空战 Boss。

## 生成

- 阶段：困难模式解锁对应敌怪与Boss；雷云地形及浮台随新世界生成。
- 位置：空岛附近或高空云层区域。
- 当前结构：雷云矿斑及最多三座51格宽平整浮台，每座三层雷云地基、10格高净空，优先放置一座鸣雷石。浮空水洼仍待实现。
- 环境：沿用现有周期性落雷。雷暴刷怪率/强度联动仍是后续设计。

## 内容

- 敌怪：雷纹鹰、劫云灵。
- Boss：[雷泽蛟](../../Bosses/Entries/Thunder_Marsh_Jiao.md)。
- 资源：雷纹羽、劫云露、鸣雷石。
- 机制：[天劫](../../../Systems/Tribulation.md)。

## 代码实现

- ✅ 世界生成骨架（Tile铺设+物件放置）
- ✅ 敌怪生成池（权重与wiki对齐）
- ✅ 环境效果（星渊污染/雷泽落雷/月骨灵压等）
- ✅ 判定阈值（tile count ≥ wiki指定值）

## 雷云浮台

第294轮起，在已有雷云附近寻找安全位置，避开原版空岛、宝箱、墙、液体和机关；不安全时跳过，并保留鸣雷石的原有放置回退。浮台使用现有雷云素材，不保证从地面无需装备即可抵达。旧世界不增建，读取不会修复拆除的平台或鸣雷石。

小世界种子20261022的三座浮台及完整鸣雷石已通过保存重载检查，单座浮台足以满足100块的雷泽生态门槛。实际飞行抵达、战斗平衡、更多种子和多人仍待验收。
