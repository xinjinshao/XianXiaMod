

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/greenwood_soil_tile/greenwood_soil_tile__tile__v01.png" alt="青木土 tile" width="64"> | 青木土 | `greenwood_soil_tile` | `tile` | 16x16 |
| <img src="../../../../Assets/Final/spirit_herb/spirit_herb__tile__v01.png" alt="灵草 tile" width="64"> | 灵草 | `spirit_herb` | `tile` | 16x24 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- Tile：`greenwood_soil_tile`，16x16，深土与青根纹理。
- 植物：`spirit_herb`，16x24，青叶、浅绿发光叶脉。
- 结构：`old_wood_beam`，16x16，旧木带药宗云纹。
- 背景：药圃墙 16x16 可平铺，低饱和绿，不抢角色。
- Prompt 重点：`ancient herbal garden tile, jade leaves, wooden sect ruins, Terraria pixel art`。

# 青木药园

[返回生态总览](../Overview.md)

## 概念

青木药园是古代药宗的地下药田遗址。它把丛林探索、炼丹材料和生命系敌怪连接起来。

## 生成

- 阶段：Pre-Hardmode。
- 当前位置：地表附近与浅地下的随机青木矿斑及附近药圃，尚未限制在原版丛林。
- 当前结构：31×13药圃，四层青木土、六株可收获灵草和通行空地；中央有破损木顶、可穿行木梁及带炼药瓶的原版工作台，形成小型残破丹房。安全位置不足时允许跳过，小水池仍是后续设计。
- 生态判定：附近青木土、灵草矿块与对应建造方块合计至少120块，无额外采集解锁条件。

## 内容

- 敌怪：药园藤妖、花瘴蝶。
- Boss：[药宗守园人](../../Bosses/Entries/Garden_Warden.md)、[青木药王残影](../../Bosses/Entries/Greenwood_Medicine_King_Echo.md)。
- 资源：灵草、青木根、药露。
- NPC：[药宗遗徒](../../NPCs/Entries/Herb_Sect_Apprentice.md)。

## 当前实现与验收

每处药圃有124青木土，六株灵草初始幼苗/成长/成熟各两株。成熟株使用既有青木根和种子收获规则，其余株可继续生长。第293轮起，中央残破丹房提供原版工作台和放置瓶制作条件，可在附近使用已满足材料要求的原版配方；它不会替代高阶炼丹站或提供炼药桌节材奖励。木梁无实体碰撞，木顶位于高处并留有破口，两侧入口与六株灵草保留。旧世界不会自动增建；加载不会补种已收走的灵草，也不会修复拆除的丹房或瓶子。

生成检查天然地形、现有结构、物件、液体和机关，并要求足够已有地面支撑。药圃避开海岸边缘及附近上方已有水源；候选位置不安全时允许不生成。药圃位置不保证与主洞穴全程连通。

早期版本的小世界种子曾生成三处药圃，后续加强海岸与水源避让。第293轮用小世界种子20261020实测两处完整药圃及丹房，保存重载一致；原生引擎另已验证生态激活、种植帧、收获钩子与原版邻近制作站判定。实际客户端穿行/采收、多人、更多种子与尺寸及经济平衡仍待验收。
