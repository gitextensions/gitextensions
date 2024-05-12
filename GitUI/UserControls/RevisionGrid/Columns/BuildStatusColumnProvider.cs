using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class BuildStatusColumnProvider : ColumnProvider
    {
        private const int IconColumnWidth = 16;
        private const int TextColumnWidth = 150;

        private readonly RevisionGridControl _grid;
        private readonly RevisionDataGridView _gridView;
        private readonly Func<IGitModule> _module;

        // Increase contrast to selected rows
        private readonly Color _lightBlue = Color.FromArgb(130, 180, 240);
        private Font? _fontWithUnicodeCache = null;

        public BuildStatusColumnProvider(RevisionGridControl grid, RevisionDataGridView gridView, Func<IGitModule> module)
            : base("Build Status")
        {
            _grid = grid;
            _gridView = gridView;
            _module = module;

            Column = new DataGridViewTextBoxColumn
            {
                HeaderText = "Build Status",
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Width = DpiUtil.Scale(TextColumnWidth)
            };
        }

        public override void ApplySettings()
        {
            bool showIcon = AppSettings.ShowBuildStatusIconColumn;
            bool showText = AppSettings.ShowBuildStatusTextColumn;

            IBuildServerSettings buildServerSettings = _module().GetEffectiveSettings().GetBuildServerSettings();
            bool columnVisible = buildServerSettings.IntegrationEnabledOrDefault && (showIcon || showText);

            Column.Visible = columnVisible;

            if (columnVisible)
            {
                UpdateWidth();
            }

            return;

            void UpdateWidth()
            {
                Column.Resizable = showText ? DataGridViewTriState.True : DataGridViewTriState.False;
                Column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

                int iconColumnWidth = DpiUtil.Scale(IconColumnWidth);

                if (showIcon && !showText)
                {
                    Column.Width = iconColumnWidth;
                }
                else if (showText && Column.Width == iconColumnWidth)
                {
                    Column.Width = DpiUtil.Scale(TextColumnWidth);
                }
            }
        }

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style)
        {
            if (revision.BuildStatus is null)
            {
                return;
            }

            string text = (AppSettings.ShowBuildStatusIconColumn ? revision.BuildStatus.StatusSymbol : string.Empty)
                + (AppSettings.ShowBuildStatusTextColumn ? (string)e.FormattedValue : string.Empty);

            if (_fontWithUnicodeCache?.Size != style.NormalFont.Size)
            {
                _fontWithUnicodeCache = new Font(FontFamily.GenericMonospace, style.NormalFont.Size);
            }

            _grid.DrawColumnText(e, text, _fontWithUnicodeCache, GetColor(style.ForeColor), bounds: e.CellBounds);

            Color GetColor(Color foreColor)
            {
                bool isSelected = _gridView.Rows[e.RowIndex].Selected;

                Color customColor;
                switch (revision.BuildStatus.Status)
                {
                    case BuildStatus.Unknown:
                        return foreColor;

                    case BuildStatus.Success:
                        customColor = isSelected ? Color.LightGreen : Color.DarkGreen;
                        break;
                    case BuildStatus.Failure:
                        customColor = isSelected ? Color.Red : Color.DarkRed;
                        break;
                    case BuildStatus.InProgress:
                        customColor = isSelected ? _lightBlue : Color.Blue;
                        break;
                    case BuildStatus.Unstable:
                        customColor = Color.OrangeRed;
                        break;
                    case BuildStatus.Stopped:
                    default:
                        customColor = isSelected ? Color.LightGray : Color.Gray;
                        break;
                }

                return customColor.AdaptTextColor();
            }
        }

        public override void OnCellFormatting(DataGridViewCellFormattingEventArgs e, GitRevision revision)
        {
            e.Value = !string.IsNullOrEmpty(revision.BuildStatus?.Description)
                ? revision.BuildStatus.Description
                : "";
            e.FormattingApplied = true;
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
        {
            if (revision.BuildStatus is not null)
            {
                toolTip = revision.BuildStatus.Tooltip ?? revision.BuildStatus.Description;
                return toolTip is not null;
            }

            return base.TryGetToolTip(e, revision, out toolTip);
        }
    }
}
