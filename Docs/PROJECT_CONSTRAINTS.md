# Terraria Mod Project Constraints

本文档定义本项目的长期约束。后续新增代码、设定、美术、数值和文档时，默认以本文为准；若需要偏离，必须先记录原因。

## 1. 项目定位

- 项目类型：Terraria / tModLoader 内容型 Mod。
- 主题方向：仙侠、修行、法宝、灵气、境界、秘境、宗门、劫雷等东方幻想内容。
- 设计目标：在 Terraria 原版探索、采集、制作、战斗循环上扩展“修行成长线”，而不是替换 Terraria 的核心体验。
- 兼容目标：优先兼容 tModLoader stable；默认不强依赖 Calamity Mod，但内容设计应能与 Calamity 的高强度后期节奏共存。

## 2. 资料优先级

实现和设计决策按以下顺序取证：

1. tModLoader stable API 文档：<https://docs.tmodloader.net/docs/stable/annotated.html>
2. Terraria 官方 wiki.gg：<https://terraria.wiki.gg/>
3. Calamity Mod 官方 wiki.gg：<https://calamitymod.wiki.gg/>
4. 本项目已有代码、测试、设计文档。
5. 其它资料只可作为参考，不可覆盖官方资料。

截至 2026-06-07，tModLoader stable 文档页面显示版本为 v2026.04。升级 tModLoader 或 Terraria 版本时，必须复查破坏性 API 变化。

## 3. 技术约束

- 语言：C#。
- 平台：tModLoader stable，对应 Terraria 1.4 系列生态。
- 所有游戏内容优先使用 tModLoader 提供的标准类型和 hook，例如 `ModItem`、`ModProjectile`、`ModNPC`、`ModSystem`、`ModPlayer`、`ModBuff`、`ModTile`。
- 不直接修改 Terraria 或其它 Mod 的原始文件。
- 不使用反射、IL patch、运行时猴子补丁作为常规方案；只有标准 hook 无法满足且风险可控时，才允许单独记录理由后使用。
- 客户端视觉逻辑与服务端游戏逻辑必须分离；图形、音效、UI 等客户端专属代码不得在 dedicated server 上强制加载。
- 多人同步相关状态必须显式设计，避免只在本地客户端生效的数值、掉落、Boss 状态或世界进度。

## 4. 推荐目录结构

后续项目初始化时，优先采用以下结构：

```text
XianXia/
  build.txt
  description.txt
  icon.png
  XianXia.cs
  Common/
  Content/
    Items/
    Projectiles/
    NPCs/
    Buffs/
    Tiles/
    Biomes/
    Systems/
  Localization/
    en-US.hjson
    zh-Hans.hjson
  Assets/
    Source/
    Generated/
    Final/
  Docs/
    PROJECT_CONSTRAINTS.md
    ART_ASSET_GENERATION_PLAN.md
  Wiki/
```

约束：

- `Content/` 存放可加载游戏内容。
- `Common/` 存放共享工具、扩展方法、通用数据结构。
- `Localization/` 存放本地化文本；新增玩家可见文本必须走本地化。
- `Assets/Source/` 存放提示词、参考说明、分层源文件等工作资料。
- `Assets/Generated/` 存放待审 AI 生成资源。
- `Assets/Final/` 存放已验收、可进入 Mod 的资源。

## 5. 命名约束

- Mod 内部命名使用英文 PascalCase，例如 `SpiritStone`, `QiCondensationPill`, `ThunderTribulation`.
- 玩家可见中文名写入 `Localization/zh-Hans.hjson`。
- 英文名应表达功能，不使用拼音作为主要代码名，除非该词已是专有概念。
- 类名与文件名保持一致。
- 资源文件名与对应内容类型一致，例如 `Content/Items/SpiritStone.cs` 对应 `Content/Items/SpiritStone.png`。

## 6. 本地化约束

- 所有物品名、Tooltip、Buff 文本、NPC 对话、系统提示必须本地化。
- 默认至少维护 `zh-Hans` 与 `en-US`。
- 中文文案采用简体中文，语气贴合仙侠但避免堆砌生僻词。
- 英文文案以可读性优先，不逐字硬译。
- 本地化键应稳定；改名时避免无意义地破坏已有翻译键。

## 7. 内容设计约束

