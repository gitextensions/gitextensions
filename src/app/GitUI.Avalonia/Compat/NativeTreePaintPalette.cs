using Avalonia.Media;
using Color = Avalonia.Media.Color;

namespace GitUI.Compat;

/// <summary>Resolves Explorer's image-based tree selection against the source control background.</summary>
internal readonly record struct NativeTreePaintPalette(
    Color Background, Color Foreground, Color Border, Color Corner, Color AdjacentEdge, Color InnerCorner)
{
    internal static NativeTreePaintPalette Resolve(bool dark, Color backdrop, bool inactive = false)
    {
        // TREEVIEW part 1 uses image pixels, not its inherited FillColor/BorderColor.
        // These native black/white samples preserve the image's alpha composition,
        // including selected-not-focused edges; the CSS PanelBackground remains live.
        Color foreground = dark ? Colors.White : Colors.Black;
        if (inactive)
        {
            return dark
                ? new(Color.FromRgb(51, 51, 51), foreground, Composite(backdrop, Color.FromRgb(22, 22, 22), Color.FromRgb(221, 221, 221)),
                    Composite(backdrop, Color.FromRgb(7, 7, 7), Color.FromRgb(243, 243, 243)),
                    Composite(backdrop, Color.FromRgb(20, 20, 20), Color.FromRgb(223, 223, 223)),
                    Composite(backdrop, Color.FromRgb(46, 46, 46), Color.FromRgb(81, 81, 81)))
                : new(Composite(backdrop, Colors.Black, Color.FromRgb(217, 217, 217)), foreground,
                    Composite(backdrop, Colors.Black, Color.FromRgb(148, 148, 148)),
                    Composite(backdrop, Colors.Black, Color.FromRgb(157, 157, 157)),
                    Composite(backdrop, Colors.Black, Color.FromRgb(148, 148, 148)),
                    Composite(backdrop, Colors.Black, Color.FromRgb(217, 217, 217)));
        }

        return dark
            ? new(Color.FromRgb(98, 98, 98), foreground, Color.FromRgb(96, 205, 255),
                Composite(backdrop, Color.FromRgb(33, 70, 87), Color.FromRgb(201, 238, 255)),
                Composite(backdrop, Color.FromRgb(88, 189, 235), Color.FromRgb(108, 209, 255)),
                Composite(backdrop, Color.FromRgb(87, 87, 87), Color.FromRgb(115, 115, 115)))
            : new(Composite(backdrop, Color.FromRgb(0, 28, 51), Color.FromRgb(204, 232, 255)), foreground,
                Color.FromRgb(0, 120, 212),
                Composite(backdrop, Color.FromRgb(0, 24, 42), Color.FromRgb(204, 228, 246)),
                Color.FromRgb(0, 120, 212),
                Composite(backdrop, Color.FromRgb(0, 28, 51), Color.FromRgb(204, 232, 255)));
    }

    private static Color Composite(Color backdrop, Color blackSample, Color whiteSample)
        => Color.FromRgb(
            CompositeChannel(backdrop.R, blackSample.R, whiteSample.R),
            CompositeChannel(backdrop.G, blackSample.G, whiteSample.G),
            CompositeChannel(backdrop.B, blackSample.B, whiteSample.B));

    private static byte CompositeChannel(byte backdrop, byte blackSample, byte whiteSample)
        => (byte)(blackSample + ((((whiteSample - blackSample) * backdrop) + 127) / 255));
}
