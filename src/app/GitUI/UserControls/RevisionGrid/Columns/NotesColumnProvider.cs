using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class NotesColumnProvider : ColumnProvider
{
    private readonly ICommitDataManager? _commitDataManager;
    private readonly RevisionGridControl _grid;

    public NotesColumnProvider(RevisionGridControl grid, ICommitDataManager? commitDataManager)
        : base("Notes")
    {
        _commitDataManager = commitDataManager;
        _grid = grid;

        DataGridViewTextBoxColumn? column = new()
        {
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            HeaderText = Name,
            ReadOnly = false,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            Width = DpiUtil.Scale(50),
            MinimumWidth = DpiUtil.Scale(25),
        };
        Column = column;
    }

    public override void ApplySettings()
    {
        Column.Visible = AppSettings.ShowGitNotesColumn.Value;
    }

    public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style)
    {
        if (FirstLine(revision.Notes) is string firstLine)
        {
            _grid.DrawColumnText(e, firstLine, style.NormalFont, style.ForeColor, e.CellBounds);
        }
        else
        {
            _commitDataManager?.InitiateDelayedLoadingOfDetails(revision);
        }

        return;

        static string? FirstLine(string? text)
            => text?.IndexOf('\n') is int eolIndex and >= 0
                ? text[..eolIndex]
                : text;
    }

    public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        toolTip = revision.Notes;
        return toolTip is not null;
    }
}
