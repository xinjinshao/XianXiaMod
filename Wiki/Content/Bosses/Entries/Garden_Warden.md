# 药宗守园人

[返回 Boss 总览](../Overview.md)

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/garden_warden/garden_warden__body__v01.png" alt="药宗守园人 body" width="96"> | 药宗守园人 | `garden_warden` | `body` | 96x96 |
| <img src="../../../../Assets/Final/garden_warden/garden_warden__boss_head__v01.png" alt="药宗守园人 boss_head" width="96"> | 药宗守园人 | `garden_warden` | `boss_head` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：96x96，人形木傀儡，背负药篓，藤蔓手臂，深绿外轮廓。
- 动画：`idle` 4 帧，`cast` 6 帧，`hit` 2 帧。
- 头像：32x32，木面具和一片发光药叶。
- 场地物件：药种 16x16，治疗花 32x32。
- Prompt 重点：`wooden herbal sect garden guardian, vine arms, medicine basket, Terraria-style boss sprite`。

## 定位

- 英文 ID：`garden_warden`
- 阶段：Pre-Hardmode
- 所属线：青木药宗
- 角色：炼丹系统入口 Boss。

## 召唤

在[青木药园](../../Biomes/Entries/Greenwood_Herb_Garden.md)使用 `守园残钥`。首次可通过采集灵草触发药园提示获得配方。

## 战斗设计

- 阶段一：挥舞木杖，召唤藤蔓墙限制走位。
- 阶段二：种下三枚药种，药种不清理会长成治疗花。
- 阶段三：低血量释放花瘴环，要求玩家横向移动。
- 核心考点：处理场地物件，不是纯追逐。

## 掉落

- 青木丹炉：中阶炼丹制作站。
- 青木根：炼丹与生命饰品材料。
- 药园残卷：解锁药宗遗徒入住条件。
- 回春丹配方升级。

## 剧情

守园人是药宗留下的护园傀儡，仍把所有采药者视作入侵者。击败后，它把玩家误认为药宗新弟子。

## 代码实现

- ? 数值与wiki对齐（HP/伤害/防御）
- ? 独特阶段AI机制
- ? 6层掉落表（主/次/灵石/灵胶/法器碎片/稀有装饰）
- ? 专家/大师难度缩放
- ? Boss召唤校验（境界+前置+场地+时间）
