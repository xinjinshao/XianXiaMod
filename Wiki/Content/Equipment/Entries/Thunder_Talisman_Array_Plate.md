

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/thunder_talisman_array_plate/thunder_talisman_array_plate__item_icon__v01.png" alt="雷符阵盘 item_icon" width="64"> | 雷符阵盘 | `thunder_talisman_array_plate` | `item_icon` | 48x48 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 详见 [武器与饰品总览](../Overview.md) 中的美术素材表。
- Prompt：Terraria pixel art weapon icon, xianxia theme.

# 雷符阵盘

[返回装备总览](../Overview.md)

## 定位

- 英文 ID：`thunder_talisman_array_plate`
- 阶段：Hardmode
- 类型：魔法/阵法

## 数值

| 属性 | 值 |
|------|-----|
| 伤害 | 46 |
| 使用时间 | 34 |
| 击退 | 0 |
| 暴击 | 6% |
| 灵气消耗 | 24 |
| 射速 | 0 |
| 稀有度 | Light Red |

## 效果

- 部署240tick雷电领域，每45tick降下MinorThunderbolt天雷。

## 配方

器胚碎片x2 + 劫云露x6 + 下品灵石x12 @ 器胚炉

## 法器觉醒

- 境界要求：金丹
- 声望要求：44
- 效果：伤害+12%，灵耗降至20

## 代码实现

- ✅ 数值与wiki对齐
- ✅ 配方实现
- ✅ 法器觉醒机制

第270轮持续法阵在权威端绑定生成时的主人槽位、对象和临时会话。重连、死亡或载入后旧阵不能继续恢复、脉冲或生成新雷击/药灵波次，并锁定失效后清理；新会话仍可正常放阵。已有子弹幕、客户端即时身份比较和真实联机运输仍待验收。

第272轮：共享小雷击继承父弹幕职业（剑匣近战、雷阵魔法），保留180tick及单次穿透；正常父弹幕结束不会截断已发雷击。主人死亡或权威会话变化后停止伤害并至多6tick清理，取消不可恢复。额外AI现为职业/取消两个字节，联机双方需同一构建。原生近战/魔法命中、自然到期与会话清理已验证；真实双客户端运输仍待验收。
