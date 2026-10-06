# 自动源码检查

本地安装 Python 与 .NET 8 SDK 后运行：

```powershell
python Tools/run_source_checks.py
```

也可通过 `--dotnet <SDK可执行文件>` 指定已有SDK。入口按顺序执行两组Python范围回归、内容契约、本地化键、PNG检查与18个Release源码回归项目；任何子命令失败立即返回非零状态，不继续后续检查。不要把输出中的模拟引擎边界当成实机验收。

`.github/workflows/source-checks.yml` 在push、pull_request和手动触发时运行同一入口，Windows runner使用Python 3.12和.NET 8；仅需仓库读取权限，相同分支的新运行取消旧运行。

这个工作流不下载游戏引擎，不构建发行模组、不运行专服/真实世界生成或客户端。Gameplay和LocalizationPaths依赖已编译模组/官方tModLoader安装，保留为本地原生验证；完整打包/专服加载仍运行 `Tools/verify_build.ps1`。联网、图形客户端、难度/人数矩阵与通关继续单独验收。PNG检查识别明确的原版ItemID/ProjectileID贴图引用，名称有效性需原生构建/加载验证。

## 可选依赖原生加载

隔离目录中可使用已有的官方依赖包，与本模组一同加载，不修改用户安装的Mods目录：

```powershell
Tools/verify_build.ps1 -TModLoaderDir <引擎目录> -DotNetPath <SDK路径> -AdditionalModPaths <BossChecklist.tmod路径> -AdditionalModDisplayNames @{BossChecklist='Boss Checklist'}
```

启用列表只包含本次明确传入的包；脚本核对所有传入模组的添加/完成阶段并复制引擎详细日志到该轮`tmodloader-server.log`。显示名和内部名不同时用映射指定；此外需检查详细日志的注册成功记录，加载成功不等于客户端或兼容玩法验收。参考包不提交进仓库，来源和版本写在审查记录。
