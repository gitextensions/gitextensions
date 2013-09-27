using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;
using GitUIPluginInterfaces.BuildServerIntegration;

namespace GitUI.BuildServerIntegration
{
    static internal class BuildInfoDrawingLogic
    {
        public static void BuildStatusImageColumnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, Brush foreBrush, Font rowFont)
        {
            if (revision.BuildStatus != null)
            {
                Image buildStatusImage = null;

                switch (revision.BuildStatus.Status)
                {
                    case BuildInfo.BuildStatus.Success:
                        buildStatusImage = Resources.BuildSuccessful;
                        break;
                    case BuildInfo.BuildStatus.Failure:
                        buildStatusImage = Resources.BuildFailed;
                        break;
                    case BuildInfo.BuildStatus.Unknown:
                        buildStatusImage = Resources.BuildCancelled;
                        break;
                }

                if (buildStatusImage != null)
                {
                    e.Graphics.DrawImage(buildStatusImage, new Rectangle(e.CellBounds.Left, e.CellBounds.Top + 4, 16, 16));
                }
            }
        }

        public static void BuildStatusMessageCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, Brush foreBrush, Font rowFont)
        {
            if (revision.BuildStatus != null)
            {
                Brush buildStatusForebrush = foreBrush;

                switch (revision.BuildStatus.Status)
                {
                    case BuildInfo.BuildStatus.Success:
                        buildStatusForebrush = Brushes.DarkGreen;
                        break;
                    case BuildInfo.BuildStatus.Failure:
                        buildStatusForebrush = Brushes.DarkRed;
                        break;
                }

                var text = (string)e.FormattedValue;
                e.Graphics.DrawString(text, rowFont, buildStatusForebrush, new PointF(e.CellBounds.Left, e.CellBounds.Top + 4));
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

        private static string GetBuildStatusMessageText(GitRevision revision)
        {
            if (revision.BuildStatus == null || string.IsNullOrEmpty(revision.BuildStatus.Description))
                return string.Empty;
            return revision.BuildStatus.Description;
        }
    }
}