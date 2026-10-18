

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/greenwood_array_plate/greenwood_array_plate__item_icon__v01.png" alt="青木阵盘 item_icon" width="64"> | 青木阵盘 | `greenwood_array_plate` | `item_icon` | 48x48 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 详见 [武器与饰品总览](../Overview.md) 中的美术素材表。
- Prompt：Terraria pixel art weapon icon, xianxia theme.

# 青木阵盘

[返回装备总览](../Overview.md)

## 定位

- 英文 ID：`greenwood_array_plate`
- 阶段：Pre-Hardmode
- 类型：魔法/阵法

## 数值

| 属性 | 值 |
|------|-----|
| 伤害 | 18 |
| 使用时间 | 36 |
| 击退 | 0 |
| 暴击 | 4% |
| 灵气消耗 | 16 |
| 射速 | 0 |
| 稀有度 | Orange |

## 效果

- 部署300tick恢复领域。领域内每60tick回复1生命+1灵气。

## 配方

器胚碎片x2 + 青木根x6 + 下品灵石x12 @ 器胚炉

## 法器觉醒

- 境界要求：筑基
- 声望要求：24
- 效果：伤害+10%，灵耗降至14

## 代码实现

- ✅ 数值与wiki对齐
- ✅ 配方实现
- ✅ 法器觉醒机制

第270轮持续法阵在权威端绑定生成时的主人槽位、对象和临时会话。重连、死亡或载入后旧阵不能继续恢复、脉冲或生成新雷击/药灵波次，并锁定失效后清理；新会话仍可正常放阵。已有子弹幕、客户端即时身份比较和真实联机运输仍待验收。
