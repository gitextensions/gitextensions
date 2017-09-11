using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;
using GitUI.UserControls;
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
                    case BuildInfo.BuildStatus.InProgress:
                        buildStatusImage = Resources.Settings;
                        break;
                    case BuildInfo.BuildStatus.Unstable:
                        buildStatusImage = Resources.IconMixed;
                        break;
                    case BuildInfo.BuildStatus.Stopped:
                        buildStatusImage = Resources.BuildCancelled;
                        break;
                }

                if (buildStatusImage != null)
                {
                    e.Graphics.DrawImage(buildStatusImage, new Rectangle(e.CellBounds.Left, e.CellBounds.Top + 4, 16, 16));
                }
            }
        }

        public static void BuildStatusMessageCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, Color foreColor, Font rowFont)
        {
            if (revision.BuildStatus != null)
            {
                var buildStatusForeColor = foreColor;

                switch (revision.BuildStatus.Status)
                {
                    case BuildInfo.BuildStatus.Success:
                        buildStatusForeColor = Color.DarkGreen;
                        break;
                    case BuildInfo.BuildStatus.Failure:
                        buildStatusForeColor = Color.DarkRed;
                        break;
                    case BuildInfo.BuildStatus.InProgress:
                        buildStatusForeColor = Color.Blue;
                        break;
                    case BuildInfo.BuildStatus.Unstable:
                        buildStatusForeColor = Color.OrangeRed;
                        break;
                    case BuildInfo.BuildStatus.Stopped:
                        buildStatusForeColor = Color.Gray;
                        break;
                }

                var text = (string)e.FormattedValue;
                var rect = RevisionGridUtils.GetCellRectangle(e);
                RevisionGridUtils.DrawColumnText(e.Graphics, text, rowFont, buildStatusForeColor, rect);
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