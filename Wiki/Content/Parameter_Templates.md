# 条目参数模板

[返回首页](../Home.md)

本页定义不同条目类型应包含的规格字段。新增内容时优先按本页模板补齐，再写叙事和玩法说明。

## 武器模板

| 字段 | 说明 |
| --- | --- |
| ID | C# 内部命名 |
| 类型 | 近战、远程、魔法、召唤、灵气、阵法、天道、星渊 |
| 阶段 | Pre-Boss、Pre-Hardmode、Hardmode 等 |
| 伤害 | 基础伤害 |
| 使用时间 | `useTime/useAnimation` 草案 |
| 击退 | Knockback |
| 暴击 | 基础暴击 |
| 消耗 | 灵气、魔力、弹药或生命 |
| 射速 | Projectile velocity |
| 投射物 | 链接到 [投射物详细目录](Equipment/Projectile_Catalog.md) |
| 稀有度 | Terraria rarity |
| 售价 | NPC sell price |
| 研究 | Journey research count |
| 美术 | item icon、projectile、held sprite 尺寸 |

## 饰品模板

| 字段 | 说明 |
| --- | --- |
| ID | C# 内部命名 |
| 阶段 | 解锁阶段 |
| 稀有度 | Terraria rarity |
| 售价 | Sell price |
| 可见 | 是否显示在角色身上 |
| 效果数值 | 明确百分比、固定值、冷却 |
| 副作用 | 星渊/禁术类必须写 |
| 美术 | 32x32 icon，必要时 wearable layer |

## 消耗品模板

| 字段 | 说明 |
| --- | --- |
| ID | C# 内部命名 |
| 类型 | 药剂、符箓、召唤物、钥匙 |
| 使用时间 | useTime |
| 最大堆叠 | maxStack |
| Buff/效果时间 | ticks 或秒 |
| 消耗 | 是否消耗 |
| 条件 | 使用地点、时间、Boss 前置 |
| 稀有度/售价/研究 | 标准 item 字段 |
| 美术 | 16x16、24x24 或 32x32 |

## NPC 模板

| 字段 | 说明 |
| --- | --- |
| ID | C# 内部命名 |
| 生命/防御/击退抗性 | Town NPC stats |
| 入住条件 | 进度、物品、Boss、空房 |
| 商店阶段 | 按 Boss/进度分段 |
| 幸福度 | 偏好生态和邻居 |
| 对话触发 | 首次入住、Boss 后、路线后 |
| 美术 | 主体、头像、walk/talk 帧 |

## 敌怪模板

| 字段 | 说明 |
| --- | --- |
| ID | C# 内部命名 |
| 生态 | 生成区域 |
| 生命/伤害/防御 | NPC stats |
| AI 类型 | Fighter、caster、flyer、crawler 等 |
| 生成权重 | Spawn weight |
| 掉落 | 物品、数量、概率 |
| Banner 击杀数 | 默认 50 |
| 美术 | sprite 尺寸、动画帧、prompt |

## Boss 模板

| 字段 | 说明 |
| --- | --- |
| ID | C# 内部命名 |
| 阶段 | 进度 |
| 召唤 | 召唤物、地点、时间 |
| 生命/伤害/防御 | 普通模式基准 |
| 阶段阈值 | 生命百分比 |
| 攻击 | 投射物、预警、冷却 |
| 掉落 | 保底、常规、稀有、专家/大师 |
| 多人缩放 | 生命倍率 |
| 美术 | 主体、头像、部件、投射物 |

## 生态模板

| 字段 | 说明 |
| --- | --- |
| ID | 内部命名 |
| 阶段 | 解锁阶段 |
| 生成位置 | Worldgen 区域 |
| 区域尺寸 | 宽高范围 |
| 判定 Tile/阈值 | Biome 判定 |
| 地图颜色 | Hex color |
| 敌怪池 | 权重 |
| Tile/Wall | 画布、frame、发光、合并、掉落 |

相关页面：

- [武器与饰品详细目录](Equipment/Equipment_Catalog.md)
- [投射物详细目录](Equipment/Projectile_Catalog.md)
- [Boss 数值与掉落表](Bosses/Boss_Stats.md)
- [生态生成与 Tile 规格](Biomes/Biome_Generation_Stats.md)
- [NPC 统计与商店](NPCs/NPC_Stats_and_Shops.md)
