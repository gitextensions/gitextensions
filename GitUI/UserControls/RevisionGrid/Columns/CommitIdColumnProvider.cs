using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class CommitIdColumnProvider : ColumnProvider
    {
        private readonly Dictionary<Font, int[]> _widthByLengthByFont = new(capacity: 4);
        private readonly RevisionGridControl _grid;
        private int? _charCount = null;
        private readonly int _maxWidth = TextRenderer.MeasureText(GitRevision.WorkTreeGuid, AppSettings.MonospaceFont).Width;

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
                MinimumWidth = DpiUtil.Scale(32),
                Visible = AppSettings.ShowObjectIdColumn
            };
        }

        public override void ApplySettings()
        {
            Column.Visible = AppSettings.ShowObjectIdColumn;
        }

        private int GetCharLengthForColumnWidth(int width)
        {
            Font monospaceFont = AppSettings.MonospaceFont;
            if (!_widthByLengthByFont.TryGetValue(monospaceFont, out int[] widthByLength))
            {
                widthByLength = Enumerable.Range(0, ObjectId.Sha1CharCount + 1).Select(c => TextRenderer.MeasureText(new string('8', c), monospaceFont).Width).ToArray();

                _widthByLengthByFont[monospaceFont] = widthByLength;
            }

            int i = Array.FindIndex(widthByLength, w => w > width);

            if (i == -1 && width >= widthByLength[^1])
            {
                return ObjectId.Sha1CharCount;
            }
            else if (i > 1)
            {
                return i - 1;
            }

            return 0;
        }

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style)
        {
            if (string.IsNullOrWhiteSpace(e.FormattedValue as string))
            {
                return;
            }

            _grid.DrawColumnText(e, (string)e.FormattedValue, style.MonospaceFont, style.ForeColor, e.CellBounds, useEllipsis: false);
        }

        public override void OnColumnWidthChanged(DataGridViewColumnEventArgs e)
        {
            _charCount = GetCharLengthForColumnWidth(e.Column.Width);
            if (e.Column.Width > _maxWidth && Column.DataGridView != null)
            {
                // Enforce from outside the current method because it is not allowed (exception thrown...)
                Task.Run(async () =>
                {
                    await Column.DataGridView!.SwitchToMainThreadAsync();
                    e.Column.Width = _maxWidth;
                });
            }
        }

        public override void OnCellFormatting(DataGridViewCellFormattingEventArgs e, GitRevision revision)
        {
            // Set the grid cell's accessibility text
            if (!revision.IsArtificial)
            {
                if (!_charCount.HasValue)
                {
                    _charCount = GetCharLengthForColumnWidth(Column.Width);
                }

                if (_charCount > 0)
                {
                    e.Value = revision.ObjectId.ToShortString(_charCount.Value);
                }
            }
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
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
