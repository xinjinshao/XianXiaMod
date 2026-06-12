# 游灵史莱�?


[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`wandering_spirit_slime`
- 中文名：游灵史莱�?- 阶段：Pre-Boss
- 生态：[浅层灵脉](../../Biomes/Entries/Shallow_Spirit_Veins.md)
- 角色：灵气系统的第一个材料来源，教学型敌怪�?
## 生成条件

| 生�?| 生成概率 | 权重 |
|------|----------|------|
| 浅层灵脉 | 28% | 0.65 |

## 数�?
| 属�?| �?|
|------|-----|
| 生命 | 45 |
| 伤害 | 12 |
| 防御 | 2 |
| 击退抗�?| 20% |
| AI 类型 | Slime hop |
| Banner 击杀�?| 50 |

## 行为

- 慢速跳跃接近玩家，标准史莱姆AI（`NPCAIStyleID.Slime`）�?- 受击时短暂释放灵气粒子（`HitEffect`�?�?`DustID.MagicMirror` 粒子）�?
## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 灵气凝胶 | 100% | 1-3 |
| 下品灵石 | 25% | 1-2 |

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/wandering_spirit_slime/wandering_spirit_slime__base__v01.png" alt="游灵史莱�?base" width="72"> | 游灵史莱�?| `wandering_spirit_slime` | `base` | 48x48 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体�?8x48，圆形青绿色史莱姆，体内漂浮小符核�?- 动画：`idle` 4帧，`hop` 4帧，`hit` 2帧�?- Prompt：`jade green slime with floating talisman core, Terraria-style pixel enemy, transparent background`

## 代码实现

- �?数值与wiki对齐（生�?5/伤害12/防御2�?- �?AI行为（HitEffect灵粒特效�?- �?掉落表（灵气凝胶100%+下品灵石25%�?
