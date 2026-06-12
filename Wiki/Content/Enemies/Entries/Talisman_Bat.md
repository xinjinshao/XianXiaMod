# 符纸蝠

[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`talisman_bat`
- 中文名：符纸蝠
- 阶段：Pre-Boss
- 生态：[浅层灵脉](../../Biomes/Entries/Shallow_Spirit_Veins.md)
- 角色：符箓制作入口，教玩家注意空中投射物威胁。

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 浅层灵脉 | 18% | 0.35 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 38 |
| 伤害 | 13 |
| 防御 | 2 |
| 击退抗性 | 10% |
| AI 类型 | Bat fly + projectile |
| Banner 击杀数 | 50 |

## 行为

- 蝙蝠AI飞行（`NPCAIStyleID.Bat`，`AIType = NPCID.CaveBat`）。
- 每120 tick有15%概率（`Main.rand.NextFloat() < 0.15f`）朝玩家发射`SpiritBoltProjectile`（速度5，伤害为NPC伤害的1/3）。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 灵气凝胶 | 50% | 1-2 |

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/talisman_bat/talisman_bat__base__v01.png" alt="符纸蝠 base" width="72"> | 符纸蝠 | `talisman_bat` | `base` | 48x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：48x32，蝙蝠身体像折纸符箓，朱砂眼点。
- 动画：`fly` 6帧。
- Prompt：`paper talisman bat, cinnabar markings, crisp pixel wings, no text`

## 代码实现

- ✅ 数值与wiki对齐（生命38/伤害13/防御2）
- ✅ 低概率符火投射物（15%每120tick）
- ✅ 掉落表（灵气凝胶50%）
