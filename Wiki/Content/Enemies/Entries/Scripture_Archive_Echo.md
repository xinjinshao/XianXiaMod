# 藏经残影

[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`scripture_archive_echo`
- 中文名：藏经残影
- 阶段：Post-Plantera
- 生态：[万宗遗址](../../Biomes/Entries/Ten_Thousand_Sects_Ruins.md)

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 万宗遗址 | 18% | 0.45 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 720 |
| 伤害 | 66 |
| 防御 | 28 |
| 击退抗性 | 50% |
| AI 类型 | Floating caster |
| Banner 击杀数 | 50 |

## 行为

- 悬浮施法型敌怪。
- 每105 tick三连`BossSpiritBoltProjectile`（12度扩散角）。
- 每3次释放获得30 tick护盾（防御72，`DustID.GoldCoin`粒子）。
- HP<50%时基础防御36。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 宗门试炼令 | 50% | 1-2 |
| 宗门试炼令 | 25% | 1-2 |

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/scripture_archive_echo/scripture_archive_echo__base__v01.png" alt="藏经残影 base" width="72"> | 藏经残影 | `scripture_archive_echo` | `base` | 64x64 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：64x64，漂浮书卷和人形残影。
- 动画：`cast` 6帧。
- Prompt：`floating scripture scroll echo, ancient pages, golden faded runes`
## 代码实现

- ✅ 数值与wiki完全对齐
- ✅ 独特AI行为
- ✅ 双重掉落表

