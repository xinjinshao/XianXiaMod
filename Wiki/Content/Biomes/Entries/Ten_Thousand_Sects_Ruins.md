

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/sect_ruin_brick/sect_ruin_brick__tile__v01.png" alt="宗门石砖 tile" width="64"> | 宗门石砖 | `sect_ruin_brick` | `tile` | 16x16 |
| <img src="../../../../Assets/Final/sword_tablet/sword_tablet__object__v05.png" alt="剑碑 object" width="64"> | 剑碑 | `sword_tablet` | `object` | 32x48 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- Tile：`sect_ruin_brick`，16x16，灰白石砖、淡金裂纹。
- 装饰：`sword_tablet`，32x48，石碑和插剑剪影。
- 背景墙：藏经阁木墙 16x16，书格图案要简化。
- Prompt 重点：`ancient cultivation sect ruins, jade stone bricks, sword tablet, Terraria pixel art`。

# 万宗遗址

[返回生态总览](../Overview.md)

## 概念

万宗遗址是 Post-Plantera 后开启的中后期内容中心。它把宗门残卷、职业分化和试炼 Boss 聚合起来。

## 生成

- 阶段：Post-Plantera。
- 位置：地牢深处、地下大结构或特殊入口。
- 结构：藏经阁、剑碑、试炼台、破损山门。
- 解锁：击败世纪之花后，残卷书灵提示入口。

## 内容

- 敌怪：执念剑修、藏经残影。
- Boss：[无相剑魄](../../Bosses/Entries/Formless_Sword_Soul.md)、[青木药王残影](../../Bosses/Entries/Greenwood_Medicine_King_Echo.md)。
- 资源：宗门令、残卷页、断剑残意。
- NPC：[残卷书灵](../../NPCs/Entries/Archive_Scroll_Spirit.md)。

## 代码实现

- ✅ 世界生成骨架（Tile铺设+物件放置）
- ✅ 敌怪生成池（权重与wiki对齐）
- ✅ 环境效果（星渊污染/雷泽落雷/月骨灵压等）
- ✅ 判定阈值（tile count ≥ wiki指定值）

剑碑附近提供抗劫增益。多人由服务器向主人端同步，持续30帧、剩余15帧时续期；离开范围后最多保留半秒，拆除物件另受最多15帧的附近扫描缓存影响。已有更长的抗劫增益不会被缩短，重复剑碑不叠加效果。


## 遗迹厅堂

启用世界生成的新世界会尝试在遗迹矿斑附近放置可步入的厅堂，两侧留有入口，平整地面设有剑碑。厅堂本身提供足够的遗迹砖激活生态；敌怪与Boss仍遵守原有世界进度。

生成会避开受保护结构、建筑、液体与机关；无法找到安全位置时跳过，剑碑仍使用原有生态内放置方式。旧世界不会自动增建，加载世界也不会修复玩家拆除的部分。新厅堂会尝试沿较短的安全路线，用最多48格长、内部5格高的通道接入附近已有天然洞穴；没有安全路线时跳过。局部连接不保证主洞穴或出生点全程可达，可能仍需挖掘探索。新生成厅堂中央还会尝试放置一只补给木箱：6–10下品灵石、2–4器胚碎片、1–2回忆药水和10火把。箱子使用原版资源，不会在取空或重载后自动补充；无法放置时跳过。更广的连通道路和装饰仍待完善。
