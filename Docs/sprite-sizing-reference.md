# XianXia Mod — Sprite Sizing Reference

> **编制日期:** 2026-06-14  
> **参考来源:** Terraria 原版 Wiki (terraria.wiki.gg)、Terraria 1.4.4.9 更新日志、tModLoader Spriting Wiki、Calamity Mod Wiki (calamitymod.wiki.gg)、Terraria 官方论坛 Spriting Guide

---

## 核心原则

### Terraria 2x 像素缩放
所有 Terraria 精灵图在 PNG 文件中储存为 **2x 分辨率**。即游戏中 1 个逻辑像素 = PNG 中的 2×2 像素块。下文所有尺寸均为 **PNG 文件中的实际像素尺寸（2x）**，与你的 `art_asset_manifest.csv` 中的 `width`/`height` 列含义一致。

### 动画帧布局
- 帧在纹理中**纵向**排列（从上到下）
- 帧之间保留 **2px 间距**（在 2x 分辨率下 = 4px）
- `frameHeight = texture.Height / frameCount`
- 当前项目：Boss 用 6 帧、敌人用 6 帧、城镇 NPC 用 4 帧

### 朝向
- NPC/敌人面朝 **左**
- 弹射物**箭头朝上**
- 武器物品图标默认**指向右上方**（约 45°）

---

## 一、Boss（13 个）

### 参考标准
| Boss（原版/Calamity） | 精灵尺寸（PNG 2x 估算） | 说明 |
|---|---|---|
| King Slime | ~160×112 | 中等 Boss，6 帧动画 |
| Eye of Cthulhu | ~160×128 | 飞行 Boss |
| Eater of Worlds | 多段虫体，每段约 64×64 | 蠕虫型 |
| Skeletron | ~160×192 | 高大型 |
| Queen Bee | ~128×96 | 横向较宽 |
| Plantera | ~192×192 | 大型 Boss |
| Calamity — Desert Scourge | 多段虫体，头约 96×64 | 类似 EoW |
| Calamity — Crabulon | ~128×96 | 横向宽型 |
| Calamity — Supreme Calamitas | ~200×200+ | 终局 Boss |

### 蠕虫型 Boss 的分段设计

> ⚠️ **重要架构决策：** Terraria 蠕虫型 Boss（Eater of Worlds、Destroyer）不是单体精灵——它们由多个独立 NPC 段拼接而成。每段有自己的精灵图、碰撞箱和 AI 行为。头段驱动移动方向，体段跟随前一节，尾段在末端收束。这个架构要求你把 Boss 精灵拆成 **head / body / tail** 三个独立素材。

**蠕虫运动逻辑：**
- **头** (`head`)：指向运动方向，有口器或面部特征。AI 控制头部的转弯、冲刺、穿墙。头段决定整条虫的速度和方向。
- **体** (`body`)：重复段，每段跟随前一节的位置和旋转角。通常 6-20 节（取决于 Boss 长度）。体段有核心发光/纹理特征。
- **尾** (`tail`)：末段，比体段略短/窄，收束虫体轮廓。尾段通常有尖刺、鳍或渐变消失效果。
- 头段受伤判定 → 全虫扣血；体段受伤 → 仅该段扣血（通常比头段减伤）。

**帧布局：** 蠕虫段通常每段用 2 列 × 3 行（6 帧）动画——比标准 Boss 的 6 帧少是因为段与段之间通过旋转来表现曲线，不需要复杂的身体动画。段精灵朝左绘制（与 Terraria NPC 朝向一致），代码中由蠕虫 AI 自动旋转。

### 当前尺寸与建议

