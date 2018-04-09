using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;
using GitUI.UserControls;
using GitUIPluginInterfaces.BuildServerIntegration;
using JetBrains.Annotations;

namespace GitUI.BuildServerIntegration
{
    internal static class BuildInfoDrawingLogic
    {
        private static readonly Dictionary<BuildInfo.BuildStatus, Image> _imageByBuildStatus;

        static BuildInfoDrawingLogic()
        {
            _imageByBuildStatus = new Dictionary<BuildInfo.BuildStatus, Image>
            {
                { BuildInfo.BuildStatus.Success, DpiUtil.Scale(Resources.BuildSuccessful) },
                { BuildInfo.BuildStatus.Failure, DpiUtil.Scale(Resources.BuildFailed) },
                { BuildInfo.BuildStatus.Unknown, DpiUtil.Scale(Resources.BuildCancelled) },
                { BuildInfo.BuildStatus.InProgress, DpiUtil.Scale(Resources.Settings) },
                { BuildInfo.BuildStatus.Unstable, DpiUtil.Scale(Resources.IconMixed) },
                { BuildInfo.BuildStatus.Stopped, DpiUtil.Scale(Resources.BuildCancelled) },
            };
        }

        public static void BuildStatusImageColumnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision)
        {
            if (revision.BuildStatus == null)
            {
                return;
            }

            if (_imageByBuildStatus.TryGetValue(revision.BuildStatus.Status, out var image))
            {
                var size = DpiUtil.Scale(new Size(16, 16));
                var location = new Point(e.CellBounds.Left, e.CellBounds.Top + ((e.CellBounds.Height - size.Height) / 2));
                e.Graphics.DrawImage(image, new Rectangle(location, size));
            }
        }

        public static void BuildStatusMessageCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, Color foreColor, Font rowFont)
        {
            if (revision.BuildStatus == null)
            {
                return;
            }

            var color = GetColor();
            var text = (string)e.FormattedValue;
            var rect = RevisionGridUtils.GetCellRectangle(e);
            RevisionGridUtils.DrawColumnText(e.Graphics, text, rowFont, color, rect);

            Color GetColor()
            {
                switch (revision.BuildStatus.Status)
                {
                    case BuildInfo.BuildStatus.Success:
                        return Color.DarkGreen;
                    case BuildInfo.BuildStatus.Failure:
                        return Color.DarkRed;
                    case BuildInfo.BuildStatus.InProgress:
                        return Color.Blue;
                    case BuildInfo.BuildStatus.Unstable:
                        return Color.OrangeRed;
                    case BuildInfo.BuildStatus.Stopped:
                        return Color.Gray;
                    default:
                        return foreColor;
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