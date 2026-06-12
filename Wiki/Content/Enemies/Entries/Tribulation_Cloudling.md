

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/tribulation_cloudling/tribulation_cloudling__base__v01.png" alt="劫云灵 base" width="72"> | 劫云灵 | `tribulation_cloudling` | `base` | 48x48 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：48x48，紫色小雷云，有玉色面具碎片。
- 动画：`float` 6帧。
- Prompt：`small tribulation cloud spirit, jade mask shard, purple lightning`

# 劫云灵

[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`tribulation_cloudling`
- 中文名：劫云灵
- 阶段：Hardmode
- 生态：[雷泽云层](../../Biomes/Entries/Thunder_Marsh_Clouds.md)

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 雷泽云层 | 18% | 0.45 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 240 |
| 伤害 | 42 |
| 防御 | 16 |
| 击退抗性 | 40% |
| AI 类型 | Teleport caster |
| Banner 击杀数 | 50 |

## 行为

- 闪现施法型敌怪。
- 每150 tick闪现到玩家预测位置（`target.Center + target.velocity * 30f`）上方随机偏移处。
- 在预测位置生成`TribulationWarningLineProjectile`落雷预警线。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 劫云露 | 50% | 1-2 |
| 器胚碎片 | 33% | 1-2 |

## 代码实现

- ✅ 数值与wiki完全对齐
- ✅ 独特AI行为
- ✅ 双重掉落表
