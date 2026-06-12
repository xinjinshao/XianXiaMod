# 天碑卫
[返回敌怪总览](../Overview.md)
## 定位
- ID: heaven_tablet_guard | 阶段: Post-Golem | 生态: [坠天宫阙](../../Biomes/Entries/Fallen_Heaven_Palace.md)
## 数值
| 属性 | 值 |
|------|-----|
| 生命 | 1500 | 伤害 | 92 | 防御 | 54 | 击退抗性 | 85% | AI | Shield walker |
## 行为
- 静止时防御提升至62。每160tick发起举盾推进(防御82，持续180tick，向玩家缓慢移动，每45tick释放碑文弹)。接近玩家(<48px)造成击退。
## 掉落
| 物品 | 概率 | 数量 |
|------|------|------|
| 天道碎片 | 50% | 1-2 |
| 器胚碎片 | 25% | 1-2 |
## 美术
- 64x80，碑甲卫士，盾像小天碑。Prompt: jade tablet shield guardian, golden decree armor, crisp pixel silhouette
## 代码实现
- ✅ 数值对齐 | ✅ AI（举盾推进+碑文弹） | ✅ 掉落表
