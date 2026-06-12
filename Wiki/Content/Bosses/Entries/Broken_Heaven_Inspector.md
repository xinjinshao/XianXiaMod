

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/broken_heaven_inspector/broken_heaven_inspector__body__v01.png" alt="残天监察使 body" width="96"> | 残天监察使 | `broken_heaven_inspector` | `body` | 128x128 |
| <img src="../../../../Assets/Final/broken_heaven_inspector/broken_heaven_inspector__boss_head__v01.png" alt="残天监察使 boss_head" width="96"> | 残天监察使 | `broken_heaven_inspector` | `boss_head` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：128x128，白玉甲胄人形，残金法旨，面部无五官只有印章。
- 动画：`cast` 6 帧，`summon` 5 帧，`blade` 6 帧。
- 头像：32x32，无面玉盔和金色印章。
- 召唤物：仙傀 64x64。
- Prompt 重点：`broken celestial inspector, jade armor, golden decree scroll, faceless divine judge`。

# 残天监察使

[返回 Boss 总览](../Overview.md)

## 定位

- 英文 ID：`broken_heaven_inspector`
- 阶段：Post-Golem
- 所属线：残天司
- 角色：残天司人格化 Boss，推动终局路线。

## 召唤

化神境后使用天庭法旨，或完成坠天信使任务线后挑战。

## 战斗设计

- 阶段一：持法旨施放直线审判。
- 阶段二：召唤仙傀协同攻击。
- 阶段三：失去法旨后改用近身裁决刃。
- 核心考点：多目标压力和弹幕空隙。

## 掉落

- 天庭法旨。
- 残天冠印。
- 仙傀令。
- 终局路线线索。

## 剧情

监察使曾经负责记录修士功过。如今它的数据损坏，却仍坚持审判所有“不在册”的生命。

## 代码实现

- ✅ 数值与wiki对齐（HP/伤害/防御）
- ✅ 独特阶段AI机制
- ✅ 6层掉落表（主/次/灵石/灵胶/法器碎片/稀有装饰）
- ✅ 专家/大师难度缩放
- ✅ Boss召唤校验（境界+前置+场地+时间）