| # | Boss | 段类型 | 建议尺寸 (w×h) | 调整理由 | 参考对象 |
|---|---|---|---|---|---|
| 1a | **Spirit Vein Wyrm** | **head** | **64×64** | 圆形玉色头部，口器特征，稍大于体段 | Eater of Worlds 头段 ~64×64 |
| 1b | **Spirit Vein Wyrm** | **body** | **64×64** | 重复段，青玉发光核心，深绿外轮廓，6-8 节 | Eater of Worlds 体段 ~64×64 |
| 1c | **Spirit Vein Wyrm** | **tail** | **64×48** | 尾段略短，锥形收束，青玉光渐隐 | Eater of Worlds 尾段 ~64×48 |
| 2 | **Garden Warden** | body | **128×128** | 植物型 Boss → Plantera 级别，96 偏小 | Plantera ~192×192, Queen Bee ~128×96 |
| 3 | **Black Furnace Iron Golem** | body | **144×144** | 铁傀儡型 → Golem 级别，需更有压迫感 | Golem ~160×160; Calamity Ravager ~160×144 |
| 4 | **Tribulation Cloud Avatar** | body | **160×120** | 云/风暴型 → 增加宽度表现云团广阔感 | Calamity Storm Weaver ~128×80 |
| 5a | **Thunder Marsh Jiao** | **head** | **96×80** | 蛟龙头部，雷角、须、张口，比体段大 | Calamity Aquatic Scourge 头 ~96×72 |
| 5b | **Thunder Marsh Jiao** | **body** | **96×80** | 蛟龙鳞身，紫蓝雷纹，脊刺，8-12 节 | Calamity Aquatic Scourge 体段 ~80×80 |
| 5c | **Thunder Marsh Jiao** | **tail** | **96×64** | 蛟尾，鳍状收束，雷纹渐疏 | Calamity Aquatic Scourge 尾段 ~80×64 |
| 6 | **Abyssal Star Womb** | body | **160×160** | 深渊/星体型，128 偏小 → 提升到终局尺寸感 | Moon Lord 手 ~128×128; 本体更大 |
| 7 | **Formless Sword Soul** | body | **128×128** | 剑魂型 → 提升后与武器尺寸一致 | Terra Blade 弹射物 ~60×60; Calamity 剑魂型投影 ~80×96 |
| 8 | **Greenwood Medicine King Echo** | body | **144×144** | 药王虚影型 → Golem/Plantera 中间级别 | Plantera ~192×192; Golem ~160×160 |
| 9 | **Heaven Tablet Guardian** | body | **128×192** | 石碑守护型 → 竖向 Boss，参照 Skeletron | Skeletron 约 ~160×192 |
| 10 | **Broken Heaven Inspector** | body | **160×160** | 天神审视者 → 大型浮空 Boss | Calamity Providence ~176×176 |
| 11 | **Moonbone Immortal** | body | **200×200** | 当前最大的终局 Boss 之一，微调保持一致 | Calamity Supreme Calamitas ~200×200 |
| 12 | **Old Heaven Dao Core** | body | **208×208** | 天道核心 → 模组最终 Boss，应最大 | Moon Lord 总尺寸约 240×240 |
| 13a | **Shattered Jade Wyrm Minion** | **head** | **32×32** | Boss 召唤的小型蠕虫仆从头 | Servant of Cthulhu ~48×32 |
| 13b | **Shattered Jade Wyrm Minion** | **body** | **32×32** | 幼体段，3-5 节 | 小型蠕虫体段 |
| 13c | **Shattered Jade Wyrm Minion** | **tail** | **32×24** | 幼体尾段，略短收束 | 小型蠕虫尾段 |

### 蠕虫型 Boss 运动描述

| Boss | 穿墙 | 运动模式 | 分裂行为 | 场地要求 |
|---|---|---|---|---|
| **Spirit Vein Wyrm** | ✅ 是（地下穿行） | 正弦波穿行，头部以 45-60° 弧度转弯。每 3-4 秒做一次直线冲刺（冲刺时头部加速 1.5x，体节拉直跟随）。 | HP < 50% 时分裂为 2-3 条小虫，每条小虫保留 head+body×3+tail 结构。 | 地下 80×40，必须封闭空间防止虫体脱出 |
| **Thunder Marsh Jiao** | ❌ 否（空中飞行） | 蛇形盘旋，头部在玩家上方 200-400px 游弋。周期性向玩家俯冲（俯冲角度 30-45°）。体段跟随呈 S 形曲线。 | 断角（HP < 35%）后头部加速 +30%，尾部会甩出雷球。不分裂为多条。 | 露天 160×100，需要平台层级 |
| **Shattered Jade Wyrm Minion** | ✅ 是 | 从 Boss 主体分裂出的小虫，独立穿行。头部追踪玩家，体段跟随。生存时间 15-20 秒后自毁。 | 已分裂产物，不再二次分裂。 | 随 Boss 场地 |

> **关键区别：** Spirit Vein Wyrm 是 **穿墙蠕虫**（如 Eater of Worlds），在地下封闭空间战斗。Thunder Marsh Jiao 是 **空中蛇形龙**（如 Aquatic Scourge），在开阔天空战斗，不穿墙。两种共用 head/body/tail 分段架构，但运动 AI 完全不同。

### Boss Head（地图图标）

