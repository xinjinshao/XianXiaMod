# 游方器师

[返回 NPC 总览](../Overview.md)

## 定位

- 英文 ID：`wandering_artificer`
- 解锁：击败[玄炉铁傀](../../Bosses/Entries/Black_Furnace_Iron_Golem.md)并放置旧炼器台。
- 功能：炼器服务、法宝升级、材料兑换。
- 阵营：[玄炉器盟](../../../Lore/Factions.md#玄炉器盟)。

## 服务

- 胚器制作：基础飞剑、护符、重器。
- 铭刻：为法宝加入雷、木、星渊或天道词条。
- 拆解：把部分旧法宝拆成器胚碎片。
- 说明：拆解不能返还 Boss 唯一材料。

## 对话方向

- 早期：把玩家称为“会走路的矿石账本”。
- 中期：解释炼器不是堆材料，而是让器物记住你的打法。
- 后期：警告天庭法旨也是一种器物，只是太多人把它当神谕。

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/wandering_artificer/wandering_artificer__body__v01.png" alt="游方器师 body" width="72"> | 游方器师 | `wandering_artificer` | `body` | 42x58 |
| <img src="../../../../Assets/Final/wandering_artificer/wandering_artificer__head__v01.png" alt="游方器师 head" width="72"> | 游方器师 | `wandering_artificer` | `head` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：42x58，深灰围裙、肩背小炉、腰间锤钳。
- 头像：32x32，护目镜和小炉火。
- 动画：idle 4 帧，hammer 4 帧，walk 6 帧。
- Prompt 重点：`wandering artificer NPC, portable furnace, smith tools, dark apron, Terraria pixel art`。

## 代码实现

- ✅ NPC类完整（对话/商店/委托/幸福度）
- ✅ 防御/生命值与wiki对齐
- ✅ 商店商品阶段性解锁
- ✅ 一次性宗门委托奖励
- ✅ Bestiary风味文本
