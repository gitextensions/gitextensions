using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class CommitIdColumnProvider : ColumnProvider
    {
        private readonly Dictionary<Font, int[]> _widthByLengthByFont = new Dictionary<Font, int[]>(capacity: 4);
        private readonly RevisionGridControl _grid;

        /// <summary>
        /// fallback value for the property Length in the case that this column has not been shown
        /// </summary>
        public const int DefaultLength = 7;

        /// <summary>
        /// the current length of the commit hashs displayed
        /// </summary>
        public int Length { get; private set; } = DefaultLength;

        public CommitIdColumnProvider(RevisionGridControl grid)
            : base("Commit ID")
        {
            _grid = grid;

            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                HeaderText = "Commit ID",
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Resizable = DataGridViewTriState.True,
                Width = DpiUtil.Scale(60),
                MinimumWidth = DpiUtil.Scale(32)
            };
        }

        public override void Refresh(int rowHeight, in VisibleRowRange range) => Column.Visible = AppSettings.ShowObjectIdColumn;

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style)
        {
            var monospaceFont = style.MonospaceFont;
            if (!_widthByLengthByFont.TryGetValue(monospaceFont, out var widthByLength))
            {
                widthByLength = Enumerable.Range(0, ObjectId.Sha1CharCount + 1).Select(c => TextRenderer.MeasureText(new string('8', c), monospaceFont).Width).ToArray();

                _widthByLengthByFont[monospaceFont] = widthByLength;
            }

            if (!revision.IsArtificial)
            {
                var i = Array.FindIndex(widthByLength, w => w > Column.Width);

                if (i == -1 && Column.Width > widthByLength[widthByLength.Length - 1])
                {
                    string objectId = revision.ObjectId.ToString();
                    Length = objectId.Length;
                    _grid.DrawColumnText(e, objectId, monospaceFont, style.ForeColor, e.CellBounds, useEllipsis: false);
                }
                else if (i > 1)
                {
                    Length = i - 1;
                    _grid.DrawColumnText(e, revision.ObjectId.ToShortString(Length), monospaceFont, style.ForeColor, e.CellBounds, useEllipsis: false);
                }
            }
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, out string toolTip)
        {
            if (revision.ObjectId.IsArtificial)
            {
                toolTip = default;
                return false;
            }

            toolTip = revision.Guid;
            return true;
        }
    }
}