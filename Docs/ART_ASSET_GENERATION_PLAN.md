# 美术资源生成方案

本文档定义 XianXia Terraria/tModLoader Mod 的美术资源生成流程。目标不是“临时画几张图”，而是建立一条可重复、可审查、可版本管理的资源流水线。

## 1. 核心原则

- Codex 负责编排：整理资产需求、生成 spec、调用脚本、维护命名和目录。
- 图片模型负责出图：根据结构化 spec 生成候选图、参考图迭代图或局部编辑图。
- 脚本负责后处理：裁切、透明背景、统一画布、nearest-neighbor 缩放、拼接 spritesheet。
- 人工负责验收：确认风格、轮廓、尺寸、可读性、版权风险和游戏内表现。

所有 AI 生成图必须先进入 `Assets/Generated/`，不得直接进入 `Content/`。

## 2. 目录规范

```text
Assets/
  Specs/
    Enemies/
    Bosses/
    Weapons/
    Items/
    Tiles/
    Projectiles/
  Reference/
  Generated/
  Cleaned/
  Final/
Tools/
  generate_assets.py
  postprocess_assets.py
  make_spritesheet.py
  review_contact_sheet.py
Docs/
  ART_ASSET_GENERATION_PLAN.md
```

- `Assets/Specs/`：结构化资产描述，建议使用 YAML。
- `Assets/Reference/`：原创参考图、锚点资产、人工草图、风格板。
- `Assets/Generated/`：图片模型原始输出。
- `Assets/Cleaned/`：脚本处理后的候选资源。
- `Assets/Final/`：人工验收通过、可准备接入 Mod 的资源。
- `Content/`：tModLoader 实际加载的最终资源，只有验收通过后才能复制或导入。

## 3. 风格规范

整体风格：

- Terraria-like 2D pixel art。
- 侧视图优先，轮廓清晰，背景透明。
- 黑色或深色外轮廓，高对比剪影。
- 单个资产控制在低到中等细节密度。
- 光源方向固定为左上方。
- 避免复杂背景、文字、UI、真实照片质感、柔和毛边和过度抗锯齿。

仙侠主题表达：

- 优先使用灵气、玉石、符箓、剑气、法阵、雷劫、丹炉、灵木、灵矿、云纹等符号。
- 视觉语言应与 Terraria 像素风兼容，不做过度写实的国风插画。
- 颜色可以使用青玉、赤金、玄铁、雷紫、灵蓝、朱砂红等，但单资产色彩不宜过多。

禁止事项：

- 不复制 Terraria、Calamity 或其它 Mod 的原始贴图。
- 不生成带可识别商标、真实人物、文字水印、UI 截图的素材。
- 不使用模糊半透明烟雾作为主要轮廓。
- 不让特效遮挡武器、敌怪或 Boss 的核心剪影。

## 4. 资产类型规则

敌怪：

- 使用正侧视或略微 3/4 侧视。
- 先生成 base sprite，再生成 idle、move、attack、hit 等动画帧。
- 小型敌怪优先保证 32x32 到 64x64 画布内可读。
- 掉落物、生态位、攻击方式必须在 spec 中说明。

Boss：

- 先生成 concept sprite，再拆分 body、部件、特效、boss head。
- Boss 至少准备主体 sprite 与 boss head。
- 大型 Boss 应保留清楚的头部、弱点或主要攻击器官。
- 不要求一次生成完整动画表，先确认主视觉锚点。

武器：

- 作为 item icon 时必须单体居中、透明背景。
- 近战武器需要明确朝向，通常从左下指向右上。
- 法器、飞剑、符箓等仙侠武器必须在小尺寸下保持识别度。
- 若需要手持或使用动画，单独建立 held/use 资源 spec。

物品与材料：

- 单图标，透明背景，高对比轮廓。
- 常用材料建议控制在 16x16 到 32x32 画布。
- 稀有材料可以有轻微发光，但边缘必须干净。

地形 Tile：

- 以 16x16 逻辑切片思维设计。
- 必须考虑重复平铺，纹理不能形成明显接缝。
- 矿石 Tile 需要同时考虑原石背景与发光/矿脉识别。
- Tile 样张进入游戏前必须做实际平铺检查。

投射物：

- 小尺寸、高识别度、透明背景。
- 飞剑、灵弹、符火、雷光等必须有明确朝向。
- 发光边缘可以存在，但碰撞核心必须清楚。

Buff 与 UI 图标：

- 优先 32x32。
- 不使用复杂背景。
- 用单一核心符号表达效果，例如雷纹代表雷劫、丹纹代表炼药、云纹代表身法。

## 5. Spec 模板

敌怪：

