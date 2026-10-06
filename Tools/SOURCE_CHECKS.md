# 自动源码检查

本地安装 Python 与 .NET 8 SDK 后运行：

```powershell
python Tools/run_source_checks.py
```

也可通过 `--dotnet <SDK可执行文件>` 指定已有SDK。入口按顺序执行两组Python范围回归、内容契约、本地化键、PNG检查与17个Release源码回归项目；任何子命令失败立即返回非零状态，不继续后续检查。不要把输出中的模拟引擎边界当成实机验收。

`.github/workflows/source-checks.yml` 在push、pull_request和手动触发时运行同一入口，Windows runner使用Python 3.12和.NET 8；仅需仓库读取权限，相同分支的新运行取消旧运行。

这个工作流不下载游戏引擎，不构建发行模组、不运行专服/真实世界生成或客户端。Gameplay和LocalizationPaths依赖已编译模组/官方tModLoader安装，保留为本地原生验证；完整打包/专服加载仍运行 `Tools/verify_build.ps1`。联网、图形客户端、难度/人数矩阵与通关继续单独验收。PNG检查识别明确的原版ItemID/ProjectileID贴图引用，名称有效性需原生构建/加载验证。
