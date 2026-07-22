using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class NotesColumnProvider : ColumnProvider
{
    public NotesColumnProvider()
        : base("Notes", new GridLength(50), minimumWidth: 25, resizable: true)
    {
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

    public override void UpdateCell(Control control, GitRevision revision)
    {
        ((TextBlock)control).Text = FirstLine(revision.Notes) ?? string.Empty;
        UpdateToolTip(control, revision);
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
