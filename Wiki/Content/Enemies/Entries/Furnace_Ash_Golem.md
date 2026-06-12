# 炉灰傀

[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`furnace_ash_golem`
- 中文名：炉灰傀
- 阶段：Pre-Hardmode
- 生态：[沉炉矿脉](../../Biomes/Entries/Sunken_Furnace_Vein.md)

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 沉炉矿脉 | 18% | 0.55 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 180 |
| 伤害 | 28 |
| 防御 | 14 |
| 击退抗性 | 70% |
| AI 类型 | Fighter heavy |
| Banner 击杀数 | 50 |

## 行为

- 高防近战型敌怪。
- 静止时防御提升至22（`NPC.velocity.LengthSquared() < 0.1f`）。
- 命中玩家时施加狱炎3秒（`BuffID.OnFire3, 180`）。
- 受击时喷出6个`DustID.Torch`火星粒子。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 炉渣铁 | 50% | 1-2 |
| 器胚碎片 | 33% | 1-2 |

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/furnace_ash_golem/furnace_ash_golem__base__v01.png" alt="炉灰傀 base" width="72"> | 炉灰傀 | `furnace_ash_golem` | `base` | 64x64 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：64x64，灰黑小傀儡，胸口暗红煤火。
- 动画：`walk` 4帧，`punch` 4帧。
- Prompt：`small ash furnace golem, ember chest, black iron outline`
## 代码实现

- ✅ 数值与wiki完全对齐
- ✅ 独特AI行为
- ✅ 双重掉落表

