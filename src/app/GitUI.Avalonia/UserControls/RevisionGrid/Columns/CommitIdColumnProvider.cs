using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUIPluginInterfaces;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class CommitIdColumnProvider : ColumnProvider
{
    private readonly Dictionary<string, double[]> _widthByLengthByFont = new(capacity: 4);
    private readonly RevisionGridControl _grid;
    private int? _charCount;
    private double _maxWidth;

    public CommitIdColumnProvider(RevisionGridControl grid)
        : base("Commit ID", new GridLength(60), minimumWidth: 32, resizable: true)
    {
        _grid = grid;
        _maxWidth = TryMeasureText(GitRevision.WorkTreeGuid, out double width)
            ? width
            : double.PositiveInfinity;
    }

    public override void ApplySettings()
    {
        Column.IsVisible = AppSettings.ShowObjectIdColumn;
    }

    private int GetCharLengthForColumnWidth(double width)
    {
        WinFormsShims.Font font = AppSettings.MonospaceFont;
        string key = $"{font.Name}\0{font.Size}\0{font.Bold}\0{font.Italic}";
        if (!_widthByLengthByFont.TryGetValue(key, out double[]? widthByLength))
        {
            widthByLength = new double[ObjectId.Sha1CharCount + 1];
            for (int characterCount = 0; characterCount < widthByLength.Length; characterCount++)
            {
                if (!TryMeasureText(new string('8', characterCount), out widthByLength[characterCount]))
                {
                    return Math.Clamp((int)(width / Math.Max(1, font.Size)), 0, ObjectId.Sha1CharCount);
                }
            }

            _widthByLengthByFont[key] = widthByLength;
        }

        int firstTooWide = Array.FindIndex(widthByLength, measuredWidth => measuredWidth > width);
        if (firstTooWide == -1 && width >= widthByLength[^1])
        {
            return ObjectId.Sha1CharCount;
        }

        return firstTooWide > 1 ? firstTooWide - 1 : 0;
    }

    public override void OnCellPainting(Control control, GitRevision revision)
    {
        TextBlock textBlock = (TextBlock)control;
        _grid.DrawColumnText(textBlock, textBlock.Text, useEllipsis: false);
    }

    public override void OnColumnWidthChanged()
    {
        if (double.IsPositiveInfinity(_maxWidth)
            && TryMeasureText(GitRevision.WorkTreeGuid, out double width))
        {
            _maxWidth = width;
        }

        if (Column.Width.IsAbsolute && Column.Width.Value > _maxWidth)
        {
            Column.Width = new GridLength(_maxWidth);
        }

        _charCount = GetCharLengthForColumnWidth(Column.Width.Value - ColumnLeftMargin);
        _grid.RefreshRealizedRows();
    }

    public override void OnCellFormatting(Control control, GitRevision revision)
    {
        TextBlock textBlock = (TextBlock)control;
        if (revision.IsArtificial)
        {
            textBlock.Text = string.Empty;
        }
        else
        {
            _charCount ??= GetCharLengthForColumnWidth(Column.Width.Value - ColumnLeftMargin);
            textBlock.Text = _charCount.Value > 0
                ? revision.ObjectId.ToShortString(_charCount.Value)
                : string.Empty;
        }

        // Set the grid cell's accessibility text
        AutomationProperties.SetName(textBlock, textBlock.Text);
    }

    public override Control CreateCell()
    {
        TextBlock textBlock = CreateTextBlock(ColumnLeftMargin);
        textBlock.TextTrimming = TextTrimming.None;
        textBlock.Classes.Add("gitextensions-commit-header");
        textBlock.Classes.Add("revision-object-id-cell");
        return textBlock;
    }

    private static bool TryMeasureText(string text, out double width)
    {
        try
        {
            WinFormsShims.Font font = AppSettings.MonospaceFont;
            TextBlock probe = new()
            {
                FontFamily = new FontFamily(font.Name),
                FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(font.Size),
                FontStyle = font.Italic ? FontStyle.Italic : FontStyle.Normal,
                FontWeight = font.Bold ? FontWeight.Bold : FontWeight.Normal,
            };
            width = WinFormsTextMeasurer.MeasureTextRenderer(probe, text).Width;
            return true;
        }
        catch (InvalidOperationException)
        {
            // The font manager is unavailable while plain unit tests construct the provider before platform startup.
            width = 0;
            return false;
        }
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
}
