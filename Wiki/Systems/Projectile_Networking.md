# 玩家所属弹体的服务器同步

灵气武器发射事务与部分子弹幕由服务器生成，但保留真实玩家owner，以沿用原生职业、击中和玩家归属。tModLoader原生netUpdate只对所有者生效；服务器创建owner为玩家的弹体需要显式广播，不能仅设置netUpdate后假定已同步。

ServerPlayerProjectileSync仅处理本模组、服务器端、owner在玩家范围内且不等于Main.myPlayer的弹体：

- OnSpawn发原生SyncProjectile，使用whoAmI槽位；初始弹体参数、AI与ExtraAI由原版序列化。
- PostAI在netUpdate脏标志为true时广播状态，并清除标志；不会每帧广播未变化弹体。
- OnKill发原生KillProjectile，使用identity与owner，覆盖死亡、断线、回收及提前清理。

客户端、单人、原版弹体、其它Mod及服务器自身所属弹体保留原生路径，不改变弹药/库存或NPC伤害的信任模型。天碑御印原独立销毁广播已移到统一hook，避免同一次清理发送两遍。

青木法阵、雷符法阵与药王法阵设置netImportant，供原生迟加入同步；两旧法阵生成模板同步此默认值，未重生成文件。正常持续时间、资源成本与部署上限不变。

依据：[官方Projectile文档](https://docs.tmodloader.net/docs/stable/class_projectile.html)的netUpdate/netImportant说明、[官方NewProjectile补丁](https://github.com/tModLoader/tModLoader/blob/stable/patches/tModLoader/Terraria/Projectile.cs.patch)的所有者自动发送条件。已通过源码边界回归、实际编译产物默认值及专服加载。尚需双客户端验证初始创建、弹跳/回收状态、销毁时序、迟加入、ExtraAI内容与实际网络负载；不能以模拟消息调用次数代替原生联机验收。
