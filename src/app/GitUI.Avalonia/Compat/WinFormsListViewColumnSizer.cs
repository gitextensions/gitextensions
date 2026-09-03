using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace GitUI.Compat;

/// <summary>
///  Reproduces the content/header measurement boundary used by WinForms ListView columns.
/// </summary>
internal static class WinFormsListViewColumnSizer
{
    private const double HorizontalCellPadding = 11;

    private const double HeaderPadding = 12;
    private const double ContentPadding = 14;

    public static double MeasureAutoSizedColumn(
        TemplatedControl owner,
        IEnumerable<string?> values,
        bool sizeToHeader,
        bool firstColumn,
        double headerPadding = HeaderPadding)
    {
        double width = values
            .Select(value => WinFormsTextMeasurer.Measure(owner, value ?? string.Empty))
            .DefaultIfEmpty()
            .Max();
        double padding = sizeToHeader ? headerPadding : ContentPadding;
        if (firstColumn)
        {
            // Framework constraint: the native ListView reserves extra leading space in its first column.
            padding += sizeToHeader ? 6 : 3;
        }

        return Math.Ceiling(width + padding);
    }

    public static double Measure(TemplatedControl owner, IEnumerable<string?> values, double additionalWidth = 0)
    {
        Typeface typeface = new(owner.FontFamily, owner.FontStyle, owner.FontWeight);
        double width = values
            .Select(value => new FormattedText(
                value ?? string.Empty,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                typeface,
                owner.FontSize,
                foreground: null).WidthIncludingTrailingWhitespace)
            .DefaultIfEmpty()
            .Max();

        return Math.Ceiling(width + HorizontalCellPadding + additionalWidth);
    }

    public static ColumnDefinitions CreateColumns(IReadOnlyList<double> widths, int fillColumn = -1)
    {
        ColumnDefinitions columns = [];
        for (int index = 0; index < widths.Count; index++)
        {
            columns.Add(new ColumnDefinition
            {
                Width = index == fillColumn
                    ? new GridLength(1, GridUnitType.Star)
                    : new GridLength(widths[index]),
            });
        }

        return columns;
    }
}
