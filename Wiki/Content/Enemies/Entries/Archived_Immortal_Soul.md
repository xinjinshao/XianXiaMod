# 归档仙魂

[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`archived_immortal_soul`
- 中文名：归档仙魂
- 阶段：Post-Moon Lord
- 生态：[月骸天渊](../../Biomes/Entries/Moonbone_Abyss.md)

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 月骸天渊 | 18% | 0.35 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 3600 |
| 伤害 | 150 |
| 防御 | 64 |
| 击退抗性 | 60% |
| AI 类型 | Copy caster |
| Banner 击杀数 | 50 |

## 行为

- 复制玩家移动型敌怪，呼应旧天道归档主题。
- 记录玩家近20帧位置（`recentPositions[positionIndex % 20]`）。
- 每95 tick沿玩家18帧前历史位置发射`BossSpiritBoltProjectile`延迟幻影弹（速度7）。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 斩道尘 | 50% | 1-2 |
| 月骨 | 25% | 1-2 |

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/archived_immortal_soul/archived_immortal_soul__base__v01.png" alt="归档仙魂 base" width="72"> | 归档仙魂 | `archived_immortal_soul` | `base` | 72x72 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：72x72，半透明仙魂，环形归档线，中心空洞。
- 动画：`float` 6帧，`copy` 4帧。
- Prompt：`archived immortal soul, circular archive lines, hollow glowing core`
## 代码实现

- ✅ 数值与wiki完全对齐
- ✅ 独特AI行为
- ✅ 双重掉落表