```yaml
id: wandering_spirit_slime
type: enemy
display_name_zh: 游灵史莱姆
progression: early_prehardmode
theme: spirit, forest, low-level cultivation
role: slow contact enemy
silhouette: round slime with a small floating talisman core
palette: jade green, pale cyan, dark teal outline
size_hint: small
animation:
  - idle
  - hop
  - hit
drops:
  - low_grade_spirit_shard
notes:
  - transparent background
  - Terraria-like pixel art
  - readable side-view silhouette
```

Boss：

```yaml
id: thunder_tribulation_serpent
type: boss
display_name_zh: 劫雷虬蛇
progression: hardmode
theme: thunder tribulation, storm cloud, ancient serpent
scale_feel: much larger than regular enemies
silhouette: long coiling serpent with horned head and lightning whiskers
features:
  - glowing thunder core
  - segmented body
  - sharp head silhouette
palette: storm purple, lightning blue, dark charcoal
required_outputs:
  - body_concept
  - boss_head
  - projectile_lightning
notes:
  - transparent background
  - no scene background
  - boss head must remain readable at small size
```

武器：

```yaml
id: cloudpiercer_flying_sword
type: weapon
display_name_zh: 破云飞剑
weapon_class: magic melee hybrid
progression: pre_hardmode
theme: flying sword, cloud pattern, refined steel
shape: slender sword with cloud-shaped guard and pale cyan aura
palette: steel silver, cloud white, spirit blue
size_hint: medium
required_outputs:
  - item_icon
  - projectile
notes:
  - transparent background
  - item icon only for first pass
  - readable at Terraria inventory size
```

物品：

```yaml
id: low_grade_spirit_stone
type: material
display_name_zh: 下品灵石
progression: early_prehardmode
theme: condensed spiritual energy crystal
shape: small faceted jade crystal with inner glow
palette: jade green, pale cyan, dark green outline
size_hint: small
notes:
  - transparent background
  - single centered item icon
  - avoid text or symbols
```

Tile：

```yaml
id: spirit_ore_tile
type: tile
display_name_zh: 灵石矿
progression: early_prehardmode
theme: stone with embedded jade spirit ore
surface_details: gray stone base with small glowing jade mineral veins
palette: stone gray, jade green, pale cyan highlights
tile_size: 16x16
required_outputs:
  - tile_sample
notes:
  - seamless repeating tile texture
  - readable ore clusters
  - avoid noisy details
```

## 6. Prompt 模板

敌怪：

```text
Draw a Terraria-style pixel art enemy sprite.
Subject: {display_name_en}
Theme: {theme}
Role: {role}
Silhouette: {silhouette}
Palette: {palette}
View: 2D side-view
Composition: single centered sprite
Background: transparent
Style: clean pixel art, crisp edges, strong dark outline, limited palette, readable silhouette, no text, no UI, no scene background
Output intent: base sprite concept for a Terraria/tModLoader enemy
```

Boss：

```text
Draw a Terraria-style pixel art boss concept sprite.
Subject: {display_name_en}
Theme: {theme}
Scale feel: {scale_feel}
Silhouette: {silhouette}
Special features: {features}
Palette: {palette}
View: 2D side-view
Composition: single centered sprite
Background: transparent
Style: Terraria-like pixel art, crisp edges, strong dark outline, limited palette, readable large silhouette, no text, no UI, no scene background
Output intent: boss base sprite concept for a Terraria/tModLoader mod
```

武器与物品：

```text
Draw a Terraria-style pixel art item icon.
Item: {display_name_en}
Category: {type}
Material/theme: {theme}
Shape/features: {shape}
Palette: {palette}
Composition: single centered item icon
Background: transparent
Style: crisp pixel art, strong readability at small inventory size, strong dark outline, limited palette, no text, no UI
Output intent: item icon for a Terraria/tModLoader mod
```

Tile：

```text
Draw a Terraria-style pixel art terrain tile sample.
Tile: {display_name_en}
Theme/material: {theme}
Surface details: {surface_details}
Palette: {palette}
Goal: seamless repeating tile texture
Style: clean pixel art, limited palette, crisp edges, tileable feel, no text, no UI
Background: transparent or plain
Output intent: terrain tile texture sample for a Terraria/tModLoader mod
```

投射物：

```text
Draw a Terraria-style pixel art projectile sprite.
Projectile: {display_name_en}
Theme: {theme}
Shape/features: {shape}
Palette: {palette}
Direction: points from left to right
Composition: single centered projectile
Background: transparent
Style: crisp pixel art, readable motion direction, limited palette, no text, no UI
Output intent: projectile sprite for a Terraria/tModLoader mod
```

## 7. 生成流程

第一阶段：建立锚点资产。

- 只做 5 个代表性资产：1 个敌怪、1 个 Boss、1 个武器、1 个材料、1 个 Tile。
- 每个资产生成 3 到 4 个候选图。
- 人工选出最接近项目风格的版本，放入 `Assets/Reference/` 作为风格锚点。

