using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitUI.Compat;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class DateColumnProvider : ColumnProvider
{
    private readonly RevisionGridControl _grid;

    public DateColumnProvider(RevisionGridControl grid)
        : base("Date", new GridLength(GetInitialWidth()), minimumWidth: 25, resizable: true)
    {
        _grid = grid;
    }

    private static double GetInitialWidth()
    {
        if (AppSettings.RelativeDate)
        {
            return 130;
        }

        TextBlock text = new()
        {
            FontFamily = new FontFamily(AppSettings.Font.Name),
            FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size),
            FontStyle = AppSettings.Font.Italic ? FontStyle.Italic : FontStyle.Normal,
            FontWeight = AppSettings.Font.Bold ? FontWeight.Bold : FontWeight.Normal,
        };
        return WinFormsTextMeasurer.MeasureTextRenderer(text, DateTime.Now.ToString("G")).Width;
    }

    public override void ApplySettings()
    {
        Column.IsVisible = AppSettings.ShowDateColumn;
    }

    public override Control CreateCell()
    {
        TextBlock textBlock = CreateTextBlock(ColumnLeftMargin);
        textBlock.Classes.Add("revision-date-cell");
        return textBlock;
    }

    public override void OnCellPainting(Control control, GitRevision revision)
    {
        TextBlock textBlock = (TextBlock)control;
        _grid.DrawColumnText(textBlock, textBlock.Text);
    }

    public override void OnCellFormatting(Control control, GitRevision revision)
    {
        DateTime dateTime = GetDate(revision, AppSettings.ShowAuthorDate);
        ((TextBlock)control).Text = revision.IsArtificial
            ? string.Empty
            : FormatDate(dateTime, DateTime.Now, AppSettings.RelativeDate);
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
