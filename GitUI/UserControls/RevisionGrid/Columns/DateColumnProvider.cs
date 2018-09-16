using System;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class DateColumnProvider : ColumnProvider
    {
        private readonly RevisionGridControl _grid;

        public DateColumnProvider(RevisionGridControl grid)
            : base("Date")
        {
            _grid = grid;

            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                HeaderText = "Date",
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Width = DpiUtil.Scale(130),
                MinimumWidth = DpiUtil.Scale(25)
            };
        }

        public override void Refresh(int rowHeight, in VisibleRowRange range) => Column.Visible = AppSettings.ShowDateColumn;

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style)
        {
            _grid.DrawColumnText(e, e.FormattedValue.ToString(), style.NormalFont, style.ForeColor, e.CellBounds);
        }

        public override void OnCellFormatting(DataGridViewCellFormattingEventArgs e, GitRevision revision)
        {
            var dateTime = AppSettings.ShowAuthorDate
                ? revision.AuthorDate
                : revision.CommitDate;

            e.Value = FormatDate(dateTime);
            e.FormattingApplied = true;

            string FormatDate(DateTime dt)
            {
                if (dt == DateTime.MinValue || dt == DateTime.MaxValue)
                {
                    return "";
                }

                if (AppSettings.RelativeDate)
                {
                    return LocalizationHelpers.GetRelativeDateString(DateTime.Now, dt, displayWeeks: false);
                }

                return dt.ToString("G");
            }
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, out string toolTip)
        {
            if (revision.ObjectId.IsArtificial)
            {
                toolTip = default;
                return false;
            }

            if (revision.Author == revision.Committer && revision.AuthorDate == revision.CommitDate)
            {
                toolTip = $"{revision.AuthorDate:g} {revision.Author} authored and committed";
            }
            else
            {
                toolTip =
                    $"{revision.AuthorDate:g} {revision.Author} authored\n" +
                    $"{revision.CommitDate:g} {revision.Committer} committed";
            }

            return true;
        }
    }
}