# 碎玉�?


[返回敌怪总览](../Overview.md)

## 定位

- 英文 ID：`shattered_jade_worm`
- 中文名：碎玉�?- 阶段：Pre-Boss
- 生态：[浅层灵脉](../../Biomes/Entries/Shallow_Spirit_Veins.md)
- 角色：提供早期饰品材料，教玩家注意地面威胁�?
## 生成条件

| 生�?| 生成概率 | 权重 |
|------|----------|------|
| 浅层灵脉 | 20% | 0.45 |

## 数�?
| 属�?| �?|
|------|-----|
| 生命 | 60 |
| 伤害 | 14 |
| 防御 | 4 |
| 击退抗�?| 40% |
| AI 类型 | Ground crawler + burrow dash |
| Banner 击杀�?| 50 |

## 行为

- 自定义AI（`NPC.aiStyle = -1`）�?- �?20 tick贴地爬行接近玩家（移�?.5，`NPC.rotation`随速度变化）�?- 之后短距离钻地冲刺（移�?，向上偏�?），释放4个`DustID.Stone`碎石粒子�?
## 掉落

| 物品 | 概率 | 数量 |
|------|------|------|
| 下品灵石 | 50% | 1-2 |
| 灵气凝胶 | 33% | 1-2 |

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/shattered_jade_worm/shattered_jade_worm__base__v01.png" alt="碎玉�?base" width="72"> | 碎玉�?| `shattered_jade_worm` | `base` | 48x24 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体�?8x24，虫形，玉壳断裂，深绿外轮廓�?- 动画：`crawl` 6帧�?- Prompt：`small jade-shelled cave worm, cracked crystal carapace, side-view Terraria pixel art`

## 代码实现

- �?数值与wiki对齐（生�?0/伤害14/防御4�?- �?自定义钻地冲刺AI
- �?掉落表（下品灵石50%+灵气凝胶33%�?
