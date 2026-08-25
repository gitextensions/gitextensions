using System.Globalization;
using System.Runtime.InteropServices;
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

    private const double HeaderPadding = 12;
    private const double ContentPadding = 14;

    public static double MeasureAutoSizedColumn(
        TemplatedControl owner,
        IEnumerable<string?> values,
        bool sizeToHeader,
        bool firstColumn)
    {
        double width = values
            .Select(value => MeasureText(owner, value ?? string.Empty))
            .DefaultIfEmpty()
            .Max();
        double padding = sizeToHeader ? HeaderPadding : ContentPadding;
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

    private static double MeasureText(TemplatedControl owner, string value)
    {
        if (OperatingSystem.IsWindows()
            && WindowsGdiTextMeasurer.TryMeasure(owner, value, out int width))
        {
            // Framework constraint: WinForms ListView auto-sizing uses unpadded GDI text metrics.
            return width;
        }

        Typeface typeface = new(owner.FontFamily, owner.FontStyle, owner.FontWeight);
        return new FormattedText(
            value,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            typeface,
            owner.FontSize,
            foreground: null).WidthIncludingTrailingWhitespace;
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

    private static class WindowsGdiTextMeasurer
    {
        private const int DefaultCharset = 1;
        private const int OutDefaultPrecision = 0;
        private const int ClipDefaultPrecision = 0;
        private const int DefaultQuality = 0;
        private const int DefaultPitchAndFamily = 0;

        public static bool TryMeasure(TemplatedControl owner, string value, out int width)
        {
            nint deviceContext = GetDC(0);
            if (deviceContext == 0)
            {
                width = 0;
                return false;
            }

            int weight = (int)owner.FontWeight;
            nint font = CreateFont(
                -(int)Math.Round(owner.FontSize, MidpointRounding.AwayFromZero),
                0,
                0,
                0,
                weight,
                owner.FontStyle == FontStyle.Italic,
                false,
                false,
                DefaultCharset,
                OutDefaultPrecision,
                ClipDefaultPrecision,
                DefaultQuality,
                DefaultPitchAndFamily,
                owner.FontFamily.Name);
            if (font == 0)
            {
                ReleaseDC(0, deviceContext);
                width = 0;
                return false;
            }

            nint previousFont = SelectObject(deviceContext, font);
            bool measured = GetTextExtentPoint32(deviceContext, value, value.Length, out NativeSize size);
            SelectObject(deviceContext, previousFont);
            DeleteObject(font);
            ReleaseDC(0, deviceContext);
            width = size.Width;
            return measured;
        }

        [DllImport("user32.dll")]
        private static extern nint GetDC(nint window);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(nint window, nint deviceContext);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        private static extern nint CreateFont(
            int height,
            int width,
            int escapement,
            int orientation,
            int weight,
            [MarshalAs(UnmanagedType.Bool)] bool italic,
            [MarshalAs(UnmanagedType.Bool)] bool underline,
            [MarshalAs(UnmanagedType.Bool)] bool strikeOut,
            int charSet,
            int outputPrecision,
            int clippingPrecision,
            int quality,
            int pitchAndFamily,
            string faceName);

        [DllImport("gdi32.dll")]
        private static extern nint SelectObject(nint deviceContext, nint value);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(nint value);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetTextExtentPoint32(
            nint deviceContext,
            string text,
            int textLength,
            out NativeSize size);

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct NativeSize
        {
            public readonly int Width;
            public readonly int Height;
        }
    }
}
