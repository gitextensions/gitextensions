# 收藏仓库状态总览

该魔改在原 Dashboard 中增加面向几十个本地仓库的状态总览。应用启动默认进入总览，左侧“回到传统视图”可切回官方仓库列表；总览只纳入收藏仓库。

## 当前行为

- 使用 Git Extensions 原生文件浏览器风格的分组 Tile 视图；仓库按收藏分类分组，未分类固定在最后。
- 每个仓库显示项目图标、分支、工作区状态、同步标签、检查时间，以及“相对时间（本地绝对时间）”格式的上次 Fetch 时间。
- 同步标签区分已同步、领先、落后、已分叉、未设置上游和分离 HEAD；工作区状态与同步状态是不同概念。
- 搜索覆盖仓库名、路径、分类和分支，并保留分组。搜索期间临时展开匹配分组且禁用排序。
- 支持组内仓库排序和分组排序：鼠标拖动或 `Alt+Up/Down`；不允许跨组移动仓库，未分类组不可移出末尾。折叠状态及顺序持久化；工具栏可重置排序。
- `Enter` 打开仓库，`F5` 检查选中仓库。工具栏支持检查或 Fetch 选中/全部收藏仓库。
- 本地状态检查不访问网络；Fetch 总是处理目标仓库配置的全部远端。

## 后台 Fetch

默认启用。当系统空闲满 5 分钟后执行一次；持续空闲时每 30 分钟再次执行。默认最多并发处理 4 个仓库，单仓库超时 120 秒。这些值可在“多仓库状态”设置页调整。

仓库正处于 merge、rebase、cherry-pick、revert、bisect 或存在 `index.lock` 时跳过 Fetch。调度与 UI 生命周期分离，即使当前显示传统视图也可继续运行；所有 UI 更新回到 UI 线程。

## 状态与错误语义

- 状态由本地 `git status`、分支及上游引用计算；ahead/behind 反映本地已有远端跟踪引用，不代表服务器上的实时状态。
- 本地检查成功不会抹除最近一次 Fetch 错误；Fetch 成功才清除 Fetch 错误。
- Fetch 错误与本地检查错误分别保留，界面优先呈现 Fetch 错误。
- 相对时间每分钟仅重绘文本，不执行 Git 命令。

## 配置与本地数据

常规设置沿用 Git Extensions 的全局设置存储，键前缀为 `multirepositorystatus.*`：自动 Fetch 开关、空闲阈值、Fetch 周期、并发数和超时。

以下可再生成数据位于 `AppSettings.LocalApplicationDataPath`：

- `MultiRepositoryStatusCache.json`：收藏仓库的最近状态和 Fetch 时间，用于启动时即时显示。
- `MultiRepositoryStatusLayout.json`：分组顺序、组内仓库顺序及折叠状态。

文件损坏、缺失或不可写不会阻止总览使用；缓存仅是本地派生数据，不应提交到仓库。

## 代码与验证入口

主要代码集中在：

- `src/app/GitUI/CommandsDialogs/BrowseDialog/DashboardControl/MultiRepositoryStatus*.cs`
- `src/app/GitUI/CommandsDialogs/BrowseDialog/DashboardControl/Dashboard.cs`
- `src/app/GitCommands/Settings/AppSettings.cs`
- `src/app/GitUI/CommandsDialogs/SettingsDialog/Pages/MultiRepositoryStatusSettingsPage.cs`

针对性测试：

```powershell
dotnet test tests/app/UnitTests/GitUI.Tests/GitUI.Tests.csproj -c Release --filter "FullyQualifiedName~MultiRepository"
```

完整构建：

```powershell
dotnet build GitExtensions.slnx -c Release
```
