# 机制详细设计

[返回机制总览](Overview.md)

## 灵气资源

### 基础规则

- 初始无灵气条，使用引气符并完成引气后解锁。
- 灵气上限随境界提升，不直接等同魔力。
- 灵气恢复受环境、饰品、丹药影响。
- 在星渊区域使用星灾装备会更快积累灵压。

### 建议数值

| 境界 | 灵气上限 | 基础恢复 | 说明 |
| --- | --- | --- | --- |
| 引气 | 40 | 1/秒 | 只支持低阶符箓 |
| 凝气 | 80 | 2/秒 | 飞剑玩法成型 |
| 筑基 | 120 | 3/秒 | 法宝槽开启 |
| 金丹 | 180 | 4/秒 | 高阶灵气技能 |
| 元婴 | 240 | 5/秒 | 分身/召唤强化 |
| 化神 | 320 | 6/秒 | 天道系装备 |
| 渡劫 | 420 | 7/秒 | 终局技能 |
| 斩道 | 500 | 8/秒 | 路线奖励 |

### UI 美术

- 灵气条：宽 160，高 12，青玉渐变，深色边框。
- 灵压警告：32x32 Buff 图标，青色气旋转为紫色裂纹。
- Prompt：`small jade spiritual energy UI icon, cyan swirl, crisp pixel art, no text`

### 灵气消耗规格

| 动作 | 消耗 | 冷却 | 备注 |
| --- | --- | --- | --- |
| 基础飞剑释放 | 4-8 灵气 | 使用时间决定 | 早期核心循环 |
| 中阶飞剑释放 | 9-16 灵气 | 使用时间决定 | Hardmode 后 |
| 高阶法旨 | 32-45 灵气 | 1.5 秒软冷却 | 高消耗爆发 |
| 阵盘部署 | 16-28 灵气 | 8 秒 | 同类阵盘最多 1 个 |
| 法宝主动 | 20-60 灵气 | 15-45 秒 | 依装备等级 |
| 境界突破 | 当前上限 100% | 无 | 触发准备窗口 |

## 境界突破

### 结构

```text
收集材料 -> 使用突破丹 -> 准备窗口 -> 天劫/试炼 -> 成功突破 -> 解锁新配方
```

### 失败处理

- 不降境界。
- 消耗突破丹。
- 返还少量普通材料。
- 给予虚弱 Debuff 3 到 5 分钟。
- 保留一次“劫后感悟”，下次挑战小幅降低难度。

### 美术

- 境界图标：每阶 32x32，使用玉环层数表达，不写文字。
- 突破特效：青色竖光，外圈符纹，硬边像素粒子。

## 天劫难度

### 触发等级

- 小天劫：筑基、金丹。
- 心魔劫：元婴。
- 天碑劫：化神。
- 斩道劫：终局。

### 参数草案

| 类型 | 持续 | 主要威胁 | 奖励 |
| --- | --- | --- | --- |
| 小天劫 | 45 秒 | 落雷、雷链 | 突破、劫云露 |
| 心魔劫 | 60 秒 | 玩家镜像 | 元婴材料 |
| 天碑劫 | Boss 战 | 印记、审判光柱 | 天道碎片 |
| 斩道劫 | Boss 战 | 空间压缩、归档锁 | 终局路线 |

### 美术

- 落雷预警：地面 16x4 闪蓝线，0.8 秒后雷柱落下。
- 雷柱：16x64 或 32x128，边缘分叉但核心清楚。
- 心魔影：玩家轮廓的黑青剪影，不复制玩家外观细节。

## Buff 与 Debuff 规格

| 状态 | ID | 类型 | 默认持续 | 效果 | 图标 |
| --- | --- | --- | --- | --- | --- |
| 回春丹效 | `spring_return_regen` | Buff | 90 秒 | +1 HP/秒 | 32x32 青丹和叶纹 |
| 聚气 | `qi_gathering` | Buff | 装备常驻 | +1 灵气/秒 | 32x32 青玉气旋 |
| 灵压紊乱 | `spiritual_pressure_disorder` | Debuff | 20 秒 | 生命恢复 -2 HP/秒，天劫积累 +20% | 32x32 紫裂气旋 |
| 抗劫 | `tribulation_resistance` | Buff | 180 秒 | 天劫伤害 -10%，雷伤 -8% | 32x32 紫玉盾 |
| 星渊侵蚀 | `star_abyss_corrosion` | Debuff | 8 秒 | 防御 -8，灵压增长 | 32x32 暗蓝星眼 |
| 归档锁 | `archive_lock` | Debuff | 6 秒 | 降低移动加速度，限制重复冲刺 | 32x32 金色环锁 |

