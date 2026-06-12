# 星渊胎主

[返回 Boss 总览](../Overview.md)

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/abyssal_star_womb/abyssal_star_womb__body__v01.png" alt="星渊胎主 body" width="96"> | 星渊胎主 | `abyssal_star_womb` | `body` | 128x128 |
| <img src="../../../../Assets/Final/abyssal_star_womb/abyssal_star_womb__boss_head__v01.png" alt="星渊胎主 boss_head" width="96"> | 星渊胎主 | `abyssal_star_womb` | `boss_head` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：128x128，暗蓝胚胎核心、晶刺外壳、紫黑液体边缘。
- 动画：`pulse` 6 帧，`open` 5 帧，`core_fly` 6 帧。
- 头像：32x32，星形瞳孔和裂隙外壳。
- 幼体：32x32，深蓝小型寄生体。
- Prompt 重点：`void star womb boss, crystalline shell, dark blue embryo core, readable horror pixel art`。

## 定位

- 英文 ID：`abyssal_star_womb`
- 阶段：Hardmode
- 所属线：星渊余孽
- 角色：星灾禁术和污染材料 Boss。

## 召唤

在[星渊裂隙](../../Biomes/Entries/Star_Abyss_Rift.md)使用星渊胎膜，或让裂隙污染值达到阈值后自然生成一次。

## 战斗设计

- 阶段一：固定核心，释放星刺和幼体。
- 阶段二：核心脱离外壳，追踪玩家。
- 阶段三：低血量打开裂隙门，周期性吸引玩家。
- 核心考点：清理召唤物和处理吸引力。

## 掉落

- 星蚀晶。
- 渊尘。
- 星渊眼。
- 星蚀弩机材料。

## 剧情

胎主不是星渊的源头，而是裂隙在玄垣界中长出的第一枚器官。

## 代码实现

- ? 数值与wiki对齐（HP/伤害/防御）
- ? 独特阶段AI机制
- ? 6层掉落表（主/次/灵石/灵胶/法器碎片/稀有装饰）
- ? 专家/大师难度缩放
- ? Boss召唤校验（境界+前置+场地+时间）
