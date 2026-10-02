using Avalonia.Media;
using Color = Avalonia.Media.Color;

namespace GitUI.Compat;

/// <summary>Resolves Explorer's image-based tree selection against the source control background.</summary>
internal readonly record struct NativeTreePaintPalette(Color Background, Color Foreground, Color Border)
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
                ? new(Color.FromRgb(51, 51, 51), foreground, Composite(backdrop, Color.FromRgb(22, 22, 22), Color.FromRgb(221, 221, 221)))
                : new(Composite(backdrop, Colors.Black, Color.FromRgb(217, 217, 217)), foreground,
                    Composite(backdrop, Colors.Black, Color.FromRgb(148, 148, 148)));
        }

        return dark
            ? new(Color.FromRgb(98, 98, 98), foreground, Color.FromRgb(96, 205, 255))
            : new(Composite(backdrop, Color.FromRgb(0, 28, 51), Color.FromRgb(204, 232, 255)), foreground,
                Color.FromRgb(0, 120, 212));
    }

    private static Color Composite(Color backdrop, Color blackSample, Color whiteSample)
        => Color.FromRgb(
            CompositeChannel(backdrop.R, blackSample.R, whiteSample.R),
            CompositeChannel(backdrop.G, blackSample.G, whiteSample.G),
            CompositeChannel(backdrop.B, blackSample.B, whiteSample.B));

    private static byte CompositeChannel(byte backdrop, byte blackSample, byte whiteSample)
        => (byte)(blackSample + ((((whiteSample - blackSample) * backdrop) + 127) / 255));
}
