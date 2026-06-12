# 符纸蝠

[返回敌怪总览](../Overview.md)

## 定位
- ID：`talisman_bat`
- 阶段：Pre-Boss
- 生态：洞穴、[浅层灵脉](../../Biomes/Entries/Shallow_Spirit_Veins.md)附近
- 角色：符箓制作入口，教玩家注意空中投射物威胁。

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
- 飞行接近玩家，低概率（15%）发射弱符火投射物。

## 掉落
| 物品 | 概率 | 数量 |
|------|------|------|
| 灵气凝胶 | 50% | 1-2 |

## 美术
- 主体：48x32，蝙蝠身体像折纸符箓，朱砂眼点。
- 动画：fly 6帧。
- Prompt：`paper talisman bat, cinnabar markings, crisp pixel wings, no text`

## 代码实现
- ✅ 数值对齐
- ✅ AI行为（符火弹幕）
- ✅ 掉落表
