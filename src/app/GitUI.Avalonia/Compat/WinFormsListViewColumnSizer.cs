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
    private const double HorizontalCellPadding = 10;

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
