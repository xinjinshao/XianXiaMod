# 灵脉蠕虫

[返回 Boss 总览](../Overview.md) | [整体进度](../../../Progression/Overview.md)

## 定位

- 英文 ID：`spirit_vein_wyrm`
- 阶段：Pre-Boss
- 所属线：浅层灵脉
- 角色：第一个 Mod Boss，用于确认玩家已经接触灵气系统。

## 召唤

在[浅层灵脉](../../Biomes/Entries/Shallow_Spirit_Veins.md)使用 `灵脉香` 召唤。灵脉香由下品灵石、灵气凝胶和普通凝胶制作。

## 战斗设计

- 阶段一：地下穿行，短距离冲刺，留下灵气尘。
- 阶段二：生命低于 50% 后分裂出 2 到 3 个灵脉幼节。
- 核心考点：跳跃和平台移动，不要求高机动装备。
- 多人注意：主体和幼节由服务端生成，避免客户端重复分裂。

## 掉落

- 下品灵核：引气到凝气的早期材料。
- 灵脉鳞片：制作木纹飞剑升级件。
- 灵气凝胶：灵气药剂和低阶符箓。
- 灵脉香配方：首次击败后提示。

## 剧情

它不是妖兽，而是一截被唤醒的灵脉。击败它后，世界承认玩家可以接触灵气。

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/spirit_vein_wyrm/spirit_vein_wyrm__body__v01.png" alt="灵脉蠕虫 body" width="96"> | 灵脉蠕虫 | `spirit_vein_wyrm` | `body` | 96x32 |
| <img src="../../../../Assets/Final/spirit_vein_wyrm/spirit_vein_wyrm__boss_head__v01.png" alt="灵脉蠕虫 boss_head" width="96"> | 灵脉蠕虫 | `spirit_vein_wyrm` | `boss_head` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：96x32 分段蠕虫，4 到 6 节，青玉发光核心，深绿外轮廓。
- 动画：`move` 6 帧，每帧 96x32；`hit` 2 帧。
- 头像：32x32，突出圆形头部和玉色口器。
- 投射物：灵气尘 16x16，浅青粒子，不要烟雾糊边。
- Prompt 重点：`small jade spirit wyrm, segmented body, underground worm boss, readable side-view silhouette`。

## 代码实现

- ✅ 数值与wiki对齐（HP/伤害/防御）
- ✅ 独特阶段AI机制
- ✅ 6层掉落表（主/次/灵石/灵胶/法器碎片/稀有装饰）
- ✅ 专家/大师难度缩放
- ✅ Boss召唤校验（境界+前置+场地+时间）
