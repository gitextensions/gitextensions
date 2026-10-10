using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class NotesColumnProvider : ColumnProvider
{
    private readonly ICommitDataManager? _commitDataManager;
    private readonly RevisionGridControl _grid;

    public NotesColumnProvider(RevisionGridControl grid, ICommitDataManager? commitDataManager)
        : base("Notes", new GridLength(50), minimumWidth: 25, resizable: true)
    {
        _commitDataManager = commitDataManager;
        _grid = grid;
    }

    public override void ApplySettings()
    {
        Column.IsVisible = AppSettings.ShowGitNotesColumn.Value;
    }

    public override Control CreateCell()
    {
        TextBlock textBlock = CreateTextBlock(ColumnLeftMargin);
        textBlock.Classes.Add("revision-notes-cell");
        return textBlock;
    }

    public override void OnCellPainting(Control control, GitRevision revision)
    {
        TextBlock textBlock = (TextBlock)control;
        if (FirstLine(revision.Notes) is string firstLine)
        {
            _grid.DrawColumnText(textBlock, firstLine);
        }
        else
        {
            _grid.DrawColumnText(textBlock, string.Empty);
            _commitDataManager?.InitiateDelayedLoadingOfDetails(revision);
        }
    }

    public override bool TryGetToolTip(GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        toolTip = revision.Notes;
        return toolTip is not null;
    }

    internal static string? FirstLine(string? text)
        => text?.IndexOf('\n') is int endOfLine and >= 0
            ? text[..endOfLine]
            : text;
}
