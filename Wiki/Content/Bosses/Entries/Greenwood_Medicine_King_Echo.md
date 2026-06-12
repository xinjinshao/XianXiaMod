# 青木药王残影

[返回 Boss 总览](../Overview.md)

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/greenwood_medicine_king_echo/greenwood_medicine_king_echo__body__v01.png" alt="青木药王残影 body" width="96"> | 青木药王残影 | `greenwood_medicine_king_echo` | `body` | 112x112 |
| <img src="../../../../Assets/Final/greenwood_medicine_king_echo/greenwood_medicine_king_echo__boss_head__v01.png" alt="青木药王残影 boss_head" width="96"> | 青木药王残影 | `greenwood_medicine_king_echo` | `boss_head` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：112x112，老者残影、青铜药鼎、背后药枝光轮。
- 动画：`cast` 6 帧，`cauldron` 4 帧，`absorb` 5 帧。
- 头像：32x32，药鼎和木纹面容。
- 场地物件：治疗花 24x24，毒花 24x24。
- Prompt 重点：`ancient herbal alchemist echo, bronze cauldron, green wood halo, Terraria boss sprite`。

## 定位

- 英文 ID：`greenwood_medicine_king_echo`
- 阶段：Post-Plantera
- 所属线：青木药宗
- 角色：高阶炼丹与生命系装备 Boss。

## 召唤

完成药宗残卷链后，在青木药园深处使用药王印。

## 战斗设计

- 阶段一：药鼎投掷丹火和灵草弹。
- 阶段二：场地生成治疗花和毒花，玩家需区分。
- 阶段三：药王残影吸收场地植物，强化下一轮攻击。
- 核心考点：场地管理和目标优先级。

## 掉落

- 高阶丹炉。
- 药王木心。
- 元婴灵胎材料。
- 生命系饰品升级件。

## 剧情

药王在坠天之夜尝试以众生药性修补灵脉，失败后残影仍在重复配方。

## 代码实现

- ? 数值与wiki对齐（HP/伤害/防御）
- ? 独特阶段AI机制
- ? 6层掉落表（主/次/灵石/灵胶/法器碎片/稀有装饰）
- ? 专家/大师难度缩放
- ? Boss召唤校验（境界+前置+场地+时间）
