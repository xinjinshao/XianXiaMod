

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/herb_garden_vine_spirit/herb_garden_vine_spirit__base__v01.png" alt="药园藤妖 base" width="72"> | 药园藤妖 | `herb_garden_vine_spirit` | `base` | 64x64 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：64x64，藤蔓人形，叶冠，根须腿。
- 动画：`idle` 4帧，`whip` 5帧。
- Prompt：`vine spirit enemy, herbal garden roots, leafy head, Terraria pixel art`

# 药园藤妖

[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`herb_garden_vine_spirit`
- 中文名：药园藤妖
- 阶段：Pre-Hardmode
- 生态：[青木药园](../../Biomes/Entries/Greenwood_Herb_Garden.md)
- 角色：青木药园核心威胁，炼丹材料的可刷来源。

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 青木药园 | 18% | 0.55 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 140 |
| 伤害 | 24 |
| 防御 | 8 |
| 击退抗性 | 60% |
| AI 类型 | Semi-stationary plant |
| Banner 击杀数 | 50 |

## 行为

- 半固定炮台型敌怪。
- 玩家进入160px范围时减速缠绕（`NPC.velocity *= 0.92f`）。
- 每90 tick自愈4点HP，释放6个`DustID.Grass`草叶粒子。
- 中距离（160-480px）每130 tick发射`SpiritBoltProjectile`藤鞭投射物（速度6）。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 青木根 | 50% | 1-2 |
| 器胚碎片 | 25% | 1-2 |

## 代码实现

- ✅ 数值与wiki对齐
- ✅ 自愈+藤鞭AI
- ✅ 双重掉落表
