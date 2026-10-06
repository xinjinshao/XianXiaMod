# 自动源码检查

本地安装 Python 与 .NET 8 SDK 后运行：

```powershell
python Tools/run_source_checks.py
```

也可通过 `--dotnet <SDK可执行文件>` 指定已有SDK。入口按顺序执行两组Python范围回归、10项经济图分析回归、8项生成器归属回归与只读输出新鲜度、内容契约、本地化键、PNG检查与20个Release源码回归项目；任何子命令失败立即返回非零状态，不继续后续检查。不要把输出中的模拟引擎边界当成实机验收。

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


## 生成器归属与手写代码保护

`generate_tmod_content.py`只维护`Localization/generated/{zh-Hans,en-US}.hjson`及`Localization/generated_bestiary/{zh-Hans,en-US}.hjson`四份输出。其余Common/Content源码、贴图和本地化都是手写维护，包括名字中带Generated/HandGenerated的现有文件；不能按文件名推断可以覆盖。数据表仍服务于生成本地化，历史玩法模板保留作参考。

旧generate_materials/projectiles/tiles/biomes/enemies/bosses/summons入口和copy_asset已禁用，调用会在任何写入前报错；通用write仅允许四份输出，解析真实目标路径后检查归属，拒绝目录穿越或重定向到手写位置。需要修改玩法时直接编辑当前实现和行为回归。生成输出统一UTF-8/LF。

```powershell
python Tools/generate_tmod_content.py
python Tools/verify_generated_localization.py
python Tools/Tests/test_generator_ownership.py
```

CI运行只读新鲜度检查，生成结果变化时须显式更新这四份输出并评审内容；空格式行差异不作为过期，但实际值差异会失败。8项隔离回归执行真实主入口并保护手写哨兵、检查旧入口/资产复制拒绝、路径边界、换行/重复运行、只读校验状态恢复及失败传播。校验不写工作区，不重新生成玩法文件。


## GitHub 原生构建与加载 CI

`.github/workflows/native-checks.yml`在push、pull_request和手动触发时独立运行Windows原生检查。下载[官方v2026.08.3.0发布包](https://github.com/tModLoader/tModLoader/releases/tag/v2026.08.3.0)，固定SHA256为`61e865f3702b12ce4a26c5a90b9de99a12c65ffc228455eb3390ce54af1eab15`，校验后解压到runner临时目录；不使用latest、不依赖Steam、本地安装或仓库内引擎副本。升级时必须同时审核版本/摘要和原生回归。

流程调用既有verify_build.ps1完成编译、隔离打包、专服内容与配方加载及经济导出，再执行Gameplay编译产物回归、LocalizationPaths官方加载器路径回归和三价格场景金币转换检查。任何非零退出均失败；经济阶段要求唯一快照，避免读错旧结果。源码28步检查保留在独立Source checks工作流。

无论成功失败，上传专服标准/错误日志、引擎详细日志、经济快照/报告及生成模组包，保留14天，支持定位加载前失败。引擎下载或摘要校验失败也会阻止通过。此流程不启动游戏世界，不验证图形、多人实战、Boss平衡或完整通关；包仅为诊断产物，不自动发布。


## 原生包内容与默认配置

Native checks另运行PackageContents，使用官方TmodFile打开实际.tmod，核对内部名称、build.txt版本、归档哈希、当前描述及Common/Content/Localization的所有PNG/HJSON（接受官方转换后的rawimg）；拒绝隐藏/越界条目、Assets/Docs/Wiki/Tools/bin/obj/README和源码/工具扩展。它不是读取源码文件名后假定打包正确。默认配置在Gameplay编译产物回归中验证，当前累计308条。

```powershell
dotnet run --project Tools/Tests/PackageContents/PackageContents.csproj -- <XianXia.tmod路径> <官方引擎目录> <仓库根目录>
```

buildIgnore同时保护普通ModSources构建；verify_build的源目录白名单仍保留。该检查不证明所有资源实际显示效果、素材权属或完整存档迁移，详见[安装与升级说明](../Docs/INSTALL_AND_UPGRADE.md)。
