# 星蚀修士

[返回敌怪总览](../Overview.md)

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/star_eclipsed_cultivator/star_eclipsed_cultivator__base__v01.png" alt="星蚀修士 base" width="72"> | 星蚀修士 | `star_eclipsed_cultivator` | `base` | 64x64 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：64x64，人形修士，暗蓝斗篷，星晶侵蚀半身。
- 动画：`cast` 5帧。
- Prompt：`star-infected cultivator, dark blue robe, crystal corruption`

## 定位

- 英文 ID：`star_eclipsed_cultivator`
- 中文名：星蚀修士
- 阶段：Hardmode
- 生态：[星渊裂隙](../../Biomes/Entries/Star_Abyss_Rift.md)

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 星渊裂隙 | 18% | 0.45 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 360 |
| 伤害 | 50 |
| 防御 | 20 |
| 击退抗性 | 45% |
| AI 类型 | Ranged humanoid |
| Banner 击杀数 | 50 |

## 行为

- 远程人形敌怪，保持距离作战。
- 与玩家距离<240px时后退。
- 每135 tick发射`BossSpiritBoltProjectile`（速度7.5）。
- HP低于40%时，每180 tick短闪避（移速+6向远离玩家方向）。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 星蚀晶 | 50% | 1-2 |
| 器胚碎片 | 25% | 1-2 |

## 代码实现

- ? 数值与wiki完全对齐
- ? 独特AI行为
- ? 双重掉落表
