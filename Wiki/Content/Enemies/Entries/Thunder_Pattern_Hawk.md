

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/thunder_pattern_hawk/thunder_pattern_hawk__base__v01.png" alt="雷纹鹰 base" width="72"> | 雷纹鹰 | `thunder_pattern_hawk` | `base` | 64x48 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：64x48，鹰形，羽毛有蓝色雷纹。
- 动画：`fly` 6帧，`dive` 3帧。
- Prompt：`hawk with blue lightning feather patterns, side-view flying`

# 雷纹鹰

[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`thunder_pattern_hawk`
- 中文名：雷纹鹰
- 阶段：Hardmode
- 生态：[雷泽云层](../../Biomes/Entries/Thunder_Marsh_Clouds.md)

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 雷泽云层 | 18% | 0.40 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 300 |
| 伤害 | 48 |
| 防御 | 18 |
| 击退抗性 | 35% |
| AI 类型 | Dive flyer |
| Banner 击杀数 | 50 |

## 行为

- 俯冲型飞行敌怪。
- 每140 tick发起高速俯冲（移速15朝向玩家），30 tick后减速停顿，再次俯冲。
- 高速移动时（`velocity.LengthSquared() > 80f`）留下`DustID.Electric`电弧粒子。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 劫云露 | 50% | 1-2 |
| 器胚碎片 | 33% | 1-2 |

## 代码实现

- ✅ 数值与wiki完全对齐
- ✅ 独特AI行为
- ✅ 双重掉落表
