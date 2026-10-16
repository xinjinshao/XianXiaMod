

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | ID | 尺寸 |
| --- | --- | --- |
| <img src="../../../../Assets/Final/low_grade_spirit_stone/low_grade_spirit_stone__item_icon__v01.png" alt="下品灵石" width="48"> | low_grade_spirit_stone | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 详见 [物品总览](../Overview.md) 美术素材表。
- Prompt：Terraria pixel art item icon, xianxia theme, transparent background.

# 下品灵石

[返回物品总览](../Overview.md)

## 定位

- 英文 ID：`low_grade_spirit_stone`
- 分类：材料
- 阶段：Pre-Boss

## 来源

- 浅层灵脉采矿
- 游灵史莱姆掉落（25%）
- 各NPC商店出售

## 用途

- 引气符合成
- 多数配方的基础材料
- 通用炼制材料

## 代码实现

- ✅ 物品类完整
- ✅ 配方/掉落实现

## 地下金箱补给

开启模组世界生成的新世界中，完整的普通、未上锁地下金箱有25%机会成为补给候选；入选后在空槽额外获得4–8枚灵石。每世界最多补充 `max(1, 世界宽度/420取整)` 个箱子（小/中/大世界10/15/20个），不会替换原版或其它模组的战利品。锁箱、特殊箱、满箱和已有灵石的箱子跳过；旧世界不会自动补箱，关闭模组世界生成时不添加。

所有合格金箱都会先参与25%的候选抽取，候选超出世界上限时再随机保留；不会因宝箱数组位置较后而失去抽取机会，同一个箱对象不会重复参与。
