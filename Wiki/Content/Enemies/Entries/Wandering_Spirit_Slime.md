# 游灵史莱姆

[返回敌怪总览](../Overview.md)

## 定位

- ID：`wandering_spirit_slime`
- 阶段：Pre-Boss
- 生态：[浅层灵脉](../../Biomes/Entries/Shallow_Spirit_Veins.md)
- 角色：灵气系统的第一个材料来源，教学型敌怪。

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 45 |
| 伤害 | 12 |
| 防御 | 2 |
| 击退抗性 | 20% |
| AI 类型 | Slime hop |
| Banner 击杀数 | 50 |

## 行为

- 慢速跳跃接近玩家，标准史莱姆AI。
- 受击时短暂释放灵气粒子（青色灵尘）。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 灵气凝胶 | 100% | 1-3 |
| 下品灵石 | 25% | 1-2 |

## 美术

- 主体：48x48，圆形青绿色史莱姆，体内漂浮小符核。
- 动画：idle 4帧，hop 4帧，hit 2帧。
- Prompt：`jade green slime with floating talisman core, Terraria-style pixel enemy, transparent background`

## 代码实现

- ✅ 数值对齐
- ✅ AI行为（受击粒子特效）
- ✅ 掉落表
- ⚠️ 稀有掉落（史莱姆符核 2%）未创建独立物品
