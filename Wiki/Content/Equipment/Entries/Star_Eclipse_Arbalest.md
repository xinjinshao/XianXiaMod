

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/star_eclipse_arbalest/star_eclipse_arbalest__item_icon__v01.png" alt="星蚀弩机 item_icon" width="64"> | 星蚀弩机 | `star_eclipse_arbalest` | `item_icon` | 64x64 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 详见 [武器与饰品总览](../Overview.md) 中的美术素材表。
- Prompt：Terraria pixel art weapon icon, xianxia theme.

# 星蚀弩机

[返回装备总览](../Overview.md)

## 定位

- 英文 ID：`star_eclipse_arbalest`
- 阶段：Hardmode
- 类型：远程/星渊

## 数值

| 属性 | 值 |
|------|-----|
| 伤害 | 68 |
| 使用时间 | 24 |
| 击退 | 2.5 |
| 暴击 | 6% |
| 灵气消耗 | 16 |
| 射速 | 12 |
| 稀有度 | Light Red |

## 效果

- 发射星蚀裂弹，首次命中敌人后分裂为两枚SpiritBolt灵弹。

## 配方

器胚碎片x2 + 星蚀晶x6 + 下品灵石x12 @ 器胚炉

## 法器觉醒

- 境界要求：金丹
- 声望要求：48
- 效果：伤害+12%，灵耗降至13

## 代码实现

- ✅ 数值与wiki对齐
- ✅ 配方实现
- ✅ 法器觉醒机制

第278轮：裂弹绑定权威主人会话，失效后无伤退场并停止分裂；保留160tick、两穿透、每次命中±18度/85%速度/半伤害的两枚远程灵弹。满池不替换其它弹幕，仅一个空槽时只生成一枚。新增1字节取消同步，双方需同一构建。原生拥有者客户端两次命中分裂、击杀及容量检查通过；真实联机运输仍待验收。
