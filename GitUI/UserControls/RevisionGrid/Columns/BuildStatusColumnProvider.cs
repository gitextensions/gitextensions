using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Settings;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using GitUIPluginInterfaces.Settings;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class BuildStatusColumnProvider : ColumnProvider
    {
        private const int IconColumnWidth = 16;
        private const int TextColumnWidth = 150;

        private readonly RevisionGridControl _grid;
        private readonly RevisionDataGridView _gridView;
        private readonly Func<GitModule> _module;

        public BuildStatusColumnProvider(RevisionGridControl grid, RevisionDataGridView gridView, Func<GitModule> module)
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

        public override void Refresh(int rowHeight, in VisibleRowRange range)
        {
            var showIcon = AppSettings.ShowBuildStatusIconColumn;
            var showText = AppSettings.ShowBuildStatusTextColumn;

            IBuildServerSettings buildServerSettings = _module().GetEffectiveSettings()
                .BuildServer();

            var columnVisible = buildServerSettings.EnableIntegration
                && (showIcon || showText);

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

                var iconColumnWidth = DpiUtil.Scale(IconColumnWidth);

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

            Size size;

            if (AppSettings.ShowBuildStatusIconColumn)
            {
                size = DpiUtil.Scale(new Size(8, 8));

                var location = new Point(
                    e.CellBounds.Left + (size.Width / 2),
                    e.CellBounds.Top + ((e.CellBounds.Height - size.Height) / 2));

                var container = e.Graphics.BeginContainer();
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var brush = CreateCircleBrush();
                e.Graphics.FillEllipse(brush, new Rectangle(location, size));
                e.Graphics.EndContainer(container);
            }
            else
            {
                size = default;
            }

            if (AppSettings.ShowBuildStatusTextColumn)
            {
                _grid.DrawColumnText(
                    e,
                    (string)e.FormattedValue,
                    style.NormalFont,
                    GetColor(style.ForeColor),
                    bounds: e.CellBounds.ReduceLeft(size.Width * 2));
            }

            Color GetColor(Color foreColor)
            {
                var isSelected = _gridView.Rows[e.RowIndex].Selected;

                Color customColor;
                switch (revision.BuildStatus.Status)
                {
                    case BuildInfo.BuildStatus.Unknown:
                        return foreColor;

                    case BuildInfo.BuildStatus.Success:
                        customColor = isSelected ? Color.LightGreen : Color.DarkGreen;
                        break;
                    case BuildInfo.BuildStatus.Failure:
                        customColor = isSelected ? Color.Red : Color.DarkRed;
                        break;
                    case BuildInfo.BuildStatus.InProgress:
                        customColor = isSelected ? Color.LightBlue : Color.Blue;
                        break;
                    case BuildInfo.BuildStatus.Unstable:
                        customColor = Color.OrangeRed;
                        break;
                    case BuildInfo.BuildStatus.Stopped:
                        customColor = isSelected ? Color.LightGray : Color.Gray;
                        break;

                    default:
                        throw new InvalidOperationException("Unsupported build status enum value.");
                }

                return customColor.AdaptTextColor();
            }

            Brush CreateCircleBrush()
            {
                Color color;
                switch (revision.BuildStatus.Status)
                {
                    case BuildInfo.BuildStatus.Success:
                        color = Color.LightGreen;
                        break;
                    case BuildInfo.BuildStatus.Failure:
                        color = Color.Red;
                        break;
                    case BuildInfo.BuildStatus.InProgress:
                        color = Color.DodgerBlue;
                        break;
                    case BuildInfo.BuildStatus.Unstable:
                        color = Color.DarkOrange;
                        break;
                    case BuildInfo.BuildStatus.Stopped:
                    case BuildInfo.BuildStatus.Unknown:
                        color = Color.Gray;
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported build status enum value.");
                }

                return new SolidBrush(color.AdaptBackColor());
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
