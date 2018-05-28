using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.UserControls;
using GitUIPluginInterfaces.BuildServerIntegration;
using JetBrains.Annotations;

namespace GitUI.BuildServerIntegration
{
    internal static class BuildInfoDrawingLogic
    {
        public static void BuildStatusImageColumnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision)
        {
            if (revision.BuildStatus == null)
            {
                return;
            }

            var size = DpiUtil.Scale(new Size(8, 8));

            var location = new Point(
                e.CellBounds.Left + ((e.CellBounds.Width - size.Width) / 2),
                e.CellBounds.Top + ((e.CellBounds.Height - size.Height) / 2));

            var container = e.Graphics.BeginContainer();
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(GetBrush(), new Rectangle(location, size));
            e.Graphics.EndContainer(container);

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

        public static void BuildStatusMessageCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, Color foreColor, Font rowFont, bool isSelected, RevisionGrid revisions)
        {
            if (revision.BuildStatus == null)
            {
                return;
            }

            var color = GetColor();
            var text = (string)e.FormattedValue;
            revisions.DrawColumnText(e, text, rowFont, color);

            Color GetColor()
            {
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
                        return foreColor;
                    default:
                        throw new InvalidOperationException("Unsupported build status enum value.");
                }
            }
        }

        public static void BuildStatusImageColumnCellFormatting(DataGridViewCellFormattingEventArgs e, DataGridView grid, GitRevision revision)
        {
            e.FormattingApplied = false;
            var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];
            cell.ToolTipText = GetBuildStatusMessageText(revision);
        }

        public static void BuildStatusMessageCellFormatting(DataGridViewCellFormattingEventArgs e, GitRevision revision)
        {
            e.Value = GetBuildStatusMessageText(revision);
        }

        [NotNull]
        private static string GetBuildStatusMessageText([NotNull] GitRevision revision)
        {
            if (string.IsNullOrEmpty(revision.BuildStatus?.Description))
            {
                return string.Empty;
            }

            return revision.BuildStatus.Description;
        }
    }
}