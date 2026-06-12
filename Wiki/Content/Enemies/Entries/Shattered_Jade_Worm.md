# 碎玉虫

[返回敌怪总览](../Overview.md)

## 定位

- ID：`shattered_jade_worm`
- 阶段：Pre-Boss
- 生态：[浅层灵脉](../../Biomes/Entries/Shallow_Spirit_Veins.md)
- 角色：提供早期饰品材料，教玩家注意地面威胁。

## 数值

| 属性 | 值 |
|------|-----|
| 生命 | 60 |
| 伤害 | 14 |
| 防御 | 4 |
| 击退抗性 | 40% |
| AI 类型 | Ground crawler + burrow dash |
| Banner 击杀数 | 50 |

## 行为

- 贴地爬行，缓慢接近玩家。
- 每120 tick短距离钻地冲刺，释放碎石粒子。

## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 下品灵石 | 50% | 1-2 |
| 灵气凝胶 | 33% | 1-2 |

## 美术

- 主体：48x24，虫形，玉壳断裂，深绿外轮廓。
- 动画：crawl 6帧。
- Prompt：`small jade-shelled cave worm, cracked crystal carapace, side-view Terraria pixel art`

## 代码实现

- ✅ 数值对齐
- ✅ AI行为（钻地冲刺）
- ✅ 掉落表
- ⚠️ wiki稀有以下落未创建独立物品
