# Duke Edition Repository Guide

修改本仓库前先读 [CONTEXT.md](CONTEXT.md)，统一使用其中的官方基线、Duke 版和魔改等术语。

- 涉及 Duke 版功能、官方同步、版本或发布时，读 [docs/customizations/README.md](docs/customizations/README.md)。
- 修改仓库状态总览时，读 [docs/customizations/multi-repository-status-overview.md](docs/customizations/multi-repository-status-overview.md)，并核对其中列出的行为边界和验证入口。
- 比较魔改时，以文档记录的确定官方基线为起点；远端分支的新提交只有在实际吸收并完成构建验证后才成为新基线。
- 将重要、难以逆转且存在真实取舍的决定写入 `docs/adr/`；其他值得保留的魔改知识简洁写入对应的 `docs/customizations/` 功能文档。
- Git 网络操作使用命令级空代理覆盖：`git -c http.proxy= -c https.proxy= ...`。