- 新内容必须嵌入 Terraria 的阶段性进度：Pre-Boss、Pre-Hardmode、Hardmode、Post-Plantera、Post-Golem、Post-Moon Lord。
- 每条修行成长线必须回答三个问题：如何解锁、如何升级、如何被战斗或探索使用。
- 新材料应尽量来自已有生态：矿物、Boss 掉落、事件、环境探索、钓鱼、宝箱、敌怪掉落。
- 不让玩家长期脱离 Terraria 原本的采集、制作、探索和 Boss 推进。
- 新机制必须有清晰反馈：视觉特效、Buff 图标、Tooltip、掉落来源或进度提示。
- 每个新增 Boss、事件或大型系统都必须有独立设计文档，至少包含：解锁条件、召唤方式、阶段行为、掉落、数值范围、多人同步风险。

## 8. 数值与平衡约束

- 原版 Terraria 是默认基准；Calamity 是高强度兼容参考，不是默认数值基准。
- 新武器不得在同进度完全覆盖原版同类武器；应通过机制差异形成选择，而不是纯数值碾压。
- 每个进度段新增装备必须有成本：材料、Boss 门槛、环境风险、制作站或副作用。
- 仙侠境界系统若提供永久属性，必须限制成长速率，并设置进度门槛。
- 不允许无限叠加、无限回蓝、无限免伤、无限召唤物、永久无敌等破坏性循环。
- 面向 Calamity 兼容时，可提供可选配置或检测逻辑，但不得强制玩家安装 Calamity。

## 9. Calamity 兼容约束

- Calamity Mod 是内容量、后期扩展、Boss 进度和高难度设计的重要参考。
- 参考 Calamity 时只学习结构经验，例如扩展 Boss 阶段、Post-Moon Lord 内容密度、材料链、难度配置、进度引导。
- 不复制 Calamity 的专有美术、文本、音频、代码、Boss 设计或物品设定。
- 若添加 Calamity 交互，必须通过软依赖、Mod 检测或条件配方实现。
- 不因 Calamity 存在而破坏无 Calamity 环境下的完整体验。

## 10. 美术资源约束

美术资源生成方案见 `Docs/ART_ASSET_GENERATION_PLAN.md`。该方案是本项目 AI 美术资产的执行规范。

核心约束：

- 所有 AI 生成资源先进入 `Assets/Generated/`，人工验收后再进入 `Assets/Final/` 或 `Content/`。
- 不使用 Terraria、Calamity 或其它 Mod 的原始贴图作为可发布资源。
- 可参考 Terraria 像素风尺寸、轮廓清晰度、动画帧组织方式，但必须产出原创资源。
- 采用“结构化 spec -> 图片生成 -> 脚本后处理 -> 人工验收 -> tModLoader 落地”的流水线。
- 图片生成必须基于结构化资产描述，不接受只有一句话的临时需求直接进入正式资源库。
- `Assets/Reference/` 中的锚点资产用于保持风格统一；批量生成前必须先确定锚点资产。
- 物品图标、弹幕、Buff、NPC、Tile、背景、UI 资源需分别建立尺寸和帧数规范。
- 后处理必须使用 nearest-neighbor 缩放，避免线性插值造成像素糊边。
- 进入 Mod 前必须检查透明背景、像素边缘、缩放表现、帧动画对齐、Tile 平铺效果和游戏内可读性。

## 11. 代码质量约束

- 小内容优先简单实现；共享抽象只在出现真实重复或跨系统规则时创建。
- 配方只在 tModLoader 允许的位置注册；修改既有配方应放在合适的后置 hook 中。
- NPC、弹幕、Buff、Player 状态必须避免魔法数字散落；关键数值集中命名。
- 随机数必须考虑多人一致性和服务端权威。
- 存档数据必须版本化或能兼容缺失字段。
- 配置项必须验证范围，不能假设玩家不会手动编辑配置文件。

## 12. 测试与验收约束

每次实现新内容至少检查：

- Mod 能在 tModLoader 中构建并加载。
- 新物品能通过预期方式获得。
- Tooltip、本地化、贴图路径、配方、稀有度、价值和研究数量合理。
- 单人模式功能正常。
- 若涉及战斗、掉落、世界状态或玩家永久数据，必须额外检查多人或服务端风险。
- 若涉及视觉资源，必须在游戏内确认尺寸、透明度、动画和特效不会遮挡核心玩法。

## 13. 版权与发布约束

- 不复制 Terraria、Calamity 或其它 Mod 的代码、贴图、音乐、文本。
- 可引用官方文档和 wiki 链接作为设计来源，但不可搬运大段文字。
- 所有第三方素材必须记录来源、许可证和可商用/可再分发状态。
- AI 生成素材必须保留生成方案、提示词、人工修改记录和验收结果。

## 14. 待补充

- 项目正式英文 Mod 名称与展示名。
- 是否需要 Calamity 软兼容。
- 第一阶段内容范围，例如首批物品、修行系统、敌怪或 Boss。
- 发布平台与版本号规则。

