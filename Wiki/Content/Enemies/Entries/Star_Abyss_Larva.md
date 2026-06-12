# 星渊幼体

[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`star_abyss_larva`
- 中文名：星渊幼体
- 阶段：Hardmode
- 生态：[星渊裂隙](../../Biomes/Entries/Star_Abyss_Rift.md)

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 星渊裂隙 | 18% | 0.70 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 260 |
| 伤害 | 46 |
| 防御 | 18 |
| 击退抗性 | 50% |
| AI 类型 | Leaping crawler |
| Banner 击杀数 | 50 |

## 行为

- 贴地爬行+跃扑型敌怪。
- 每90 tick，当与玩家距离<260px时跃扑（移速8，向上偏转4）。
- 命中后90 tick附着状态：与玩家距离<40px时减速玩家（`target.velocity *= 0.6f`）。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 星蚀晶 | 50% | 1-2 |
| 器胚碎片 | 33% | 1-2 |

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/star_abyss_larva/star_abyss_larva__base__v01.png" alt="星渊幼体 base" width="72"> | 星渊幼体 | `star_abyss_larva` | `base` | 48x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：48x32，深蓝寄生幼体，星点眼。
- 动画：`crawl` 6帧，`leap` 3帧。
- Prompt：`dark blue star abyss larva, parasitic shape, bright tiny star eyes`
## 代码实现

- ✅ 数值与wiki完全对齐
- ✅ 独特AI行为
- ✅ 双重掉落表

