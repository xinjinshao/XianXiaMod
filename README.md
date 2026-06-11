# XianXiaMod

XianXiaMod 是一个 Terraria/tModLoader 仙侠主题内容 Mod。项目目标是在 Terraria 原版探索、采集、制作和战斗循环上，扩展“灵气、修行境界、丹药、法器、天劫、宗门遗迹与末法天道”的成长线。

## 当前状态

- tModLoader 项目骨架已建立，当前目标是持续推进到完整可玩版本。
- 已实现灵气玩家状态、灵气 UI、修行境界、境界属性加成、灵压、逐阶突破、保存与基础同步。
- 已实现浅层灵脉与 7 个生成生态的 worldgen 骨架。
- 已为生成生态 Tile 接入基础采集掉落，覆盖雷泽云层和万宗遗址等关键生态材料。
- 已接入最终美术素材，生成材料、丹药、饰品、法器、投射物、敌怪、Boss、Boss 召唤物、Tile、Biome 和本地化。
- 已实现丹药突破、回春/聚气/抗劫 buff、灵压紊乱 debuff、突破天劫事件和站点附近加成。
- 已实现炼丹炉与器胚炉，并迁移丹药、饰品和法器配方。
- 已实现 5 个友好 NPC：药宗学徒、游方炼器师、观劫客、经阁卷灵、坠天使者，以及按进度变化的商店和对话。
- 已为友好 NPC 接入 Bestiary 风味文本，方便在游戏内查看其世界观定位。
- 已为友好 NPC 接入原版幸福度生态偏好。
- 已为生成 Boss 接入召唤配方、最低修行境界门槛和通用阶段 AI。
- 已为生成敌怪与 Boss 接入 Bestiary 文本，图鉴中可查看生态、掉落和试炼定位。
- 已为浅层灵脉的手写敌怪、灵脉蠕虫和幼节接入 Bestiary 文本。
- 已接入宗门声望：Boss 击败会提升声望，并影响部分高阶 NPC 商店。
- 已新增宗门玉简：玩家可使用它读取当前世界和自身修行对应的下一步目标。
- 已接入 Calamity Mod 软兼容探测入口，但不强依赖 Calamity。
- 已新增 tModLoader smoke test，用于确认 Mod 能被 tModLoader 实际加载。
- 已新增 Bestiary 本地化 key 校验脚本，减少图鉴缺失文本的回归。

## 设计入口

- [Wiki 首页](Wiki/Home.md)
- [设计状态](Wiki/Design_Status.md)
- [内容进度](Wiki/Progression/Overview.md)
- [机制总览](Wiki/Systems/Overview.md)
- [素材总览](Wiki/Art_Gallery.md)
- [项目约束](Docs/PROJECT_CONSTRAINTS.md)
- [美术资源生成方案](Docs/ART_ASSET_GENERATION_PLAN.md)

## 开发目标

- 平台：tModLoader stable
- 语言：C#
- 内容参考：Terraria 原版节奏、Calamity Wiki 的大型内容组织方式，以及仙侠修行题材
- 兼容策略：不强依赖 Calamity，保留软兼容配置与扩展点
- 开发节奏：分批开发，每批通过 tModLoader smoke test 后提交并推送

## 验证

每个开发批次都必须通过：

```powershell
python Tools\verify_localization_keys.py
dotnet build XianXia.csproj
powershell -ExecutionPolicy Bypass -File Tools\tmodloader_smoke_test.ps1
```

`Tools\tmodloader_smoke_test.ps1` 会同步当前仓库到 tModLoader 的 `ModSources\XianXia`，构建并打包 `XianXia.tmod`，再启动 tModLoader dedicated server 确认 XianXiaMod 能完成加载。

涉及世界生成的批次还必须通过：

```powershell
powershell -ExecutionPolicy Bypass -File Tools\tmodloader_worldgen_smoke_test.ps1
```
