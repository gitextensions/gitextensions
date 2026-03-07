# UI Design Guidelines

Guidelines for creating and updating WinForms dialogs in Git Extensions.
See [#6183](https://github.com/gitextensions/gitextensions/issues/6183) for background and examples.

## Dialog base class

All dialogs **must** inherit from `GitExtensionsDialog`, which provides:

| Panel | Purpose | Designer defaults |
|---|---|---|
| `MainPanel` | Content area (white/themed background) | `Dock = Fill`, `Padding = new Padding(12)` |
| `ControlsPanel` | Bottom button bar (light grey background) | `Dock = Bottom`, `FlowDirection = RightToLeft`, `Padding = new Padding(5)`, `MinimumSize = (0, 32)` |

A thin separator line is drawn automatically at the top of `ControlsPanel`.

## Overall layout

1. Place a single root `TableLayoutPanel` (conventionally named `tpnlMain` or `tableLayout`) inside `MainPanel`.
2. The root `TableLayoutPanel` should be:
   - `Dock = DockStyle.Fill`
   - `Margin = new Padding(0)` (the outer padding comes from `MainPanel`)
   - `AutoSize = true` and `AutoSizeMode = GrowAndShrink` when the dialog itself is auto-sized
3. Use nested `TableLayoutPanel` or `FlowLayoutPanel` controls for sub-sections; avoid absolute positioning.

## Column patterns

For label + input + browse-button rows, use a **three-column** `TableLayoutPanel`:

| Column | `ColumnStyle` | Contents |
|---|---|---|
| 0 | `AutoSize` | Label (`AutoSize = true`, `TextAlign = MiddleLeft`, `Dock = Left`) |
| 1 | `Percent(100)` | Input control (`Anchor = Left \| Right` or `Dock = Fill`) |
| 2 | `Absolute(100–120)` | Browse button (`Anchor = Right`) |

When there is no browse button, use a **two-column** layout:

| Column | `ColumnStyle` | Contents |
|---|---|---|
| 0 | `AutoSize` | Label |
| 1 | `Percent(100)` | Input control |

## GroupBoxes

- Use `GroupBox` to visually group related options (e.g. "Repository type", "Pull from", "Local changes").
- `GroupBox` should span all columns of its parent `TableLayoutPanel` (use `SetColumnSpan`).
- Prefer `AutoSize = true` and `AutoSizeMode = GrowAndShrink` on `GroupBox` controls.
- Inner content of a `GroupBox` is typically a `FlowLayoutPanel` with `Dock = Fill`.
- Use `Padding = new Padding(8)` inside the `GroupBox` for inner spacing.

## Radio buttons and checkboxes

- Place radio buttons inside a `FlowLayoutPanel` within their `GroupBox`.
- The `FlowLayoutPanel` should use `Dock = Fill`, `AutoSize = true`, `AutoSizeMode = GrowAndShrink`.
- Add small inner padding on the `FlowLayoutPanel` (e.g. `Padding = new Padding(9, 4, 9, 4)` or `Padding = new Padding(8, 0, 0, 0)`) to visually indent from the `GroupBox` border.
- Standalone checkboxes (not inside a `GroupBox`) are placed in their own `TableLayoutPanel` row, typically in the input column (`Dock = Fill`).

## Action buttons (`ControlsPanel`)

- The primary action button (e.g. "Clone", "Create branch", "Pull") goes into `ControlsPanel`.
- Set `AcceptButton` on the form to the primary action button.
- Button properties:
  - `AutoSize = true`, `AutoSizeMode = GrowAndShrink`
  - `MinimumSize = new Size(75, 23)` (ensures a reasonable minimum width)
- When a button includes an icon, set `ImageAlign = ContentAlignment.MiddleLeft` (or `TopLeft`) and `TextImageRelation = ImageBeforeText`.
- Secondary buttons (e.g. "Stash", "Mergetool", "Load SSH Key") also go in `ControlsPanel` and flow to the left of the primary button.
- Use the ampersand (`&`) prefix for keyboard accelerators in button text (e.g. `"&Clone"`, `"&Create branch"`).

## Browse buttons

- Use the `FolderBrowserButton` user control or a standard `Button` with the `Properties.Images.BrowseFileExplorer` icon.
- Anchor to the right of the row: `Anchor = AnchorStyles.Right`.
- Set `ImageAlign = ContentAlignment.MiddleLeft` and show text such as `"&Browse"` or `"B&rowse"`.

## Labels

- `AutoSize = true`
- `TextAlign = ContentAlignment.MiddleLeft`
- `Dock = DockStyle.Fill` or `Dock = DockStyle.Left` (vertically centers the label in the row)
- `Margin = new Padding(3)` (default)

## Input controls (TextBox, ComboBox)

- Stretch to fill: `Anchor = AnchorStyles.Left | AnchorStyles.Right` or `Dock = DockStyle.Fill`
- Never hard-code widths for text inputs; let the `TableLayoutPanel` column sizing handle it.

## DPI and scaling

- Set `AutoScaleDimensions = new SizeF(96F, 96F)` and `AutoScaleMode = AutoScaleMode.Dpi` on the form.
- Never use fixed pixel sizes that assume 96 DPI for margins or control dimensions at runtime; use `DpiUtil.Scale` helpers when computing sizes in code.
- The designer records sizes at 96 DPI; `AutoScaleMode.Dpi` handles the rest.

## Form properties

All dialogs should set the following in the designer:

```text
AutoScaleDimensions = new SizeF(96F, 96F)
AutoScaleMode = AutoScaleMode.Dpi
MaximizeBox = false
MinimizeBox = false
StartPosition = FormStartPosition.CenterParent
HelpButton = true                               // when a manual section exists
ManualSectionAnchorName = "..."                  // link to readthedocs
ManualSectionSubfolder = "..."
```

- Set `MinimumSize` to prevent the dialog from being resized too small.
- Set `FormBorderStyle = FormBorderStyle.FixedDialog` for simple dialogs that should not be resizable.
- Use `AutoSize = true` on the form when its content determines the size (e.g. `FormInit`).

## Theme compatibility

- Layouts must look correct in both **light** and **dark** themes.
- Do not hard-code colors; rely on `SystemColors`, `AppColor` theme colors, and the theming infrastructure.
- Test dialogs in both system app modes (light and dark) before submitting.

## Naming conventions

| Element | Prefix | Convention | Example |
|---|---|---|---|
| Label | `lbl` | `lbl` + descriptive name | `lblMainText` |
| Button | `btn` | `btn` + action name | `btnAccept` |
| TextBox | `txt` | `txt` + descriptive name | `txtBranchName` |
| ComboBox | `cbx` | `cbx` + descriptive name | `cbxOrders` |
| CheckBox | `chk` | `chk` + descriptive name | `chkOpenWorktree` |
| RadioButton | `rb` | `rb` + descriptive name | `rbCheckoutExisting` |
| GroupBox | `gbx` | `gbx` + descriptive name | `gbxOrderDetails` |
| TableLayoutPanel | `tpnl` | `tpnl` + descriptive name | `tpnlMain` |
| FlowLayoutPanel | `flpnl` | `flpnl` + descriptive name | `flpnlLocalOptions` |
| Non-translated controls | `_NO_TRANSLATE_` | `_NO_TRANSLATE_` + name | `_NO_TRANSLATE_Remotes` |

## Checklist for updating a dialog

1. Inherit from `GitExtensionsDialog`.
2. Place content in `MainPanel` via a root `TableLayoutPanel`.
3. Place action buttons in `ControlsPanel`.
4. Use `TableLayoutPanel` grid for label/input alignment.
5. Group related options in `GroupBox` controls.
6. Set `AutoScaleMode = Dpi` and `AutoScaleDimensions = (96, 96)`.
7. Set `StartPosition = CenterParent`, disable maximize/minimize.
8. Avoid hard-coded colors; verify light and dark themes.
9. Set keyboard accelerators on buttons and labels.
10. Set `AcceptButton` to the primary action button.
