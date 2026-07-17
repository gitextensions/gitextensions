using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using GitCommands;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class DateColumnProvider : ColumnProvider
{
    public DateColumnProvider()
        : base("Date", new GridLength(130), minimumWidth: 25, resizable: true)
    {
    }

    public override void ApplySettings()
    {
        Column.IsVisible = AppSettings.ShowDateColumn;
    }

    public override Control CreateCell()
    {
        TextBlock textBlock = CreateTextBlock(ColumnLeftMargin, opacity: 0.7);
        textBlock.Classes.Add("revision-date-cell");
        return textBlock;
    }

    public override void UpdateCell(Control control, GitRevision revision)
    {
        DateTime dateTime = GetDate(revision, AppSettings.ShowAuthorDate);
        ((TextBlock)control).Text = revision.IsArtificial
            ? string.Empty
            : FormatDate(dateTime, DateTime.Now, AppSettings.RelativeDate);
        UpdateToolTip(control, revision);
    }

    public override bool TryGetToolTip(GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        if (revision.IsArtificial)
        {
            toolTip = null;
            return false;
        }

        toolTip = revision.Author == revision.Committer && revision.AuthorDate == revision.CommitDate
            ? $"{revision.AuthorDate:g} {revision.Author} authored and committed"
            : $"{revision.AuthorDate:g} {revision.Author} authored\n{revision.CommitDate:g} {revision.Committer} committed";
        return true;
    }

    internal static DateTime GetDate(GitRevision revision, bool showAuthorDate)
        => showAuthorDate ? revision.AuthorDate : revision.CommitDate;

    internal static string FormatDate(DateTime dateTime, DateTime now, bool relative)
    {
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue)
        {
            return string.Empty;
        }

        return relative
            ? LocalizationHelpers.GetRelativeDateString(now, dateTime, displayWeeks: false)
            : dateTime.ToString("G");
    }
}
