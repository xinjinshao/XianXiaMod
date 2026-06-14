

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/obsessed_sword_cultivator/obsessed_sword_cultivator__base__v01.png" alt="执念剑修 base" width="72"> | 执念剑修 | `obsessed_sword_cultivator` | `base` | 64x80 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：72x88，残影剑修，破旧道袍，手持断剑。
- 动画：`guard` 3帧，`thrust` 5帧。
- Prompt：`ghostly sword cultivator, broken robe, broken sword, pale cyan aura`

# 执念剑修

[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`obsessed_sword_cultivator`
- 中文名：执念剑修
- 阶段：Post-Plantera
- 生态：[万宗遗址](../../Biomes/Entries/Ten_Thousand_Sects_Ruins.md)

## 生成条件

| 生态 | 生成概率 | 权重 |
|------|----------|------|
| 万宗遗址 | 18% | 0.45 |

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 850 |
| 伤害 | 72 |
| 防御 | 34 |
| 击退抗性 | 70% |
| AI 类型 | Guard fighter |
| Banner 击杀数 | 50 |

## 行为

- 格挡反击型敌怪。
- 玩家正面X轴<96px时进入格挡（防御42移速x0.65）。
- 被投射物击中时（`OnHitByProjectile`）设置反击标记。
- 每120 tick：有反击标记时释放反击突刺（移速12伤害x1.3），否则普通冲刺。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 宗门试炼令 | 50% | 1-2 |
| 器胚碎片 | 33% | 1-2 |

## 代码实现

- ✅ 数值与wiki完全对齐
- ✅ 独特AI行为
- ✅ 双重掉落表