| 类型 | 当前尺寸 | 建议尺寸 | 参考 |
|---|---|---|---|
| 标准 Boss Head | 32×32 | **32×32** ✅ | 原版所有 Boss Head 均为 32×32 |
| 大型 Boss Head（Jiao, Immortal, Dao Core） | 48×48 | **40×40** | 略大于标准但不需要 48；Calamity 终局 Boss 头部约 40×40 |

---

## 二、敌怪 / Enemies（14 个）

### 参考标准
| 敌怪（原版） | 精灵尺寸（PNG 2x 估算） | 说明 |
|---|---|---|
| Green Slime | ~32×24 | 最小敌怪 |
| Zombie | ~32×56 | 人形标准 |
| Demon Eye | ~40×36 | 眼球型 |
| Cave Bat | ~32×24 | 飞行小型 |
| Chaos Elemental | ~48×64 | 人形中等 |
| Mimic | ~64×48 | 宝箱怪 |
| Tortoise | ~64×40 | 横向中型 |
| Paladin | ~64×80 | 大型人形 |
| Calamity — Wulfrum Gyrator | ~48×32 | 飞行小型 |
| Calamity — Stormlion | ~64×48 | 中型陆行 |

### 当前尺寸与建议

| # | 敌怪 | 当前尺寸 (w×h) | 建议尺寸 | 调整理由 | 参考对象 |
|---|---|---|---|---|---|
| 1 | **Wandering Spirit Slime** | 48×48 | **40×40** | 史莱姆型 → 比原版最大 Slime 稍大即可 | King Slime 子 Slime ~32×32; 最大普通 Slime ~32×24 |
| 2 | **Shattered Jade Worm** | 48×24 | **54×28** | 虫型 → 略加宽使其细节可见 | 原版 Worm ~48×24; Calamity 小型虫 ~50×24 |
| 3 | **Talisman Bat** | 48×32 | **50×34** | 蝙蝠型 → 微调 | Cave Bat ~32×24; Giant Bat ~48×32 |
| 4 | **Herb Garden Vine Spirit** | 64×64 | **64×64** ✅ | 藤蔓精灵 → 尺寸合适 | Chaos Elemental ~48×64; Angry Trapper ~64×80 |
| 5 | **Miasma Flower Moth** | 48×48 | **56×56** | 飞蛾型 → 略增大 | 原版 Moth ~44×40; Calamity Plague Charger ~56×56 |
| 6 | **Furnace Ash Golem** | 64×64 | **72×72** | 小型傀儡 → 比普通敌人坚固 | 原版 Rock Golem ~72×64; Tortoise ~64×40 |
| 7 | **Iron Shard Spirit** | 32×32 | **36×36** | 小型灵体 → 当前偏小 | 原版 Wisp ~28×28; Dungeon Spirit ~44×36 |
| 8 | **Tribulation Cloudling** | 48×48 | **56×48** | 云灵 → 微调宽度 | Calamity Cloud Elemental 小兵 ~48×40 |
| 9 | **Thunder Pattern Hawk** | 64×48 | **68×52** | 雷纹鹰 → 略增大 | 原版 Harpy ~48×40; Calamity 飞行小兵 ~56×44 |
| 10 | **Star Eclipsed Cultivator** | 64×64 | **64×64** ✅ | 星蚀修士 → 尺寸合适 | Paladin/修士类 ~64×80; 原版 Cultist ~48×64 |
| 11 | **Star Abyss Larva** | 48×32 | **52×36** | 星渊幼虫 → 微调 | 原版 Larva ~36×24; Calamity 幼虫 ~50×32 |
| 12 | **Obsessed Sword Cultivator** | 64×80 | **72×88** | 剑痴修士 → 大型人形，适当增高 | Paladin ~64×80; Calamity 重甲型 ~72×88 |
| 13 | **Scripture Archive Echo** | 64×64 | **64×64** ✅ | 经卷回响 → 尺寸合适 | Rune Wizard ~56×56; 经典型 ~64×64 |
| 14 | **Celestial Puppet** | 64×80 | **72×80** | 天机傀儡 → 人形高度合理 | 原版 Possessed Armor ~56×72 |
| 15 | **Heaven Tablet Guard** | 64×80 | **72×88** | 石碑守卫 → 同上，大型人形 | Paladin ~64×80 |
| 16 | **Moonbone Cultivator** | 72×80 | **72×88** | 月骨修士 → 微调 | 同上 |
| 17 | **Archived Immortal Soul** | 72×72 | **80×80** | 归档仙魂 → 大型灵体，应更显眼 | Dungeon Spirit ~44×36; 但此为修仙模组高级敌怪 |

