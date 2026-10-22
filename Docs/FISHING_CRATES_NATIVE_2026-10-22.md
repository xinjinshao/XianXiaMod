# 八种钓鱼匣原生奖励与世界门槛验收

后续[第315轮](CRATE_INVENTORY_NATIVE_2026-10-22.md)补充正式ItemSlot库存右键、扣件和满背包奖励路径；下方保留第314轮当时的证据边界。

第314轮（2026-10-22），推进R25。本轮补原生验收，未改玩法、配方、掉率或奖励，版本保持0.1.3。

## 验证内容

八种实际注册物品通过新建、Clone和ItemIO保存/读取标签产生三种实例；四个世界进度标志（困难模式、世纪之花、石巨人、月总）共16种组合，使用正式ItemLoader.CanRightClick判定，共128组断言、384次实例判定。含不一致的世界标志，要求所有前置门槛都满足，且同一物品随当前世界标志变化重新检查，不缓存旧世界许可。注册的钓鱼匣/困难匣与可消耗标志也符合预期。

| 匣 | 门槛 | 对应保证材料 |
|---|---|---|
| SpiritVeinFishingCrate | 无 | SpiritGel |
| GreenwoodFishingCrate | 无 | GreenwoodRoot |
| FurnaceFishingCrate | 无 | FurnaceSlagIron |
| StarAbyssFishingCrate | 困难模式 | StarAbyssMembrane |
| ThunderFishingCrate | 困难模式 | ThunderPatternFeather |
| SectFishingCrate | 困难模式及世纪之花 | SectTrialToken |
| HeavenFishingCrate | 再要求石巨人 | HeavenTabletRubbing |
| MoonboneFishingCrate | 再要求月总 | ColdMoonDust |

随后在全部门槛满足时，对每匣正式Player.OpenFishingCrate调用8次，共64次。每次独立核对4–8低阶灵石、2–4对应材料，恰好两种/两个奖励实体，没有其它稀有进度奖励。预期材料映射和范围由夹具显式指定，不从待测掉落规则反推。

新增329项原生，完整35,082项、39项源码、7,720项Gameplay及构建/打包/专服装载通过。[断言分组与包/报告哈希](Evidence/FishingCrates_2026-10-22.json)。最终恢复玩家、随机数、所有四个世界进度标志和原活动物品集合。来源世界只复制使用并校验原文件哈希未变。

## 边界

资格判定与开匣是分开调用：没有通过完整库存UI右键、消耗一个库存匣或验证锁定匣在UI中不可开。保存重载使用ItemIO内存标签，不是历史用户存档。没有真实鱼池、钓鱼调度、概率统计或双客户端运输。全部贴图仍复用原版木匣；R25实际非战斗获得整链、经济/独立美术和联机仍待验收。

本轮GitHub前一提交两条工作流复查仍排队，不能将本地通过视为托管成功。
