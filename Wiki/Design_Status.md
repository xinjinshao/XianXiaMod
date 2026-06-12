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
| 世界生成 | 已推进 | 浅层灵脉以及青木药园、沉炉矿脉、雷泽云层、星渊裂隙、万宗遗址、坠天宫阙、月骨深渊的生成骨架和采集掉落 |
| 物品 | 已推进 | 生成材料、丹药、饰品、法器、召唤物、基础配方、炼丹配方、炼器升级配方、丹药灵压管理、法器代价差异、法器觉醒和主题投射物行为 |
| 制作站 | 已推进 | 炼丹炉、器胚炉、丹药配方迁移、法器配方迁移与站点附近加成 |
| Buff/Debuff | 已推进 | 聚气、回春、抗劫、灵压紊乱、丹炉温养、器胚共鸣 |
| 天劫 | 已推进 | 小天劫（筑基/金丹，落雷）与心魔劫（元婴+，召唤敌怪）两种类型；失败处理保留境界、施加虚弱、返还部分灵气、失败次数降低下次挑战时长；劫后感悟永久灵气加成 |
| 修行进度 | 已推进 | 引气、凝气、筑基、金丹、元婴、斩灵、渡劫、斩道的突破链路；突破物品会校验境界并避免误消耗 |
| 宗门进度 | 已推进 | Boss 击败转化为宗门声望，并影响经阁卷灵与坠天使者商店；宗门玉简可显示声望、完整主线下一步、召唤地点提示和待领取委托；友好 NPC 可发放一次性宗门委托奖励 |
| 敌怪/Boss | 已推进 | 17种敌怪全部拥有独特AI行为（灵粒受击、钻地冲刺、符火弹幕、藤鞭投射、瘴毒环、炉心防御、铁屑蜂群、劫云闪现、雷鹰俯冲、星蚀闪避、星渊附体、执念格挡反击、经阁护盾、仙傀模块攻击、天碑举盾推进、月骨剑气、归档位置记录）；12个Boss各具独特阶段机制（治疗花、铁屑傀儡、劫云灵召唤、雷泽云层、星渊环形弹幕、无相剑影、药王藤灵、天碑雷道、残天仙傀、月骨归档仙魂、旧天道模块轮换）；Boss掉落表含主/次材料+灵石；专家/大师难度缩放 |
| tModLoader 验证 | 已完成 | 可验证本地化 key、PNG 素材、生成内容新鲜度、构建、打包、dedicated server 加载与小世界生成 |

## 下一批开发

- 进一步差异化 Boss 阶段机制（治疗花实体、封印盾、心魔镜像 NPC）。
- 实现炼器铭刻系统（青木、玄炉、雷泽、星渊、残天五种铭刻类型）。
- 添加更多制作站（土炉、简易符案、星纹丹鼎、雷纹锻台、宗门试炼台、天火炉、斩道台）。
- 实现终局路线选择的重铸/斩断/接纳三条分支装备。
- 增加更多 tModLoader 可自动验证的 smoke test 场景。

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
