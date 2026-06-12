# 浅层灵脉

[返回生态总览](../Overview.md)

## 概念

浅层灵脉是玩家第一次接触修行系统的生态。它不应像完整新群系那样大规模改变世界，而是作为洞穴层的小型发光矿脉和灵气泉出现。

## 生成

- 阶段：开局可生成。
- 位置：地下浅层和洞穴上部。
- 规模：小型矿脉房间，宽 20 到 50 格。
- 视觉：灰石中夹杂青玉色灵脉，少量发光灵苔。

## 内容

- 敌怪：游灵史莱姆、碎玉虫、符纸蝠。
- Boss：[灵脉蠕虫](../../Bosses/Entries/Spirit_Vein_Wyrm.md)。
- 资源：下品灵石、灵气凝胶、灵苔。
- 制作引导：引气符、木纹飞剑、聚气坠。

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/spirit_ore_tile/spirit_ore_tile__tile__v01.png" alt="灵石矿 tile" width="64"> | 灵石矿 | `spirit_ore_tile` | `tile` | 16x16 |
| <img src="../../../../Assets/Final/spirit_moss/spirit_moss__tile__v01.png" alt="灵苔 tile" width="64"> | 灵苔 | `spirit_moss` | `tile` | 16x16 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- Tile：`spirit_ore_tile`，16x16，灰石底、青玉矿脉、浅青高光。
- 装饰：`spirit_moss`，16x16，深绿苔面和细小青色光点。
- 背景物：`spirit_spring`，32x32，静态灵泉，不做复杂透明雾。
- 地图颜色：冷青绿，避免和丛林过近。
- Prompt 重点：`seamless jade spirit ore in gray stone, Terraria terrain tile, crisp pixel edges`。

## 代码实现

- ✅ 世界生成骨架（Tile铺设+物件放置）
- ✅ 敌怪生成池（权重与wiki对齐）
- ✅ 环境效果（星渊污染/雷泽落雷/月骨灵压等）
- ✅ 判定阈值（tile count ≥ wiki指定值）
