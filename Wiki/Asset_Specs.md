# 美术资源规格索引

[返回首页](Home.md) | [美术资源生成方案](../Docs/ART_ASSET_GENERATION_PLAN.md)

本页把 Wiki 条目中的美术描述统一成可执行规格。所有资源均遵循 Terraria-like 2D pixel art、透明背景、左上光源、深色轮廓、有限色板、禁止文字和 UI 的规则。

## 通用画布

| 类型 | 建议画布 | 用途 | 备注 |
| --- | --- | --- | --- |
| 小材料 | 16x16 或 24x24 | 常见掉落物、碎片、草药 | 单体居中，保留 1 到 2 像素 padding |
| 普通物品 | 32x32 | 背包图标、消耗品、饰品 | 轮廓优先，细节不要挤满画面 |
| 中型武器 | 48x48 或 64x64 | 飞剑、符器、弩机 | 默认左下到右上朝向 |
| 投射物 | 16x16 到 48x48 | 灵弹、剑气、符火、雷光 | 明确运动方向和碰撞核心 |
| 小敌怪 | 48x48 | 史莱姆、虫、蝠 | 至少 idle、move、hit |
| 中敌怪 | 64x64 或 96x96 | 傀儡、修士、飞行怪 | 动画帧尺寸固定 |
| Boss 主体 | 128x128 到 256x256 | Boss 基础 sprite | 大型或分段 Boss 可拆部件 |
| Boss 头像 | 32x32 或 48x48 | 地图/UI 头像 | 只保留头部核心剪影 |
| Tile | 16x16 或 16x16 组图 | 地形块、矿石、墙 | 必须平铺检查 |
| Buff 图标 | 32x32 | 状态图标 | 单一符号，避免复杂背景 |

## 文件命名

```text
{id}__{output_type}__v{number}.png
```

示例：

```text
spirit_ore_tile__tile_sample__v01.png
cloudpiercer_flying_sword__item_icon__v01.png
spirit_vein_wyrm__boss_head__v01.png
```

## 统一 Prompt 后缀

```text
Terraria-like 2D pixel art, transparent background, single centered sprite, crisp edges, strong dark outline, limited palette, readable silhouette, left-top lighting, no text, no UI, no scene background.
```

## 仙侠视觉词库

| 主题 | 关键词 | 推荐色 |
| --- | --- | --- |
| 灵气 | mist ribbon, jade glow, inner light | 青玉、浅青、深绿 |
| 符箓 | talisman paper, cinnabar ink, seal mark | 朱砂、旧纸黄、墨黑 |
| 飞剑 | slender blade, cloud guard, spirit trail | 钢银、云白、灵蓝 |
| 炼丹 | bronze cauldron, herbal vapor, pill glow | 青铜、药绿、暖金 |
| 炼器 | furnace coal, hammered metal, ember seam | 玄铁、暗红、橙火 |
| 天劫 | storm cloud, lightning vein, cracked sky | 雷紫、闪蓝、炭黑 |
| 星渊 | star infection, void crystal, dark fluid | 深蓝、紫黑、冷白 |
| 旧天道 | jade tablet, golden decree, archive seal | 白玉、残金、墨灰 |

## 条目内美术段落模板

```text
## 美术资源

- 主体：{画布尺寸}，{剪影描述}，{颜色与轮廓要求}。
- 动画：{帧组与每帧尺寸}。
- 图标：{item icon 或 boss head 尺寸}。
- Prompt 重点：{英文提示词要点}。
- 验收：透明背景、无文字、无 UI、轮廓在小尺寸下可读。
```

相关页面：

- [Boss 总览](Content/Bosses/Overview.md)
- [敌怪总览](Content/Enemies/Overview.md)
- [物品总览](Content/Items/Overview.md)
- [武器与饰品](Content/Equipment/Overview.md)
