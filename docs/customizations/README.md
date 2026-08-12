# Duke 版魔改索引

Git Extensions Duke Edition 是基于官方 Git Extensions 的个人发行线。本文档只索引当前有效的魔改；代码细节以 Git diff 为准，重要取舍见 [ADR](../adr/)。

## 当前版本与官方基线

| 项目 | 当前值 |
| --- | --- |
| Duke 版版本 | `7.2.0-duke.1` |
| Windows 文件版本 | `7.2.0.1` |
| 官方跟踪分支 | `origin/release/7.0` |
| 已吸收官方版本 | `v7.2.0` |
| 官方基线提交 | `501f831ed25127e4a301b7649d5d4e6524f53bba` |
| 首个功能实现快照 | `e472d8413b648b8b5a9610ccb4e7bade54286b8d` |

当前代码差异可用以下命令复现：

```powershell
git diff --stat 501f831e...HEAD
git diff 501f831e...HEAD
git log --reverse --oneline 501f831e..HEAD
```

官方基线只在 Duke 版实际吸收新的官方提交并完成构建验证后推进；单纯更新远端引用不改变基线。

## 当前魔改

- [收藏仓库状态总览](multi-repository-status-overview.md)：首页集中检查收藏仓库、展示同步状态，并在系统空闲时 Fetch 全部远端。

## 维护方式

- 功能文档描述当前行为和边界，不重复可由 diff 直接获得的逐行实现，也不维护版本流水账。
- 普通但值得保留的魔改知识写入对应功能文档。
- 重要、难以逆转且存在真实取舍的决定写入 ADR；失效决策通过 ADR 状态和 Git 历史追踪。
- Duke 版本采用“官方版本 + `duke` 修订号”：同一官方基线递增修订号，吸收新的官方版本后从 `.1` 重新开始。Git 标签采用 `v<版本>`，例如 `v7.2.0-duke.1`。
