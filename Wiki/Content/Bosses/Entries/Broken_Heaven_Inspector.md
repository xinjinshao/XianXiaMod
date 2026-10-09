

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/broken_heaven_inspector/broken_heaven_inspector__body__v01.png" alt="残天监察使 body" width="96"> | 残天监察使 | `broken_heaven_inspector` | `body` | 160x160 |
| <img src="../../../../Assets/Final/broken_heaven_inspector/broken_heaven_inspector__boss_head__v01.png" alt="残天监察使 boss_head" width="96"> | 残天监察使 | `broken_heaven_inspector` | `boss_head` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：160x160，白玉甲胄人形，残金法旨，面部无五官只有印章。
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

## 战斗设计（裁决刃仍待扩展）

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

## 当前代码实现

- 三阶段门槛为70%和35%生命，保留普通灵弹、召唤门槛、难度数值和现有掉落。
- 避中令：橙色中央64×480区域危险，离开该区域。归位令：橙色两侧区域危险，回到中央空隙。法旨交替，危险位置在60帧预警开始时锁定，玩家移动不会重瞄。
- 光柱持续30帧，施放后恢复45帧。预警与恢复停止移动/其它施法并无接触伤害，恢复末帧仍安全；目标变化取消旧预告，重新完整间隔。
- 第二阶段起，每场最多成功创建两只仙傀。容量/位置失败不消耗缺额，下条完整预告再试，击败后不补刷。出生身体在Boss侧青色区域预告，生成资格在读条开始时锁定，不因突然转阶段而追加。
- 仙傀父实例绑定/限时撤场、裁决刃、独立美术、诏令链路和真实多人实战仍待完善。

第154轮源码组合22188项/弹体1427项、原生2846项及注册专服集成3133项通过。注册审计手动推进AI，不能据此认定完整玩家战斗、图形、援军生命周期或多人已验收。
