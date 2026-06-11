# XianXiaMod

XianXiaMod 是一个 Terraria/tModLoader 仙侠主题内容 Mod 项目。当前仓库已经从前期设计进入 MVP 可玩纵切阶段：先实现浅层灵脉、灵气资源、首批物品/敌怪和灵脉蠕虫 Boss，再逐步扩展完整修行进度。

## 设计入口

- [Wiki 首页](Wiki/Home.md)
- [设计状态](Wiki/Design_Status.md)
- [条目参数模板](Wiki/Content/Parameter_Templates.md)
- [项目约束](Docs/PROJECT_CONSTRAINTS.md)
- [美术资源生成方案](Docs/ART_ASSET_GENERATION_PLAN.md)

## 当前内容

- 完整世界观与历史背景。
- 修行、灵气、天劫、炼丹、炼器等核心机制。
- Boss、敌怪、NPC、生态、物品、武器、饰品、配方的 Wiki 规格。
- 面向 Terraria 像素风的美术资源尺寸、Prompt 和验收规范。
- tModLoader MVP 代码骨架，包括灵气玩家状态、灵气 UI、浅层灵脉 worldgen、早期材料、初级武器、三种早期敌怪和灵脉蠕虫 Boss。

## 技术目标

- 平台：tModLoader stable。
- 语言：C#。
- 内容方向：在 Terraria 原版探索、采集、制作和战斗循环上扩展仙侠修行成长线。
- 兼容方向：默认不强依赖 Calamity Mod，但参考大型内容 Mod 的 Wiki 组织、进度结构和参数化条目写法。

## 状态

当前代码纵切目标是能在 tModLoader 中构建并加载，验证第一条玩法闭环：

1. 探索浅层灵脉。
2. 获得下品灵石并唤醒灵气。
3. 使用引气符进入引气阶段。
4. 制作木纹飞剑或灵木短弩。
5. 使用灵脉香召唤并击败灵脉蠕虫。
