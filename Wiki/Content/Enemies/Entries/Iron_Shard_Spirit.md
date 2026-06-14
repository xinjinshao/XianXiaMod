

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/iron_shard_spirit/iron_shard_spirit__base__v01.png" alt="铁屑灵 base" width="72"> | 铁屑灵 | `iron_shard_spirit` | `base` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：36x36，漂浮铁片和小火光。
- 动画：`spin` 4帧。
- Prompt：`floating iron shard spirit, tiny ember core`

# 铁屑灵

[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`iron_shard_spirit`
- 中文名：铁屑灵
- 阶段：Pre-Hardmode
- 生态：[沉炉矿脉](../../Biomes/Entries/Sunken_Furnace_Vein.md)

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 沉炉矿脉 | 18% | 0.75 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 70 |
| 伤害 | 22 |
| 防御 | 6 |
| 击退抗性 | 30% |
| AI 类型 | Flying swarm |
| Banner 击杀数 | 50 |

## 行为

- 小型飞行敌怪，通常成群出现。
- 每75 tick朝目标高速冲刺（移速11）。
- 蜂群加成：200px范围内每只同类型敌怪增加25%冲刺速度（`swarmBonus += 0.25f`）。
- 旋转角度跟随速度（`NPC.rotation = NPC.velocity.X * 0.04f`）。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 器胚碎片 | 50% | 1-2 |
| 炉渣铁 | 25% | 1-2 |

## 代码实现

- ✅ 数值与wiki完全对齐
- ✅ 独特AI行为
- ✅ 双重掉落表
