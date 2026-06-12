# 花瘴蝶

[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`miasma_flower_moth`
- 中文名：花瘴蝶
- 阶段：Pre-Hardmode
- 生态：[青木药园](../../Biomes/Entries/Greenwood_Herb_Garden.md)
- 角色：教玩家不要站桩输出的教学型敌怪。

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 青木药园 | 18% | 0.50 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 90 |
| 伤害 | 20 |
| 防御 | 4 |
| 击退抗性 | 20% |
| AI 类型 | Hover caster |
| Banner 击杀数 | 50 |

## 行为

- 慢速飞行（`NPC.velocity *= 0.985f`）。
- 每45 tick对128px内所有在线玩家施加中毒（`BuffID.Poisoned`，持续90 tick）。
- 释放10个`DustID.Poisoned`环形瘴毒粒子从中心向外扩散。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 青木根 | 50% | 1-2 |
| 器胚碎片 | 25% | 1-2 |

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/miasma_flower_moth/miasma_flower_moth__base__v01.png" alt="花瘴蝶 base" width="72"> | 花瘴蝶 | `miasma_flower_moth` | `base` | 48x48 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：48x48，蝶翼带花纹，药绿和淡紫色板。
- 动画：`fly` 6帧，`miasma` 3帧。
- Prompt：`flower moth with herbal miasma, green and pale purple wings, clean pixel outline`
## 代码实现

- ✅ 数值与wiki对齐
- ✅ 瘴毒环AI
- ✅ 双重掉落表

