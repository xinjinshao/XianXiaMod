

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/celestial_puppet/celestial_puppet__base__v01.png" alt="仙傀 base" width="72"> | 仙傀 | `celestial_puppet` | `base` | 64x80 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：72x80，白玉傀儡，金线关节，无脸。
- 动画：`walk` 4帧，`attack` 5帧。
- Prompt：`white jade celestial puppet, golden joints, faceless divine automaton`

# 仙傀

[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`celestial_puppet`
- 中文名：仙傀
- 阶段：Post-Golem
- 生态：[坠天宫阙](../../Biomes/Entries/Fallen_Heaven_Palace.md)

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 坠天宫阙 | 18% | 0.50 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 1350 |
| 伤害 | 88 |
| 防御 | 46 |
| 击退抗性 | 80% |
| AI 类型 | Modular fighter |
| Banner 击杀数 | 50 |

## 行为

- 模块化攻击型敌怪，每130 tick三阶段轮换。
- Phase 0：横扫冲刺（移速7）。
- Phase 1：跳跃后发射`BossSpiritBoltProjectile`。
- Phase 2：高速冲刺（移速10）。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 天道碎片 | 50% | 1-2 |
| 天道碎片 | 33% | 1-2 |

## 代码实现

- ✅ 数值与wiki完全对齐
- ✅ 独特AI行为
- ✅ 双重掉落表
