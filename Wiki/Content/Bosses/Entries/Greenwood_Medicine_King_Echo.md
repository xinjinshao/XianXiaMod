

## 当前美术素材

<!-- ART_SECTION:entry-art:START -->

| 素材 | 名称 | ID | 类型 | 尺寸 |
| --- | --- | --- | --- | --- |
| <img src="../../../../Assets/Final/greenwood_medicine_king_echo/greenwood_medicine_king_echo__body__v01.png" alt="青木药王残影 body" width="96"> | 青木药王残影 | `greenwood_medicine_king_echo` | `body` | 144x144 |
| <img src="../../../../Assets/Final/greenwood_medicine_king_echo/greenwood_medicine_king_echo__boss_head__v01.png" alt="青木药王残影 boss_head" width="96"> | 青木药王残影 | `greenwood_medicine_king_echo` | `boss_head` | 32x32 |

<!-- ART_SECTION:entry-art:END -->

## 美术资源

- 主体：144x144，老者残影、青铜药鼎、背后药枝光轮。
- 动画：`cast` 6 帧，`cauldron` 4 帧，`absorb` 5 帧。
- 头像：32x32，药鼎和木纹面容。
- 场地物件：治疗花 24x24，毒花 24x24。
- Prompt 重点：`ancient herbal alchemist echo, bronze cauldron, green wood halo, Terraria boss sprite`。

# 青木药王残影

[返回 Boss 总览](../Overview.md)

## 定位

- 英文 ID：`greenwood_medicine_king_echo`
- 阶段：Post-Plantera
- 所属线：青木药宗
- 角色：高阶炼丹与生命系装备 Boss。

## 召唤

完成药宗残卷链后，在青木药园深处使用药王印。

## 战斗设计

- 阶段一：药鼎投掷丹火和灵草弹。
- 阶段二：场地生成治疗花和毒花，玩家需区分。
- 阶段三：药王残影吸收场地植物，强化下一轮攻击。
- 核心考点：场地管理和目标优先级。

## 掉落

- 高阶丹炉。
- 药王木心。
- 元婴灵胎材料。
- 生命系饰品升级件。

## 剧情

药王在坠天之夜尝试以众生药性修补灵脉，失败后残影仍在重复配方。

## 当前恢复机制与验收范围

第143轮第二阶段起有恢复仪式：每240帧开始90帧绿色读条，期间对Boss造成累计最大生命1%的伤害即可打断。换目标也会打断；读条成功恢复最多1%生命，每场最多三次，生命不会超过上限。成功或打断后进入60帧无接触伤害恢复，读条与恢复时暂停其它招式。原无前摇加速已取消。

普通攻击、法阵和召唤藤灵已有实现；第144轮召唤按三只成功配额，失败下轮补缺额、击败不补刷；来源撤场和召唤预警仍待完善。早期设计中的治疗花与毒花不代表已按该方案实现。

源码组合11139项、实际编译1766项、构建/打包/专服加载通过。绿色读条实际显示、真实多人运输、治疗反制阈值和四职业战斗时长仍待实机验收。

第144轮藤灵创建继承Boss目标并同步。新增64项源码边界回归，完整创建/联机运输、地形安全与来源清理尚需后续验证。
