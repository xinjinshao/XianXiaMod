

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/tribulation_cloud_avatar/tribulation_cloud_avatar__body__v01.png" alt="劫云化身 body" width="96"> | 劫云化身 | `tribulation_cloud_avatar` | `body` | 128x96 |
| <img src="../../../../Assets/Final/tribulation_cloud_avatar/tribulation_cloud_avatar__boss_head__v01.png" alt="劫云化身 boss_head" width="96"> | 劫云化身 | `tribulation_cloud_avatar` | `boss_head` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：128x96，深紫雷云中露出模糊玉面，不要真实云雾糊边。
- 动画：`idle` 6 帧，`strike` 5 帧，`split` 4 帧。
- 头像：32x32，雷云和玉面轮廓。
- 投射物：雷柱 16x64，雷链 64x16。
- Prompt 重点：`storm cloud avatar with jade mask, lightning tribulation boss, sharp pixel cloud edges`。

# 劫云化身

[返回 Boss 总览](../Overview.md) | [天劫](../../../Systems/Tribulation.md)

## 定位

- 英文 ID：`tribulation_cloud_avatar`
- 阶段：Wall of Flesh 前后
- 所属线：残天司
- 角色：第一次天劫 Boss，用于筑基突破。

## 触发

玩家准备筑基并使用筑基丹后，天空聚云，给予 20 秒准备时间。玩家也可用 `引雷玉` 主动挑战。

## 战斗设计

- 阶段一：从上方落雷，地面出现短暂预警。
- 阶段二：云影横移，释放弧形雷链。
- 阶段三：生成玩家影子，影子只使用基础攻击。
- 核心考点：读预警、控制灵气爆发时机。

## 掉落

- 筑基印。
- 劫云露。
- 避雷玉佩材料。
- 天劫机制说明 Lore。

## 剧情

劫云化身不是有意识的敌人，而是残天司从旧天道规则中抽出的测试程序。

## 代码实现

- ✅ 数值与wiki对齐（HP/伤害/防御）
- ✅ 独特阶段AI机制
- ✅ 6层掉落表（主/次/灵石/灵胶/法器碎片/稀有装饰）
- ✅ 专家/大师难度缩放
- ✅ Boss召唤校验（境界+前置+场地+时间）
