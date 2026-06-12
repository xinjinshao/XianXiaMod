

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/black_furnace_iron_golem/black_furnace_iron_golem__body__v01.png" alt="玄炉铁傀 body" width="96"> | 玄炉铁傀 | `black_furnace_iron_golem` | `body` | 112x112 |
| <img src="../../../../Assets/Final/black_furnace_iron_golem/black_furnace_iron_golem__boss_head__v01.png" alt="玄炉铁傀 boss_head" width="96"> | 玄炉铁傀 | `black_furnace_iron_golem` | `boss_head` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：112x112，厚重铁傀，胸口橙红炉心，玄铁灰和暗红色板。
- 动画：`idle` 4 帧，`slam` 6 帧，`forge_breath` 6 帧，`hit` 2 帧。
- 头像：32x32，方形铁面和炉心光。
- 投射物：炉灰 16x16，火星 8x8。
- Prompt 重点：`heavy black iron furnace golem, glowing chest furnace, hammer fists, crisp pixel art`。

# 玄炉铁傀

[返回 Boss 总览](../Overview.md)

## 定位

- 英文 ID：`black_furnace_iron_golem`
- 阶段：Pre-Hardmode
- 所属线：玄炉器盟
- 角色：炼器系统入口 Boss。

## 召唤

在[沉炉矿脉](../../Biomes/Entries/Sunken_Furnace_Vein.md)修复旧炉，消耗炉渣铁、器胚碎片和火把。

## 战斗设计

- 阶段一：慢速重拳、跳砸、喷出炉灰。
- 阶段二：点燃胸口炉心，攻击速度提高。
- 阶段三：召唤铁屑灵修复护甲，玩家需要打断。
- 核心考点：读动作、绕背、清小怪。

## 掉落

- 旧炼器台。
- 器胚碎片。
- 炉心戒。
- 玄炉重锤。

## 剧情

它原本是器盟的自动锻造傀儡。坠天之夜后，炉心指令损坏，只会把一切移动物锻成材料。

## 代码实现

- ✅ 数值与wiki对齐（HP/伤害/防御）
- ✅ 独特阶段AI机制
- ✅ 6层掉落表（主/次/灵石/灵胶/法器碎片/稀有装饰）
- ✅ 专家/大师难度缩放
- ✅ Boss召唤校验（境界+前置+场地+时间）