第二阶段：生成可用单帧。

- 根据锚点资产批量生成同风格资源。
- 每个资源先只要求单帧或 item icon。
- 通过后处理脚本统一裁切、缩放和画布。

第三阶段：扩展动画与变体。

- 敌怪按 idle、move、attack、hit 拆分生成。
- Boss 按主体、部件、boss head、投射物和特效拆分生成。
- 逐帧生成后由脚本拼接 spritesheet。

第四阶段：接入 tModLoader。

- 通过人工验收的资源进入 `Assets/Final/`。
- 需要加载的最终 PNG 再放入对应 `Content/` 路径。
- 对应 C# 类、本地化键、贴图路径必须一起检查。

## 8. 后处理要求

后处理脚本至少支持：

- 保留或生成透明背景。
- 裁切透明边缘，保留统一 padding。
- 使用 nearest-neighbor 缩放，禁止线性插值导致像素变糊。
- 统一画布尺寸，例如 16x16、32x32、48x48、64x64、96x96、128x128。
- 可选调色板压缩，降低颜色漂移。
- 生成 contact sheet，方便人工横向比较候选图。
- 拼接 spritesheet，并保持每帧尺寸一致。

输出命名：

```text
{id}__{output_type}__v{number}.png
```

示例：

```text
low_grade_spirit_stone__item_icon__v01.png
wandering_spirit_slime__idle__v03.png
thunder_tribulation_serpent__boss_head__v02.png
```

## 9. 验收清单

每张资源进入 `Assets/Final/` 前必须检查：

- 是否透明背景。
- 是否没有文字、水印、UI 或复杂场景背景。
- 是否没有复制 Terraria、Calamity 或其它 Mod 的原始素材。
- 轮廓在小尺寸下是否可读。
- 色彩是否符合当前资产线的锚点风格。
- 像素边缘是否清晰，没有明显糊边。
- 画布尺寸与 tModLoader 用途是否匹配。
- 动画帧是否对齐，帧间比例是否稳定。
- Tile 是否能平铺。
- Boss head 是否能在地图或 UI 小尺寸下识别。

## 10. tModLoader 落地注意事项

- `ModItem` 默认贴图路径应与类路径对应，除非显式覆写 `Texture`。
- 装备类物品不一定只有 item icon；头盔、身体、腿部等穿戴显示可能需要额外贴图。
- Boss 通常需要主体贴图和 boss head 贴图。
- `ModProjectile` 贴图需要考虑朝向、旋转中心、碰撞箱与视觉尺寸的关系。
- `ModTile` 资源需要考虑 16x16 tile frame、地图颜色和掉落物 icon。
- 所有最终资源进入 `Content/` 后，应运行 tModLoader 加载检查。

## 11. 建议脚本职责

`Tools/generate_assets.py`：

- 读取 `Assets/Specs/` 下的 YAML。
- 根据资产类型套用 prompt 模板。
- 支持文本生成、参考图生成和多候选输出。
- 输出到 `Assets/Generated/`。
- 记录模型、参数、prompt、时间和输入 spec。

`Tools/postprocess_assets.py`：

- 读取 `Assets/Generated/`。
- 裁切透明边缘。
- 统一 padding 与画布。
- 使用 nearest-neighbor 缩放。
- 输出到 `Assets/Cleaned/`。

`Tools/make_spritesheet.py`：

- 读取同一资产的多帧 PNG。
- 检查帧尺寸一致。
- 横向或网格拼接 spritesheet。
- 输出 spritesheet 与帧数据说明。

`Tools/review_contact_sheet.py`：

- 把候选图拼成审阅图。
- 标注文件名和版本号。
- 用于快速选择锚点资产。

## 12. 第一批推荐试制资产

先用以下 5 个资产验证流程：

- 敌怪：`wandering_spirit_slime`，游灵史莱姆。
- Boss：`thunder_tribulation_serpent`，劫雷虬蛇。
- 武器：`cloudpiercer_flying_sword`，破云飞剑。
- 材料：`low_grade_spirit_stone`，下品灵石。
- Tile：`spirit_ore_tile`，灵石矿。

这 5 个资源覆盖敌怪、Boss、武器、材料、地形五条主要资产线，适合用来确定项目整体视觉锚点。

## 13. 当前执行顺序

1. 建立 `Docs/ART_ASSET_GENERATION_PLAN.md`。
2. 创建 `Assets/Specs/`、`Assets/Generated/`、`Assets/Cleaned/`、`Assets/Final/`、`Assets/Reference/`、`Tools/`。
3. 下一步创建风格规范 `Docs/STYLE_GUIDE.md`。
4. 再创建 YAML 示例 spec 与 Python 脚本。
5. 最后生成第一批 5 个锚点资产候选图。
