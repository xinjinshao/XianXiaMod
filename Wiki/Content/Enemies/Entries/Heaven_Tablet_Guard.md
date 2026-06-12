# 天碑卫

[返回敌怪总览](../Overview.md)

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/heaven_tablet_guard/heaven_tablet_guard__base__v01.png" alt="天碑卫 base" width="72"> | 天碑卫 | `heaven_tablet_guard` | `base` | 64x80 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：64x80，碑甲卫士，盾像小天碑。
- 动画：`guard` 4帧，`blast` 4帧。
- Prompt：`jade tablet shield guardian, golden decree armor, crisp pixel silhouette`

## 定位

- 英文 ID：`heaven_tablet_guard`
- 中文名：天碑卫
- 阶段：Post-Golem
- 生态：[坠天宫阙](../../Biomes/Entries/Fallen_Heaven_Palace.md)

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 坠天宫阙 | 18% | 0.45 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 1500 |
| 伤害 | 92 |
| 防御 | 54 |
| 击退抗性 | 85% |
| AI 类型 | Shield walker |
| Banner 击杀数 | 50 |

## 行为

- 举盾推进型敌怪。
- 静止时防御62。每160 tick举盾推进180 tick（防御82移速3，每45 tick释放`BossSpiritBoltProjectile`碑文弹）。
- 接近玩家<48px击退并结束推进。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 天道碎片 | 50% | 1-2 |
| 器胚碎片 | 25% | 1-2 |

## 代码实现

- ? 数值与wiki完全对齐
- ? 独特AI行为
- ? 双重掉落表
