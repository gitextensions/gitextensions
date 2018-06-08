using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class CommitIdColumnProvider : ColumnProvider
    {
        private readonly RevisionGridControl _grid;

        public CommitIdColumnProvider(RevisionGridControl grid)
            : base("Commit ID")
        {
            _grid = grid;

            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "Commit ID",
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                FillWeight = 20,
                Resizable = DataGridViewTriState.True,
                MinimumWidth = 16
            };
        }

        public override void Refresh() => Column.Visible = AppSettings.ShowIds;

        private readonly Dictionary<Font, int[]> _widthByLengthByFont = new Dictionary<Font, int[]>(capacity: 4);

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, (Brush backBrush, Color backColor, Color foreColor, Font normalFont, Font boldFont) style)
        {
            if (!_widthByLengthByFont.TryGetValue(style.normalFont, out var widthByLength))
            {
                widthByLength = Enumerable.Range(0, ObjectId.Sha1CharCount).Select(c => TextRenderer.MeasureText(new string('8', c), style.normalFont).Width).ToArray();

                _widthByLengthByFont[style.normalFont] = widthByLength;
            }

            if (!revision.IsArtificial)
            {
                var i = Array.FindIndex(widthByLength, w => w > Column.Width);

                if (i > 1 && revision.ObjectId != null)
                {
                    _grid.DrawColumnText(e, revision.ObjectId.ToShortString(i - 1), style.normalFont, style.foreColor, e.CellBounds, useEllipsis: false);
                }
            }
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, out string toolTip)
        {
            if (revision.ObjectId?.IsArtificial == true)
            {
                toolTip = default;
                return false;
            }

            toolTip = revision.Guid;
            return true;
        }
    }
}