# 自动源码检查

本地安装 Python 与 .NET 8 SDK 后运行：

```powershell
python Tools/run_source_checks.py
```

也可通过 `--dotnet <SDK可执行文件>` 指定已有SDK。入口按顺序执行两组Python范围回归、10项经济图分析回归、内容契约、本地化键、PNG检查与19个Release源码回归项目；任何子命令失败立即返回非零状态，不继续后续检查。不要把输出中的模拟引擎边界当成实机验收。

`.github/workflows/source-checks.yml` 在push、pull_request和手动触发时运行同一入口，Windows runner使用Python 3.12和.NET 8；仅需仓库读取权限，相同分支的新运行取消旧运行。

这个工作流不下载游戏引擎，不构建发行模组、不运行专服/真实世界生成或客户端。Gameplay和LocalizationPaths依赖已编译模组/官方tModLoader安装，保留为本地原生验证；完整打包/专服加载仍运行 `Tools/verify_build.ps1`。联网、图形客户端、难度/人数矩阵与通关继续单独验收。PNG检查识别明确的原版ItemID/ProjectileID贴图引用，名称有效性需原生构建/加载验证。

## 可选依赖原生加载

隔离目录中可使用已有的官方依赖包，与本模组一同加载，不修改用户安装的Mods目录：

```powershell
Tools/verify_build.ps1 -TModLoaderDir <引擎目录> -DotNetPath <SDK路径> -AdditionalModPaths <BossChecklist.tmod路径> -AdditionalModDisplayNames @{BossChecklist='Boss Checklist'}
```

启用列表只包含本次明确传入的包；脚本核对所有传入模组的添加/完成阶段并复制引擎详细日志到该轮`tmodloader-server.log`。显示名和内部名不同时用映射指定；此外需检查详细日志的注册成功记录，加载成功不等于客户端或兼容玩法验收。参考包不提交进仓库，来源和版本写在审查记录。


## 原生制作与金币经济审计

需要本地官方引擎，主动开启导出；默认打包/加载不写审计快照。导出仅读取注册与独立Player的原生价格，不开店、不消耗库存。不能与`-SkipServerLoad`并用。

```powershell
Tools/verify_build.ps1 -TModLoaderDir <引擎目录> -DotNetPath <SDK路径> -ExportEconomyAudit
python Tools/audit_economy.py <输出的economy-audit.json路径> --report Docs/ECONOMY_REGISTRATION_AUDIT_2026-10-06.md --check
```

`--check`对任何本模组正收益金币转换候选返回非零；候选仍需核对真实开店条件和操作。成本传播包含配方组、多步配方和批量输出；特殊货币/自定义价被排除，条件视为开放，随机奖励/前缀、原购物品退款、种植/掉落/钓鱼/微光/旅途和联机经济不在模型内。原生价格使用0.75最高快乐度场景（[官方ShopHelper常量](https://docs.tmodloader.net/docs/preview/class_shop_helper.html)），折扣场景同时设置独立角色的discountEquipped/discountAvailable，再读取GetItemExpectedPrice，避免自行假定买卖价比例。

源码CI包含`test_economy_audit.py`的10个图分析行为测试；实际注册导出和引擎价格检查仍需上述专服流程。本次原生数据报告见[制作经济注册审计](../Docs/ECONOMY_REGISTRATION_AUDIT_2026-10-06.md)，它不代表完整经济平衡或可获得性验收。
