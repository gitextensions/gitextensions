using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class BuildStatusColumnProvider : ColumnProvider
    {
        private readonly RevisionGridControl _grid;
        private readonly Func<GitModule> _module;

        public BuildStatusColumnProvider(RevisionGridControl grid, Func<GitModule> module)
            : base("Build Status")
        {
            _grid = grid;
            _module = module;

            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "Build Status",
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                FillWeight = 50
            };
        }

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, (Brush backBrush, Color backColor, Color foreColor, Font normalFont, Font boldFont) style)
        {
            if (revision.BuildStatus == null)
            {
                return;
            }

            var size = DpiUtil.Scale(new Size(8, 8));

            var location = new Point(
                e.CellBounds.Left + (size.Width / 2),
                e.CellBounds.Top + ((e.CellBounds.Height - size.Height) / 2));

            var container = e.Graphics.BeginContainer();
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(GetBrush(), new Rectangle(location, size));
            e.Graphics.EndContainer(container);

            if (_module().EffectiveSettings.BuildServer.ShowBuildSummaryInGrid.ValueOrDefault)
            {
                _grid.DrawColumnText(
                    e,
                    (string)e.FormattedValue,
                    style.normalFont,
                    GetColor(),
                    bounds: e.CellBounds.ReduceLeft(size.Width * 2));
            }

            Color GetColor()
            {
                var isSelected = _grid.Graph.Rows[e.RowIndex].Selected;

                switch (revision.BuildStatus.Status)
                {
                    case BuildInfo.BuildStatus.Success:
                        return isSelected ? Color.LightGreen : Color.DarkGreen;
                    case BuildInfo.BuildStatus.Failure:
                        return isSelected ? Color.Red : Color.DarkRed;
                    case BuildInfo.BuildStatus.InProgress:
                        return isSelected ? Color.LightBlue : Color.Blue;
                    case BuildInfo.BuildStatus.Unstable:
                        return Color.OrangeRed;
                    case BuildInfo.BuildStatus.Stopped:
                        return isSelected ? Color.LightGray : Color.Gray;
                    case BuildInfo.BuildStatus.Unknown:
                        return style.foreColor;
                    default:
                        throw new InvalidOperationException("Unsupported build status enum value.");
                }
            }

            Brush GetBrush()
            {
                switch (revision.BuildStatus.Status)
                {
                    case BuildInfo.BuildStatus.Success:
                        return Brushes.LightGreen;
                    case BuildInfo.BuildStatus.Failure:
                        return Brushes.Red;
                    case BuildInfo.BuildStatus.InProgress:
                        return Brushes.DodgerBlue;
                    case BuildInfo.BuildStatus.Unstable:
                        return Brushes.DarkOrange;
                    case BuildInfo.BuildStatus.Stopped:
                    case BuildInfo.BuildStatus.Unknown:
                        return Brushes.Gray;
                    default:
                        throw new InvalidOperationException("Unsupported build status enum value.");
                }
            }
        }

        public override void OnCellFormatting(DataGridViewCellFormattingEventArgs e, GitRevision revision)
        {
            e.Value = !string.IsNullOrEmpty(revision.BuildStatus?.Description)
                ? revision.BuildStatus.Description
                : "";
            e.FormattingApplied = true;
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, out string toolTip)
        {
            toolTip = revision.Guid;
            return true;
        }
    }
}