

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/thunder_marsh_jiao/thunder_marsh_jiao__body__v01.png" alt="雷泽蛟 body" width="96"> | 雷泽蛟 | `thunder_marsh_jiao` | `body` | 160x96 |
| <img src="../../../../Assets/Final/thunder_marsh_jiao/thunder_marsh_jiao__boss_head__v01.png" alt="雷泽蛟 boss_head" width="96"> | 雷泽蛟 | `thunder_marsh_jiao` | `boss_head` | 48x48 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：160x96，长身蛟龙，雷角、须、分段鳞片，雷紫和闪蓝色板。
- 动画：`fly` 8 帧，`dash` 4 帧，`roar` 4 帧。
- 头像：48x48，角和须必须清楚。
- 投射物：电弧 32x16，雷球 24x24。
- Prompt 重点：`thunder jiao dragon, lightning horns, cloud serpent boss, Terraria-style side-view`。

# 雷泽蛟

[返回 Boss 总览](../Overview.md)

## 定位

- 英文 ID：`thunder_marsh_jiao`
- 阶段：Hardmode
- 所属线：雷泽云层
- 角色：雷系装备和抗劫材料 Boss。

## 召唤

在[雷泽云层](../../Biomes/Entries/Thunder_Marsh_Clouds.md)使用鸣雷石。雷暴天气下召唤会进入强化版本。

## 战斗设计

- 阶段一：空中盘旋，俯冲，留下电弧。
- 阶段二：召唤雷纹鹰并制造云台。
- 阶段三：断角后狂暴，连续短冲刺。
- 核心考点：空战机动和平台控制。

## 掉落

- 雷纹羽。
- 劫云露。
- 雷纹剑匣材料。
- 雷纹锻台。

## 剧情

雷泽蛟是天劫系统泄漏出的自然妖兽。它既被残天司追捕，也在吞食残余劫雷成长。

## 代码实现

- ✅ 数值与wiki对齐（HP/伤害/防御）
- ✅ 独特阶段AI机制
- ✅ 6层掉落表（主/次/灵石/灵胶/法器碎片/稀有装饰）
- ✅ 专家/大师难度缩放
- ✅ Boss召唤校验（境界+前置+场地+时间）
