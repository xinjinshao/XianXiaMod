

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/cinnabar_talisman_flame_item/cinnabar_talisman_flame_item__item_icon__v01.png" alt="朱砂符火 item_icon" width="64"> | 朱砂符火 | `cinnabar_talisman_flame_item` | `item_icon` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 详见 [武器与饰品总览](../Overview.md) 中的美术素材表。
- Prompt：Terraria pixel art weapon icon, xianxia theme.

# 朱砂符火

[返回装备总览](../Overview.md)

## 定位

- 英文 ID：`cinnabar_talisman_flame_item`
- 阶段：Pre-Hardmode
- 类型：魔法/灵气

## 数值

| 属性 | 值 |
|------|-----|
| 伤害 | 24 |
| 使用时间 | 24 |
| 击退 | 2 |
| 暴击 | 4% |
| 灵气消耗 | 5 |
| 射速 | 7 |
| 稀有度 | Green |

## 效果

- 消耗灵气发射燃烧符火，命中施加狱炎（OnFire3）。

## 配方

器胚碎片x2 + 炉渣铁x6 + 下品灵石x12 @ 器胚炉

## 法器觉醒

- 境界要求：筑基
- 声望要求：24
- 效果：伤害+10%，灵耗降至4

## 投射物

| 素材 | 类型 | 尺寸 |
| --- | --- | --- |
| <img src="../../../../Assets/Final/cinnabar_talisman_flame/cinnabar_talisman_flame__projectile__v01.png" width="64"> | projectile | 32x32 |

## 代码实现

- ✅ 数值与wiki对齐
- ✅ 配方实现
- ✅ 法器觉醒机制

第279轮：符火保留魔法职业、180tick、一次穿透及命中3秒灼烧；新增主人会话绑定，死亡/重连等失效后先无伤再至多6tick清理，取消后的迟到命中不追加灼烧。新增1字节取消同步，双方需同一构建。原生真实NPC命中灼烧、自然到期及会话清理通过；真实联机Buff运输仍待验收。