---

## 三、城镇 NPC（5 个）

### 参考标准
| NPC（原版） | 精灵尺寸（PNG 2x） | 帧数 |
|---|---|---|
| Guide / Merchant 等标准城镇 NPC | Body: **40×56**（20×28 1x）, Head: 32×32 | 20-25 帧 |
| Skeleton Merchant | ~40×56 | 25 帧 |
| 全原版城镇 NPC | Body 统一 **40×56** (±2px), Head 统一 **32×32** | |

> **关键发现：** 原版所有城镇 NPC 的 Body 帧尺寸统一为 **40×56**（PNG 2x）。这是硬约定。

### 当前尺寸与建议

| # | NPC | Body 当前 | Body 建议 | Head 当前 | Head 建议 | 调整理由 |
|---|---|---|---|---|---|---|
| 1 | **Herb Sect Apprentice** | 40×56 | **40×56** ✅ | 32×32 | **32×32** ✅ | 完全符合原版标准 |
| 2 | **Wandering Artificer** | 42×58 | **40×56** ✏️ | 32×32 | **32×32** ✅ | 42×58 略超标准，建议缩到 40×56 与其他 NPC 对齐 |
| 3 | **Tribulation Observer** | 40×56 | **40×56** ✅ | 32×32 | **32×32** ✅ | 符合标准 |
| 4 | **Archive Scroll Spirit** | 36×48 | **40×56** ✏️ | 32×32 | **32×32** ✅ | 36×48 偏小，填充到原版标准 40×56 |
| 5 | **Fallen Heaven Messenger** | 42×60 | **40×56** ✏️ | 32×32 | **32×32** ✅ | 同理，对齐原版标准 |

> **结论：城镇 NPC Body 统一为 40×56，Head 统一为 32×32** —— 这是原版的硬性标准。

---

## 四、武器物品图标

### 参考标准
| 武器（原版） | 1x 尺寸 | PNG 2x 尺寸 | 等级 |
|---|---|---|---|
| Copper Shortsword | ~16×24 | ~32×48 | T0 |
| Iron Broadsword | 36×36 | **72×72** | T1 |
| Light's Bane | ~44×44 | ~88×88 | T2 |
| Muramasa / Night's Edge | 58×58 | **116×116** | T3-T4 |
| Adamantite Sword | 60×60 | **120×120** | T5 |
| Chlorophyte Claymore | 68×68 | **136×136** | T6 |
| Terra Blade | 64×64 | **128×128** | T7 |
| Breaker Blade | 80×92 | **160×184** | 最大原版剑 |
| Starfury | 42×42 | **84×84** | T1.5 |
| Death Sickle | 70×64 | **140×128** | T7 |
| Influx Waver | 64×64 | **128×128** | T8 |

**Calamity 武器参考：**
| 武器 | 约尺寸 (PNG 2x) |
|---|---|
| Aegis Blade | ~72×72 |
| Ark of the Cosmos | ~176×192 |
| Anarchy Blade | ~96×96 |
| 中型 Calamity 剑 | ~80×80 — 120×120 |
| 终局剑 | ~160×160 — 200×200 |

### 当前尺寸与建议

| # | 武器 | 当前 (w×h) | 建议 | 等级 | 调整理由 | 参考 |
|---|---|---|---|---|---|---|
| 1 | **Woodgrain Flying Sword** | 48×48 | **56×56** | T1 | 最基础修仙剑 → 应大于铜剑小于铁剑 | Iron Broadsword 72×72; 折中用 56×56 |
| 2 | **Cloudpiercer Flying Sword** | 64×64 | **72×72** | T2 | 突破云层剑 → 与 Iron Broadsword 同级 | Iron Broadsword ~72×72 |
| 3 | **Thunder Pattern Sword Case** | 64×64 | **80×80** | T3 | 飞剑匣 → 中等级别 | Starfury ~84×84 |
| 4 | **Formless Sword Wheel** | 64×64 | **96×96** | T4 | 无形剑轮 → 高级武器 | Anarchy Blade ~96×96; Night's Edge ~116×116 |
| 5 | **Moonbone Dharma Sword** | 64×64 | **104×104** | T5 | 月骨法剑 → 终局前最强剑 | Adamantite Sword ~120×120 |
| 6 | **Cinnabar Talisman Flame** | 32×32 | **48×48** | T2 | 朱砂符火 → 符箓类武器，不应太小 | 原版 Magic Missile ~36×36; Flower of Fire ~40×44 |
| 7 | **Greenwood Array Plate** | 48×48 | **56×56** | T3 | 青木阵盘 → 阵列型武器 | 原版 Magnet Sphere ~48×48; 稍大到 56 |
| 8 | **Thunder Talisman Array Plate** | 48×48 | **60×60** | T4 | 雷符阵盘 → Greenwood 的升级版 | 同级武器略大 |
| 9 | **Broken Heaven Decree** | 48×48 | **56×56** | T5 | 破天敕令 → 符令型 | 同上 |
| 10 | **Old Heaven Dao Scroll** | 64×64 | **72×72** | T6 | 古天道卷 → 高阶法器 | Terra Blade ~128×128; 卷轴类偏小 |
| 11 | **Spiritwood Crossbow** | 48×48 | **56×44** | T2 | 弩 → 原版弩横向宽 | 原版 Bow 约 20×30 (1x) = 40×60 (2x); Crossbow 更宽 |
| 12 | **Star Eclipse Arbalest** | 64×64 | **72×52** | T5 | 星蚀重弩 → 大型远程 | 原版 Daedalus Stormbow ~48×36 (1x) = 96×72 (2x) |

