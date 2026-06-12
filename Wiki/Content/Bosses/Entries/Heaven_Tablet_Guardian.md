# 天碑守御

[返回 Boss 总览](../Overview.md)

## 定位

- 英文 ID：`heaven_tablet_guardian`
- 阶段：Post-Golem
- 所属线：残天司
- 角色：天庭残响和化神门槛 Boss。

## 召唤

在[坠天宫阙](../../Biomes/Entries/Fallen_Heaven_Palace.md)激活破损天碑。

## 战斗设计

- 阶段一：天碑悬浮，召唤碑文弹幕。
- 阶段二：碑文组成护盾，需要击破四枚印记。
- 阶段三：天碑裂开，释放审判光柱。
- 核心考点：按顺序处理印记和躲避直线威胁。

## 掉落

- 天道碎片。
- 天碑拓片。
- 天碑镇印。
- 化神突破材料。

## 剧情

天碑是旧天道的离线数据库。它不理解玩家，只能把玩家归类为未登记修士。

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/heaven_tablet_guardian/heaven_tablet_guardian__body__v01.png" alt="天碑守御 body" width="96"> | 天碑守御 | `heaven_tablet_guardian` | `body` | 96x160 |
| <img src="../../../../Assets/Final/heaven_tablet_guardian/heaven_tablet_guardian__boss_head__v01.png" alt="天碑守御 boss_head" width="96"> | 天碑守御 | `heaven_tablet_guardian` | `boss_head` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：96x160，竖直白玉碑，裂纹、残金碑文、悬浮碎片。
- 动画：`idle` 6 帧，`shield` 4 帧，`break` 5 帧。
- 头像：32x32，玉碑上半和金色眼状符号。
- 投射物：碑文弹 16x16，审判光柱 32x128。
- Prompt 重点：`floating jade heaven tablet, golden decree runes, cracked divine archive boss`。

## 代码实现

- ✅ 数值与wiki对齐（HP/伤害/防御）
- ✅ 独特阶段AI机制
- ✅ 6层掉落表（主/次/灵石/灵胶/法器碎片/稀有装饰）
- ✅ 专家/大师难度缩放
- ✅ Boss召唤校验（境界+前置+场地+时间）
