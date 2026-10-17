# 灾厄材料互通

日期：2026-10-17，第244轮。原有独立主线保持可用；灾厄为可选软引用，不引入灾厄程序集类型依赖。

启用服务端配置 `EnableSoftCompatibilityHooks` 且加载 `CalamityMod` 时，在炼丹炉注册以下三个独立配方，均要求当前世界处于困难模式：

| 灾厄材料内部名 | 消耗 | 下品灵石产出 |
| --- | --- | --- |
| EssenceofEleum | 1 | 8 |
| EssenceofHavoc | 1 | 8 |
| EssenceofSunlight | 1 | 8 |

材料名从官方1.4.4分支核对：[EssenceofEleum](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Items/Materials/EssenceofEleum.cs)、[EssenceofHavoc](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Items/Materials/EssenceofHavoc.cs)、[EssenceofSunlight](https://github.com/CalamityTeam/CalamityModPublic/blob/1.4.4/Items/Materials/EssenceofSunlight.cs)。这不是对所有灾厄版本的支持承诺。

配置关闭或没有灾厄时不注册这些配方。材料逐个通过 `TryFind<ModItem>` 解析，缺少某种材料时只跳过对应配方并写日志；不存在有效材料则不注册。不会替换灾厄或原有配方，不绕过本模组突破/Boss门槛。配置变更需要重新加载模组。

第245轮补充游戏内来源：悬停下品灵石可查看实际成功注册的材料及8枚产出说明，材料名称由当前语言读取。关闭兼容、无依赖或无有效材料时不显示；缺项不会列入提示。注册时缓存物品类型，读取提示不重复查询依赖或注册配方，卸载清空缓存。

证据：直接链接生产代码的20项回归覆盖关闭开关、依赖缺失、三材料完整、逐个缺失、全部缺失、当前世界条件和卸载清理。完整源码入口现为37步、27个.NET项目，已通过。官方无灾厄构建/打包/专服加载run-62496f14212348bdb87818694bbafc42通过；无灾厄注册审计16942项，summons-14d1e226889548708dd1436591bbc416。

现有环境没有实际加载灾厄：有依赖分支的上述测试使用模拟模组/配方注册边界，不能代替真实灾厄客户端、最终注册配方、联机、完整经济图和进度平衡验收。原生审计已提供有依赖时检查实际配方的分支，尚未在本轮执行该分支。R29保持未完成。

第246轮：隔离注册审计工具支持重复传入 `--extra-mod "内部名称=完整tmod路径"`，例如 `--extra-mod "CalamityMod=C:\Mods\CalamityMod.tmod"`，其运行依赖也逐一显式传入。工具只将包复制到临时Mods目录并生成启用列表，检查原依赖文件哈希；不会搜索或修改玩家安装目录。此入口不意味着全部既有战斗审计已适配第三方模组的全局数值修改，真实灾厄环境和可能需要的专用审计范围仍待验证。

本轮全量检查另外暴露新护甲的继承式纹理路径无法被PNG检查器识别，已改为六件装备逐一显式声明相同原版纹理。没有关闭检查或添加假图片，1243张PNG检查通过，护甲属性不变。
