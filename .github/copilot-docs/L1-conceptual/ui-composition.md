<!-- L1 CONCEPTUAL. Agentic doc: TL;DR at top, Why→What→How, code pointers not code, ~100 lines. -->
# UI Composition (L1)

**TL;DR:** WinForms UI is built from a small class hierarchy. Base classes add translation,
theming, hotkeys, window-position memory, and access to `IGitUICommands`/`IGitModule`. The main
window `FormBrowse` is a `sealed partial` split across several files, hosting the commit graph
(`RevisionGridControl`), the left-panel object tree (`RepoObjectsTree`), commit details, menus,
and toolbars.

**Related:** [architecture-overview](architecture-overview.md) · [threading-model](threading-model.md) · [revision-grid-flow](../L3-flows/revision-grid-flow.md) · [L0 primer](../L0-foundations/gitextensions-primer.md)

## Why

Every dialog needs the same cross-cutting behavior: localised strings, theme colors, hotkeys,
DPI/position handling, and a handle to the current repo. Rather than repeat that, Git Extensions
puts it in base classes so each form/control focuses on its own job. Splitting the huge
`FormBrowse` into partials keeps concerns (grid, menus, commit details) navigable.

## What (form/control hierarchy)

| Base type | File | Adds |
| --- | --- | --- |
| `GitExtensionsFormBase` | [ResourceManager/GitExtensionsFormBase.cs](../../../src/app/ResourceManager/GitExtensionsFormBase.cs) | Translation, theming, hotkey plumbing. |
| `GitExtensionsForm` | [GitUI/GitExtensionsForm.cs](../../../src/app/GitUI/GitExtensionsForm.cs) | Window position restore, common form behavior. |
| `GitModuleForm` | [GitUI/GitModuleForm.cs](../../../src/app/GitUI/GitModuleForm.cs) | `UICommands` / `Module` access for repo-aware dialogs. |
| `GitModuleControl` | [GitUI/GitModuleControl.cs](../../../src/app/GitUI/GitModuleControl.cs) | UserControl base for repo-aware panels. |
| `GitExtensionsDialog` | [GitUI/GitExtensionsDialog.cs](../../../src/app/GitUI/GitExtensionsDialog.cs) | Standard dialog chrome (see UI design guidelines). |

Most dialogs derive from `GitModuleForm`; most panels from `GitModuleControl`.

## What (the main window)

`FormBrowse` ([FormBrowse.cs](../../../src/app/GitUI/CommandsDialogs/FormBrowse.cs)) is `sealed
partial`, split into:

- `FormBrowse.Designer.cs` — WinForms designer layout.
- `FormBrowse.InitRevisionGrid.cs` — wires up the `RevisionGridControl`.
- `FormBrowse.InitMenusAndToolbars.cs` — menus and toolbars.
- `FormBrowse.InitCommitDetails.cs` — commit info / diff panels.
- `FormBrowse.UpdateTargets.cs` — enable/disable UI based on state.

### Hosted controls

- **`RevisionGridControl`** — the commit graph.
  [RevisionGridControl.cs](../../../src/app/GitUI/UserControls/RevisionGrid/RevisionGridControl.cs).
- **`RepoObjectsTree`** — left panel of branches/tags/stashes/submodules/remotes.
  [RepoObjectsTree.cs](../../../src/app/GitUI/LeftPanel/RepoObjectsTree.cs). Tree nodes derive
  from `Node` / `BaseRevisionNode` ([Node.cs](../../../src/app/GitUI/LeftPanel/Node.cs)); concrete
  types include `LocalBranchNode`, `RemoteBranchNode`, `TagNode`, `StashNode`.

## How (add a new dialog)

1. Derive from `GitModuleForm` (repo-aware) or `GitExtensionsDialog`.
2. Follow control-naming in [ui_design_guidelines.md](../../ui_design_guidelines.md) (`btn`, `lbl`, `txt`, …).
3. Use `UICommands`/`Module` for git; never new up a `GitModule` yourself.
4. Add UI strings as `TranslationString` and run `update-loc.cmd` (see [translation-system](../L2-core-platform/translation-system.md)).

## Hard rules

- New dialogs **MUST** inherit from `GitExtensionsDialog` (or an existing base), not raw `Form`.
- Follow the WinForms control-naming conventions in `ui_design_guidelines.md`.
- Touch controls only on the UI thread ([threading-model](threading-model.md)).

**Next:** jump to [L2 subsystems](../L2-core-platform/docs-index.md) or an [L3 flow](../L3-flows/docs-index.md).
