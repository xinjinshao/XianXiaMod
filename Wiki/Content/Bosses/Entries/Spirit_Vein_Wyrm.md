

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/spirit_vein_wyrm/spirit_vein_wyrm__boss_head__v01.png" alt="灵脉蠕虫 boss_head" width="96"> | 灵脉蠕虫 | `spirit_vein_wyrm` | `boss_head` | 32x32 |
| <img src="../../../../Assets/Final/spirit_vein_wyrm/spirit_vein_wyrm__head__v01.png" alt="灵脉蠕虫 head" width="96"> | 灵脉蠕虫 | `spirit_vein_wyrm` | `head` | 64x64 |
| <img src="../../../../Assets/Final/spirit_vein_wyrm/spirit_vein_wyrm__body__v01.png" alt="灵脉蠕虫 body" width="96"> | 灵脉蠕虫 | `spirit_vein_wyrm` | `body` | 64x64 |
| <img src="../../../../Assets/Final/spirit_vein_wyrm/spirit_vein_wyrm__tail__v01.png" alt="灵脉蠕虫 tail" width="96"> | 灵脉蠕虫 | `spirit_vein_wyrm` | `tail` | 64x48 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- **分段架构：** 穿墙蠕虫型，由 head / body / tail 三个独立段拼接，共 6-8 体节。
- **头段 (head)：** 64×64，圆形玉色头部，明显口器（深绿裂口），头顶一对短触角。深绿外轮廓 + 青玉内发光。`move` 6 帧（纵向排列）。
- **体段 (body)：** 64×64，重复段。深绿外壳、青玉发光核心（半透明感用硬边高光表达）、腹面浅色纹理。`move` 6 帧。每段之间 2px 衔接间距。
- **尾段 (tail)：** 64×48，锥形收束，青玉光从核心向尾尖渐隐，尾尖略上翘。`move` 6 帧。
- **头像：** 32×32，突出圆形头部和玉色口器，地图图标。
- **投射物：** 灵气尘 16×16，浅青粒子，不要烟雾糊边。由体段周期性释放，飘向玩家方向。
- **Prompt 重点：** 头段 `small jade spirit wyrm head, round mouth, antennae, dark green outline, inner cyan glow, side-view Terraria worm boss`。体段 `repeating wyrm body segment, jade glowing core, dark green carapace, segmented worm, Terraria pixel art`。尾段 `wyrm tail segment, tapered jade tip, fading cyan glow, side-view`。

# 灵脉蠕虫

[返回 Boss 总览](../Overview.md) | [整体进度](../../../Progression/Overview.md)

## 定位

- 英文 ID：`spirit_vein_wyrm`
- 阶段：Pre-Boss
- 所属线：浅层灵脉
- 角色：第一个 Mod Boss，用于确认玩家已经接触灵气系统。

## 召唤

在[浅层灵脉](../../Biomes/Entries/Shallow_Spirit_Veins.md)使用 `灵脉香` 召唤。灵脉香由下品灵石、灵气凝胶和普通凝胶制作。

## 战斗设计

- **分段结构：** head ×1 + body ×6-8 + tail ×1，共 8-10 节。每节为独立 NPC，通过蠕虫 AI 串联。
- **阶段一：** 地下穿行，头部以正弦波移动（转弯半径 ~120px，转弯角 45-60°）。每 3-4 秒直线冲刺一次（冲刺速度 ×1.5，体节拉直跟随头部轨迹）。
- **阶段二：** 生命低于 50% 后，主体断裂为 2-3 条独立小虫。每条小虫保留 head×1 + body×3 + tail×1 结构。小虫各自追踪玩家，AI 相同但伤害和生命降低。
- **碰撞逻辑：** 头部受伤全额扣血；体段受伤减伤 40%（仅该段损血）；尾段减伤 60%。
- **核心考点：** 跳跃和平台移动，不要求高机动装备。玩家须在虫体环绕时找到空隙输出头部。
- **多人注意：** 所有段由服务端生成和同步。分裂时服务端生成新虫段，客户端只播放粒子效果。避免客户端重复分裂。

## 掉落

- 下品灵核：引气到凝气的早期材料。
- 灵脉鳞片：制作木纹飞剑升级件。
- 灵气凝胶：灵气药剂和低阶符箓。
- 灵脉香配方：首次击败后提示。

## 剧情

它不是妖兽，而是一截被唤醒的灵脉。击败它后，世界承认玩家可以接触灵气。

## 代码实现

- ✅ 数值与wiki对齐（HP/伤害/防御）
- ✅ 独特阶段AI机制
- ✅ 6层掉落表（主/次/灵石/灵胶/法器碎片/稀有装饰）
- ✅ 专家/大师难度缩放
- ✅ Boss召唤校验（境界+前置+场地+时间）
