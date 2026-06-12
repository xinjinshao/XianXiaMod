# 坠天信使

[返回 NPC 总览](../Overview.md)

## 定位

- 英文 ID：`fallen_heaven_messenger`
- 解锁：击败[天碑守御](../../Bosses/Entries/Heaven_Tablet_Guardian.md)。
- 功能：天庭任务、终局路线提示、Post-Golem 材料兑换。
- 阵营：残天司半叛离个体。

## 服务

- 解析天庭法旨。
- 用残天玉兑换路线提示物品。
- 提供坠天宫阙挑战入口。
- 在 Post-Moon Lord 后解释三条终局路线。

## 对话方向

- 早期：把玩家称作“未登记但未被抹除者”。
- 中期：承认残天司内部也有冲突。
- 后期：询问玩家想修复天道，还是让它第一次真正沉默。

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/fallen_heaven_messenger/fallen_heaven_messenger__body__v01.png" alt="坠天信使 body" width="72"> | 坠天信使 | `fallen_heaven_messenger` | `body` | 42x60 |
| <img src="../../../../Assets/Final/fallen_heaven_messenger/fallen_heaven_messenger__head__v01.png" alt="坠天信使 head" width="72"> | 坠天信使 | `fallen_heaven_messenger` | `head` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：42x60，破损白玉羽衣、半张金色面具、悬浮法旨碎片。
- 头像：32x32，半面具和碎法旨。
- 动画：idle 6 帧，decree 4 帧，walk 6 帧。
- Prompt 重点：`fallen celestial messenger NPC, broken jade robe, half golden mask, floating decree fragments`。

## 代码实现

- ✅ NPC类完整（对话/商店/委托/幸福度）
- ✅ 防御/生命值与wiki对齐
- ✅ 商店商品阶段性解锁
- ✅ 一次性宗门委托奖励
- ✅ Bestiary风味文本
