using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitUI.UserControls;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class AuthorNameColumnProvider : ColumnProvider
{
    private readonly AuthorRevisionHighlighting _authorHighlighting;

    public AuthorNameColumnProvider(AuthorRevisionHighlighting authorHighlighting)
        : base("Author Name", new GridLength(130), minimumWidth: 25, resizable: true)
    {
        _authorHighlighting = authorHighlighting;
    }

    public override void ApplySettings()
    {
        Column.IsVisible = AppSettings.ShowAuthorNameColumn;
    }

    public override Control CreateCell()
    {
        TextBlock textBlock = CreateTextBlock(ColumnLeftMargin, opacity: 0.85);
        textBlock.Classes.Add("revision-author-cell");
        return textBlock;
    }

    public override void UpdateCell(Control control, GitRevision revision)
    {
        TextBlock textBlock = (TextBlock)control;
        textBlock.Text = revision.IsArtificial ? string.Empty : revision.Author ?? string.Empty;
        textBlock.FontWeight = _authorHighlighting.IsHighlighted(revision)
            ? FontWeight.Bold
            : FontWeight.Normal;
        UpdateToolTip(control, revision);
    }

    public override bool TryGetToolTip(GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        if (revision.IsArtificial)
        {
            toolTip = null;
            return false;
        }

        toolTip = GetAuthorAndCommiterToolTip(revision);
        return true;
    }

    public static string GetAuthorAndCommiterToolTip(GitRevision revision)
    {
        if (revision.Author == revision.Committer && revision.AuthorEmail == revision.CommitterEmail)
        {
            return $"{revision.Author} <{revision.AuthorEmail}> {TranslatedStrings.AuthoredAndCommitted}";
        }

        return $"{revision.Author} <{revision.AuthorEmail}> {TranslatedStrings.Authored}\n"
            + $"{revision.Committer} <{revision.CommitterEmail}> {TranslatedStrings.Committed}";
    }
}
