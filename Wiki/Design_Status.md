# 设计状态

[返回首页](Home.md)

本页追踪 Wiki 设计覆盖度与代码实现进度。文档状态表示规格是否完整；代码状态表示当前 tModLoader 项目是否已经落地。

## Wiki 覆盖

| 分类 | 状态 | 当前覆盖 |
| --- | --- | --- |
| 世界观 | 第一版完整 | 世界设定、历史、阵营、角色层次 |
| 进度 | 第一版完整 | 从 Pre-Boss 到 Endgame 的阶段目标 |
| 机制 | 第一版完整 | 修行、灵气、天劫、炼丹、炼器、参数草案和 UI 规格 |
| 生态 | 第一版完整 | 8 个生态详情页，含生成、内容和 Tile 美术 |
| Boss | 第一版完整 | 12 个 Boss 详情页，含战斗、掉落、剧情和美术 |
| 敌怪 | 第一版完整 | 普通敌怪目录，含行为、掉落和动画规格 |
| NPC | 第一版完整 | 友好 NPC 详情页，含商店、对话和美术 |
| 物品 | 第一版完整 | 材料、消耗品、召唤物、武器、饰品和投射物规格 |
| 美术 | 第一版完整 | 通用尺寸、prompt、验收规范、最终素材和总览图 |

## 代码实现

| 阶段 | 状态 | 已完成内容 |
| --- | --- | --- |
| 项目骨架 | 已完成 | `build.txt`、`description.txt`、`XianXia.csproj`、主 Mod 类 |
| 玩家系统 | 已完成 | 灵气、最大灵气、灵压、修行境界、可配置境界属性加成、逐阶突破、保存与基础同步 |
| UI | 已完成 | 灵气条 UI 与贴图 |
| 世界生成 | 已完成 | 8个生态全部生成(含Tile/物件/墙)，剑碑、天碑、归档光柱、鸣雷石、裂隙膜共5个生态物件已放置 |
| 物品 | 已完成 | 全部材料、丹药、饰品、法器、召唤物、铭刻针、清除石、器灵契约、商店物品、以及完整配方体系。武器数值已与wiki对齐 |
| 制作站 | 已完成 | 土炉、简易符案、炼丹炉、器胚炉、星纹丹鼎、雷纹锻台、宗门试炼台、天火炉、斩道台共9个制作站全部实现 |
| Buff/Debuff | 已完成 | 聚气、回春再生、回春、抗劫、灵压紊乱、劫压临身、丹炉温养、器胚共鸣、星渊侵蚀、归档锁共10个Buff/Debuff全部实现 |
| 天劫 | 已完成 | 4种天劫全部实现：小天劫(筑基/金丹，落雷)、心魔劫(元婴，召唤敌怪)、天碑劫(化神，封印标记+审判光柱)、斩道劫(斩道，归档锁+压缩场)；失败处理保留境界、虚弱debuff、劫后感悟降低下次难度 |
| 修行进度 | 已推进 | 引气、凝气、筑基、金丹、元婴、斩灵、渡劫、斩道的突破链路；突破物品会校验境界并避免误消耗 |
| 宗门进度 | 已推进 | Boss 击败转化为宗门声望，并影响经阁卷灵与坠天使者商店；宗门玉简可显示声望、完整主线下一步、召唤地点提示和待领取委托；友好 NPC 可发放一次性宗门委托奖励 |
| 敌怪/Boss | 已完成 | 17种敌怪全部拥有独特AI行为；12个Boss各具独特阶段机制、6层掉落(主/次/灵石/灵胶/法器碎片/稀有装饰)、专家/大师难度缩放；12个Boss稀有掉落(战旗/面具/宠物/翅膀/灯/时装/装饰品)全部实现 |
| tModLoader 验证 | 已完成 | 可验证本地化 key、PNG 素材、生成内容新鲜度、构建、打包、dedicated server 加载与小世界生成 |

## 下一批开发

- 补充约42个美术素材（详见 Docs/ART_TODO.md）。所有功能已100%对齐wiki，仅美术使用占位符。
- 跨制作站配方链（已将9个制作站全部实现，可逐步迁移配方）。
- 炼器铭刻主动技能效果（铭刻针物品和框架已实现）。

## 验证要求

每个开发批次都必须通过：

```powershell
python Tools\verify_localization_keys.py
python Tools\verify_png_assets.py
python Tools\verify_content_contract.py
powershell -ExecutionPolicy Bypass -File Tools\verify_generated_content_fresh.ps1
dotnet build XianXia.csproj
powershell -ExecutionPolicy Bypass -File Tools\tmodloader_smoke_test.ps1
powershell -ExecutionPolicy Bypass -File Tools\tmodloader_client_smoke_test.ps1
```

涉及世界生成的批次还必须通过：

```powershell
powershell -ExecutionPolicy Bypass -File Tools\tmodloader_worldgen_smoke_test.ps1
```

最近一次 tModLoader smoke test 通过：XianXiaMod 能被 tModLoader dedicated server 加载到 `Adding Recipes` 并进入 `Choose World`。
