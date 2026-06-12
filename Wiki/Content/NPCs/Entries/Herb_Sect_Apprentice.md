

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/herb_sect_apprentice/herb_sect_apprentice__body__v01.png" alt="药宗遗徒 body" width="72"> | 药宗遗徒 | `herb_sect_apprentice` | `body` | 40x56 |
| <img src="../../../../Assets/Final/herb_sect_apprentice/herb_sect_apprentice__head__v01.png" alt="药宗遗徒 head" width="72"> | 药宗遗徒 | `herb_sect_apprentice` | `head` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：40x56，青绿短袍、药篓、发簪像嫩芽。
- 头像：32x32，药篓和青叶发簪作为识别点。
- 动画：idle 4 帧，talk 2 帧，walk 6 帧。
- Prompt 重点：`young herbal sect apprentice NPC, green robe, medicine basket, Terraria town NPC pixel art`。

# 药宗遗徒

[返回 NPC 总览](../Overview.md)

## 定位

- 英文 ID：`herb_sect_apprentice`
- 解锁：击败[药宗守园人](../../Bosses/Entries/Garden_Warden.md)并持有药园残卷。
- 功能：炼丹入门、灵草种植、恢复类物品。
- 阵营：[青木药宗](../../../Lore/Factions.md#青木药宗)。

## 商店阶段

| 阶段 | 新增出售 |
| --- | --- |
| 入住 | 灵草种子、回春丹、低阶丹方 |
| 击败血肉墙 | 凝气丹辅材、抗毒丹 |
| Post-Plantera | 高阶药引、药王印线索 |
| Post-Golem | 抗劫丹辅材 |

## 对话方向

- 早期：怀疑玩家是否懂得“采三留一”的药宗规矩。
- 中期：提示药王残影仍困在药园深处。
- 后期：质疑旧天道是否有资格判定众生药性。

## 代码实现

- ✅ NPC类完整（对话/商店/委托/幸福度）
- ✅ 防御/生命值与wiki对齐
- ✅ 商店商品阶段性解锁
- ✅ 一次性宗门委托奖励
- ✅ Bestiary风味文本