> ⚠️ **重要提醒：** 武器图标 > ~85×85 时，挥动动画中会脱离玩家手部！如果武器超过此尺寸，请在代码中启用：  
> `ItemID.Sets.UsesBetterMeleeItemLocation[Type] = true;`  
> 或者改用 Held-Projectile 风格。来源：[tModLoader Spriting Wiki](https://github.com/tModLoader/tModLoader/wiki/Spriting)

---

## 五、材料 / 饰品 / 消耗品 / Boss 召唤物

### 参考标准
| 物品类型 | 原版典型尺寸 (PNG 2x) | 例子 |
|---|---|---|
| 矿石 | **16×16** — 24×24 | Copper Ore ~16×16 |
| 锭 / 基础材料 | **24×24** — 32×32 | Iron Bar ~30×44; Soul ~24×24 |
| Boss 召唤物 | **32×32** — 40×40 | Suspicious Looking Eye ~32×32; Abeemination ~36×36 |
| 消耗品（药水/药丸） | **24×24** — 32×32 | Healing Potion ~24×36; Buff Potion ~20×32 |
| 饰品 | **32×32** — 40×40 | Shackle ~28×24; Cross Necklace ~36×36 |
| 中型材料 | **32×32** — 36×36 | 各种魂、碎片 |
| 大型稀有材料 | **40×40** — 48×48 | Terra Core / 终局合成材料 |

### 当前尺寸与建议

| # | 物品 | 当前 | 建议 | 类型 | 调整理由 |
|---|---|---|---|---|---|
| 1 | Low-Grade Spirit Stone | 24×24 | **24×24** ✅ | 基础材料 | 合理 |
| 2 | Spirit Gel | 16×16 | **20×20** | 基础材料 | 16×16 是最小的矿石尺寸，凝胶应略大 |
| 3 | Torn Talisman Paper | 24×24 | **28×24** | 基础材料 | 纸类应略宽 |
| 4 | Greenwood Root | 24×24 | **24×24** ✅ | 基础材料 | 合理 |
| 5 | Furnace Slag Iron | 24×24 | **28×28** | 基础材料 | 炉渣应比一般材料大 |
| 6 | Artifact Blank Shard | 24×24 | **24×24** ✅ | 基础材料 | 合理 |
| 7 | Tribulation Cloud Dew | 24×24 | **24×24** ✅ | 基础材料 | 合理 |
| 8 | Star Eclipse Crystal | 24×24 | **28×28** | 基础材料 | 水晶类应略大 |
| 9 | Sect Trial Token | 32×32 | **32×32** ✅ | 中级材料 | 合理 |
| 10 | Heaven Dao Fragment | 32×32 | **32×32** ✅ | 中级材料 | 合理 |
| 11 | Moonbone | 32×32 | **36×36** | 高级材料 | 月骨 → 终局材料应更大 |
| 12 | Dao Severing Dust | 32×32 | **28×28** | 中级材料 | 粉尘类应略小 |
| 13 | Qi Drawing Talisman | 32×32 | **32×32** ✅ | Boss 召唤物 | 合理 |
| 14 | Spring Return Pill | 16×16 | **22×22** | 消耗品 | 药丸不应只有矿石大小 |
| 15 | Qi Condensing Pill | 24×24 | **24×24** ✅ | 消耗品 | 合理 |
| 16 | Foundation Pill | 24×24 | **24×24** ✅ | 消耗品 | 合理 |
| 17 | Tribulation Resisting Pill | 24×24 | **28×28** | 消耗品 | 重要药丸略大 |
| 18 | Star Abyss Forbidden Talisman | 32×32 | **36×36** | Boss 召唤物 | 终局 Boss 召唤物应更大 |
| 19 | Spirit Vein Incense | 32×32 | **32×32** ✅ | Boss 召唤物 | 合理 |
| 20 | Garden Broken Key | 32×32 | **32×32** ✅ | Boss 召唤物 | 合理 |
| 21 | Old Furnace Ember | 24×24 | **28×28** | Boss 召唤物 | 火种 → 略大 |
| 22 | Thunder Calling Jade | 32×32 | **36×36** | Boss 召唤物 | 玉器 → 略大 |
| 23 | Star Abyss Membrane | 32×32 | **32×32** ✅ | 材料 | 合理 |
| 24 | Heaven Tablet Rubbing | 32×32 | **32×32** ✅ | Boss 召唤物 | 合理 |
| 25 | Moonbone Ritual Talisman | 32×32 | **36×36** | Boss 召唤物 | 月骨仪式符 → 高级 |
| 26 | Qi Gathering Pendant | 32×32 | **36×32** | 饰品 | 项链类 → 略宽 |
| 27 | Spiritwood Charm | 32×32 | **32×32** ✅ | 饰品 | 合理 |
| 28 | Furnace Heart Ring | 32×32 | **32×32** ✅ | 饰品 | 合理 |
| 29 | Lightning Ward Jade | 32×32 | **32×32** ✅ | 饰品 | 合理 |
| 30 | Star Abyss Eye | 32×32 | **36×36** | 饰品 | 星渊之眼 → 稀有饰品 |
| 31 | Nascent Soul Jade Box | 32×32 | **36×32** | 饰品 | 玉盒 → 略宽 |
| 32 | Broken Heaven Crown Seal | 32×32 | **36×36** | 饰品 | 破天冠印 → 大型饰品 |
| 33 | Dao Severing Ring | 32×32 | **32×32** ✅ | 饰品 | 合理 |

---

## 六、弹射物 / Projectiles（18 个）

### 参考标准
| 弹射物类型 | 原版/Calamity 典型尺寸 (PNG 2x) | 例子 |
|---|---|---|
| 子弹 | ~4×40 — 8×40 | Musket Ball ~4×40 |
| 箭 | ~10×20 — 16×32 | Wooden Arrow ~10×20 |
| 小型魔法弹射物 | ~16×16 — 24×24 | Water Bolt ~16×16 |
| 中型魔法弹射物 | ~32×32 — 48×48 | Demon Scythe ~40×40 |
| 剑弹射物（挥动投射物） | ~48×16 — 80×24 | Terra Blade ~76×24 |
| 光束型 | ~16×128 — 32×256 | Last Prism ~30×200 |
| 大型 AoE 场 | ~64×64 — 128×128 | Magnet Sphere ~64×64 |
| 召唤物弹射物 | ~24×24 — 48×48 | 各种各样的 |
| Calamity 大型弹射物 | ~80×80 — 200×200 | 终局 Boss 弹射物 |

> ⚠️ **重要：** 弹射物的 `width`/`height` 在 C# 代码中定义了**碰撞体积**，可以与精灵尺寸不同。命中框通常**小于或等于**精灵尺寸。

### 当前尺寸与建议

| # | 弹射物 | 当前 (w×h) | 建议 | 类型 | 调整理由 |
|---|---|---|---|---|---|
| 1 | **Spirit Bolt** | 16×8 | **20×12** | 小型灵气弹 | 8px 高太薄，需加高 |
| 2 | **Woodgrain Sword Proj** | 32×16 | **40×20** | 剑型投射物 | 基础飞剑 → 参考 Terra Blade ~76×24，缩小 |
| 3 | **Cloudpiercer Sword Proj** | 48×16 | **56×24** | 剑型投射物 | 云穿飞剑 → 中级 |
| 4 | **Cloud Wisp Proj** | 24×16 | **28×20** | 小精灵 | 云灵 → 微调 |
| 5 | **Thunder Sword Proj** | 48×16 | **56×24** | 剑型投射物 | 雷剑 → 与 Cloudpiercer 同级 |
| 6 | **Minor Thunderbolt Proj** | 16×64 | **24×80** | 雷电 | 竖直闪电 → 加宽使可见性更好 |
| 7 | **Formless Sword Wheel Proj** | 64×64 | **80×80** | 旋转剑轮 | 大招级 → 加大 |
| 8 | **Moonbone Shard Proj** | 24×16 | **28×20** | 碎片 | 月骨碎片 → 微调 |
| 9 | **Cinnabar Talisman Flame** | 24×24 | **32×32** | 符火 | 朱砂火焰 → 同级增大 |
| 10 | **Greenwood Array Field** | 96×96 | **96×96** ✅ | 大型 AoE | 青木阵 → 尺寸合理 |
| 11 | **Thunder Talisman Array** | 96×96 | **96×96** ✅ | 大型 AoE | 雷符阵 → 尺寸合理 |
| 12 | **Decree Judgement Beam** | 32×128 | **40×160** | 天罚光束 | 敕令光束 → 加宽加长 |
| 13 | **Star Eclipse Split Bolt** | 24×24 | **32×32** | 分裂弹 | 星蚀弹 → 增大 |
| 14 | **Boss Spirit Bolt** | 16×16 | **24×24** | Boss 弹射物 | Boss 射弹应比玩家的大 |
| 15 | **Boss Array Field** | 16×16 | **48×48** | Boss AoE | Boss 领域应远大于 16×16 |
| 16 | **Thunder Sword Proj** | 48×16 | **56×24** | 同上 | 已在 #5 |
| 17 | **Tribulation Lightning Proj** | 16×16 | **24×80** | 天劫闪电 | 竖直闪电型 |
| 18 | **Tribulation Warning Line Proj** | 16×4 | **24×6** | 预警线 | 太细看不清楚 |

---

## 七、方块 / Tiles（12 个）

### 参考标准
| 方块类型 | 标准尺寸 (PNG 2x) | 说明 |
|---|---|---|
| **所有固体方块（单格）** | **16×16** | 这是原版的硬性标准！1 格 = 16×16 px (1x) = 32×32 (2x 但这是整个 spritesheet) |
| 单个 Tile 帧 | **16×16** | 精确 16×16，有 8×8 子格用于斜坡/电线 |
| 多格物体 | 16×16 的倍数 | e.g. 2×2 = 32×32, 3×2 = 48×32 |
| 墙壁 | **16×16** | 与 Tile 同 |
| 树/植物 | **16×16** — 32×48 | 高型植物 |

> ⚠️ **这是最重要的硬性标准：** 单个 Tile 帧必须是 **16×16**（PNG 2x = 32×32 但在单个帧的 spritesheet 中每格 16×16）。Terraria 依赖此尺寸进行斜坡、制动器、电线、Tile 合并等所有计算。

### 当前尺寸与建议

| # | 方块 | 当前 | 建议 | 类型 | 调整理由 |
|---|---|---|---|---|---|
| 1 | Spirit Ore Tile | 16×16 | **16×16** ✅ | 矿石 | 完全符合标准 |
| 2 | Spirit Moss | 16×16 | **16×16** ✅ | 苔藓 | 符合标准 |
| 3 | Greenwood Soil Tile | 16×16 | **16×16** ✅ | 土壤 | 符合标准 |
| 4 | Spirit Herb | 16×24 | **16×24** ✅ | 草药（高于 1 格） | 合理，草药常高于单格 |
| 5 | Furnace Slag Tile | 16×16 | **16×16** ✅ | 矿石 | 符合标准 |
| 6 | Black Furnace Wall | 16×16 | **16×16** ✅ | 墙壁 | 符合标准 |
| 7 | Thunder Cloud Tile | 16×16 | **16×16** ✅ | 云块 | 符合标准 |
| 8 | Star Abyss Crystal Tile | 16×16 | **16×16** ✅ | 水晶 | 符合标准 |
| 9 | Sect Ruin Brick | 16×16 | **16×16** ✅ | 砖块 | 符合标准 |
| 10 | Fallen Heaven Jade Tile | 16×16 | **16×16** ✅ | 玉块 | 符合标准 |
| 11 | Moonbone Tile | 16×16 | **16×16** ✅ | 月骨 | 符合标准 |

### 多格物体（Object）

| # | 物体 | 当前 | 建议 | 有效格数 | 调整理由 |
|---|---|---|---|---|---|
| 1 | **Singing Thunder Stone** | 24×32 | **32×32** | 2×2 | 24 不对齐 → 必须是 16 的倍数 |
| 2 | **Rift Membrane** | 32×32 | **32×32** ✅ | 2×2 | 对齐 |
| 3 | **Sword Tablet** | 32×48 | **32×48** | 2×3 | 对齐（16 的倍数）✅ |
| 4 | **Broken Heaven Tablet** | 32×64 | **32×64** | 2×4 | 对齐 ✅ |
| 5 | **Archive Light Pillar** | 32×96 | **32×96** | 2×6 | 对齐 ✅ |

> 唯一需要修正的是 **Singing Thunder Stone**：24×32 → **32×32**

---

## 八、UI 元素

| # | UI 元素 | 当前 | 建议 | 参考 |
|---|---|---|---|---|
| 1 | Spiritual Energy Bar Frame | 164×16 | **164×16** ✅ | 自定义 UI，合理 |
| 2 | Spiritual Energy Bar Fill | 160×12 | **160×12** ✅ | 比 Frame 略小以留边距 |
| 3 | Pressure Warning Icon | 32×32 | **32×32** ✅ | Buff 图标尺寸 |
| 4 | Artifact Slot Frame | 40×40 | **40×40** ✅ | 配饰槽加大版 |
| 5 | Tribulation Warning Line | 16×4 | **16×4** ✅ | 细线 UI |

---

## 九、工作站方块（Stations，9 个）

### 参考标准
| 工作站（原版） | 约尺寸 (PNG 2x) |
|---|---|
| Work Bench | 32×16 (2×1 格) |
| Furnace | 48×32 (3×2 格) |
| Anvil | 32×24 (2×1.5 格) |
| Hellforge | 48×32 (3×2 格) |
| Alchemy Table | 48×32 (3×2 格) |
| Crystal Ball | 32×32 (2×2 格) |
| Ancient Manipulator | 48×48 (3×3 格) |

### 当前尺寸

工作站由 `GeneratedStations.cs` 管理，当前代码中的 Item 尺寸：

| 工作站 | Item 当前 (w×h) | 建议 | 参考 |
|---|---|---|---|
| Earth Clay Furnace | 48×48 | **48×32** | Furnace 3×2 格 |
| Simple Talisman Table | 48×32 | **48×32** ✅ | Alchemy Table 同级 |
| Star Pattern Cauldron | 64×64 | **48×48** | Cauldron ~3×3 格 |
| Thunder Pattern Forge | 64×48 | **48×32** | Hardmode Forge ~3×2 格 |
| Sect Trial Altar | 64×48 | **48×48** | Altar ~3×3 格 |
| Heaven Fire Furnace | 64×64 | **64×48** | 大型锻造台 ~4×3 格 |
| Dao Severing Altar | 80×48 | **64×48** | 终局 Altar ~4×3 格 |

---

## 十、总结优先级

### 🔴 必须修正（硬性标准违反）
| 问题 | 当前 | 修正 | 原因 |
|---|---|---|---|
| Singing Thunder Stone | 24×32 | **32×32** | Tile 必须是 16 的倍数 |
| 城镇 NPC 尺寸不统一 | 36×48 ~ 42×60 | 统一 **40×56** | 原版硬标准 |
| Boss 头部过大 | 48×48 | **40×40** | 原版最大 32×32 |

### 🟡 强烈建议（游戏体验改善）
| 类别 | 调整范围 |
|---|---|
| Boss Body | +20~40px 全面提升 Boss 存在感 |
| 大型弹射物 | Boss 弹射物从 16×16 提升到 24×24+ |
| 武器图标 | 按等级梯度 56→72→80→96→104 |
| 终局材料 | 32×32 → 36×36 |

### 🟢 可选微调（精细化打磨）
| 类别 | 调整范围 |
|---|---|
| 小型弹射物 | +4~8px |
| 敌怪 | +2~8px |
| 普通材料 | +2~4px |

---

## 附录：技术约束检查清单

- [ ] 所有 Tile 帧 = 16×16（或 16 的倍数，多格物体）
- [ ] 动画帧纵向排列，帧间距 = 2px
- [ ] NPC 面朝左
- [ ] 弹射物箭头朝上
- [ ] 武器 > 85×85 时添加 `UsesBetterMeleeItemLocation`
- [ ] 最大纹理 ≤ 2048×2048（单图）
- [ ] PNG 使用预乘 Alpha
- [ ] `npc.width`/`npc.height`（命中框）≤ 精灵尺寸
- [ ] Boss Head ≤ 40×40
- [ ] 精灵与对应的 C# 类 `SetDefaults()` 尺寸一致

---

*本文档基于 Terraria 1.4.4.9、tModLoader 官方文档和 Calamity Mod Wiki 编写。所有建议尺寸均为 PNG 文件中的实际像素值（2x 渲染分辨率）。*
