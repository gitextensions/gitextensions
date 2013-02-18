using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;

namespace GitUI.RevisionGridClasses
{
    static internal class BuildInfoDrawingLogic
    {
        public static void RevisionsCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, Brush foreBrush, Font rowFont)
        {
            switch (e.ColumnIndex)
            {
                case 4:
                    {
                        if (revision.BuildStatus != null)
                        {
                            Image buildStatusImage = null;

                            switch (revision.BuildStatus.Status)
                            {
                                case BuildInfo.BuildStatus.Success:
                                    buildStatusImage = Resources.IconClean;
                                    break;
                                case BuildInfo.BuildStatus.Failure:
                                    buildStatusImage = Resources.error;
                                    break;
                                case BuildInfo.BuildStatus.Unknown:
                                    buildStatusImage = Resources.Conflict;
                                    break;
                            }

                            if (buildStatusImage != null)
                            {
                                e.Graphics.DrawImage(buildStatusImage, new Rectangle(e.CellBounds.Left, e.CellBounds.Top + 4, 16, 16));
                            }
                        }
                    }
                    break;
                case 5:
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
                    break;
            }
        }

        public static void RevisionsCellFormatting(DataGridViewCellFormattingEventArgs e, GitRevision revision)
        {
            switch (e.ColumnIndex)
            {
                case 4:
                    e.FormattingApplied = false;
                    break;
                case 5:
                    e.Value = revision.BuildStatus != null
                                  ? revision.BuildStatus.Description
                                  : String.Empty;
                    break;
            }
        }
    }
}