## 冷却与免疫规格

| 机制 | 冷却 | 免疫/限制 |
| --- | --- | --- |
| 法宝主动 | 15-45 秒 | 同类法宝共享 5 秒全局冷却 |
| 阵盘部署 | 8 秒 | 同一玩家同类阵盘最多 1 个 |
| 天劫落雷 | 0.8 秒预警 | 命中后 20 ticks 雷劫局部免疫 |
| 星渊禁符 | 1 秒 | 每次使用灵压 +15 |
| 归档复制 | 1.5 秒延迟 | 不复制传送和坐骑动作 |

## UI 资源规格

| UI 资源 | ID | 尺寸 | 帧数 | 备注 |
| --- | --- | --- | --- | --- |
| 灵气条框 | `spiritual_energy_bar_frame` | 164x16 | 1 | 深色描边 |
| 灵气条填充 | `spiritual_energy_bar_fill` | 160x12 | 1 | 可横向裁切 |
| 灵压警告 | `pressure_warning_icon` | 32x32 | 4 | 闪烁动画 |
| 境界图标组 | `cultivation_tier_icons` | 32x32 each | 8 | 每境界一枚 |
| 法宝槽 | `artifact_slot_frame` | 40x40 | 1 | 玉色槽框 |
| 天劫预警线 | `tribulation_warning_line` | 16x4 | 2 | 可拉伸或重复 |

## 炼丹

### 品质

| 品质 | 结果 |
| --- | --- |
| 粗丹 | 效果较弱，材料保底产物 |
| 成丹 | 标准效果 |
| 良丹 | 效果或持续时间提高 |
| 灵丹 | 稀有高品质，可用于高级配方 |

### 炼丹 UI 美术

- 丹炉图标：48x48，青铜炉，药绿蒸汽，硬边像素。
- 丹药品质用边框颜色表达：灰、绿、蓝、金。
- 不使用复杂小字。

## 炼器

### 升级字段

- 胚器：基础形态。
- 铭刻：元素或机制标签。
- 淬炼：数值小幅提升。
- 觉醒：解锁主动效果。
- 道化：终局路线。

### 铭刻类型

| 铭刻 | 效果倾向 | 资源 |
| --- | --- | --- |
| 青木 | 恢复、持续、生命 | 青木根 |
| 玄炉 | 破甲、重击、火星 | 炉渣铁 |
| 雷泽 | 速度、连锁、抗劫 | 劫云露 |
| 星渊 | 高伤、副作用、污染 | 星蚀晶 |
| 残天 | 控制、审判、高消耗 | 天道碎片 |

### 美术

- 铭刻槽图标：16x16 小符号。
- 炼器台：64x48，铁砧、炉口、悬浮铭刻线。
- Prompt：`Terraria pixel crafting station, black iron anvil, small furnace, glowing talisman engraving lines`

## UI 与状态图标素材

<!-- ART_SECTION:systems-ui-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../Assets/Final/spiritual_energy_bar_frame/spiritual_energy_bar_frame__ui__v09.png" alt="灵气条框 ui" width="64"> | 灵气条框 | `spiritual_energy_bar_frame` | `ui` | 164x16 |
| <img src="../../Assets/Final/spiritual_energy_bar_fill/spiritual_energy_bar_fill__ui__v09.png" alt="灵气条填充 ui" width="64"> | 灵气条填充 | `spiritual_energy_bar_fill` | `ui` | 160x12 |
| <img src="../../Assets/Final/pressure_warning_icon/pressure_warning_icon__ui__v01.png" alt="灵压警告图标 ui" width="64"> | 灵压警告图标 | `pressure_warning_icon` | `ui` | 32x32 |
| <img src="../../Assets/Final/artifact_slot_frame/artifact_slot_frame__ui__v01.png" alt="法宝槽 ui" width="64"> | 法宝槽 | `artifact_slot_frame` | `ui` | 40x40 |
| <img src="../../Assets/Final/tribulation_warning_line/tribulation_warning_line__ui__v09.png" alt="天劫预警线 ui" width="64"> | 天劫预警线 | `tribulation_warning_line` | `ui` | 16x4 |

<!-- ART_SECTION:systems-ui-art:END -->

相关页面：

- [修行境界](Cultivation.md)
- [灵气](Spiritual_Energy.md)
- [天劫](Tribulation.md)
- [炼丹](Alchemy.md)
- [炼器](Refining.md)
