

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/moonbone_cultivator/moonbone_cultivator__base__v01.png" alt="月骸修士 base" width="72"> | 月骸修士 | `moonbone_cultivator` | `base` | 72x80 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：72x88，月白骨甲，残月披肩。
- 动画：`dash` 4帧，`cast` 5帧。
- Prompt：`moonbone armored cultivator, white lunar bone armor, cold blue cracks`

# 月骸修士

[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`moonbone_cultivator`
- 中文名：月骸修士
- 阶段：Post-Moon Lord
- 生态：[月骸天渊](../../Biomes/Entries/Moonbone_Abyss.md)

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 月骸天渊 | 18% | 0.45 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 4200 |
| 伤害 | 160 |
| 防御 | 72 |
| 击退抗性 | 75% |
| AI 类型 | Dash caster |
| Banner 击杀数 | 50 |

## 行为

- 高速冲刺+剑气型敌怪。
- 每70 tick冲刺（移速12）+向预测位置发射`BossSpiritBoltProjectile`月骨剑气（速度9）。
- 持续冷月光照（`Lighting.AddLight`）。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 月骨 | 50% | 1-2 |
| 斩道尘 | 25% | 1-2 |

## 代码实现

- ✅ 数值与wiki完全对齐
- ✅ 独特AI行为
- ✅ 双重掉落表
