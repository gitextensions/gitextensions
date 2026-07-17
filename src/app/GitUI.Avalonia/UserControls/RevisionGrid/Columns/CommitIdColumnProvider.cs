using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class CommitIdColumnProvider : ColumnProvider
{
    public CommitIdColumnProvider()
        : base("Commit ID", new GridLength(60), minimumWidth: 32, resizable: true)
    {
    }

    public override void ApplySettings()
    {
        Column.IsVisible = AppSettings.ShowObjectIdColumn;
    }

    public override Control CreateCell()
    {
        TextBlock textBlock = CreateTextBlock(ColumnLeftMargin);
        textBlock.TextTrimming = TextTrimming.None;
        textBlock.Classes.Add("gitextensions-commit-header");
        textBlock.Classes.Add("revision-object-id-cell");
        return textBlock;
    }

    public override void UpdateCell(Control control, GitRevision revision)
    {
        TextBlock textBlock = (TextBlock)control;
        if (revision.IsArtificial)
        {
            textBlock.Text = string.Empty;
        }
        else
        {
            FormattedText character = new(
                "8",
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight),
                textBlock.FontSize,
                Brushes.Black);
            int characterCount = GetCharLengthForColumnWidth(Column.Width.Value - ColumnLeftMargin, character.Width);
            textBlock.Text = characterCount > 0
                ? revision.ObjectId.ToShortString(characterCount)
                : string.Empty;
        }

        UpdateToolTip(control, revision);
    }

    public override bool TryGetToolTip(GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        if (revision.IsArtificial)
        {
            toolTip = null;
            return false;
        }

        toolTip = revision.Guid;
        return true;
    }

    internal static int GetCharLengthForColumnWidth(double width, double characterWidth)
        => characterWidth <= 0
            ? 0
            : Math.Clamp((int)Math.Floor(width / characterWidth), 0, ObjectId.Sha1CharCount);
}
