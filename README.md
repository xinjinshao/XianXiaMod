# XianXiaMod

XianXiaMod 是一个 Terraria/tModLoader 仙侠主题内容 Mod。项目目标是在 Terraria 原版探索、采集、制作和战斗循环上扩展“灵气、修行境界、丹药、法器、天劫、宗门遗迹与末法天道”的成长线。

## 当前状态

- tModLoader 项目骨架已建立，可通过 `dotnet build XianXia.csproj` 构建。
- 已实现灵气玩家状态、灵气 UI、修行境界、灵压、突破与基础网络同步。
- 已实现浅层灵脉与 7 个生成生态的 worldgen 骨架。
- 已接入素材生成结果，生成材料、丹药、饰品、法器、投射物、敌怪、Boss、Boss 召唤物、Tile、Biome 和本地化。
- 已实现第一批可玩机制：丹药突破、回春/聚气/抗劫 buff、灵压紊乱 debuff、生成法器灵气消耗。
- 已实现第一批友好 NPC：药宗学徒、游方炼器师、观劫客、经阁卷灵、坠天使者及基础商店。

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

## 验证

当前验证命令：

```powershell
dotnet build XianXia.csproj
```
