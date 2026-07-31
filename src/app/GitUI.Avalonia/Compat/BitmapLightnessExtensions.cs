using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;

namespace GitUI.Compat;

public static class BitmapLightnessExtensions
{
    private static readonly ConditionalWeakTable<Bitmap, ConcurrentDictionary<(int text, int background), Lazy<Bitmap>>> _cache = new();

    public static Bitmap AdaptLightness(this Bitmap original)
        => AdaptLightness(original, ThemeModule.Settings);

    // parity-scaffolding: Allows exact light/dark/custom algorithm verification without mutating global theme state.
    internal static Bitmap AdaptLightness(Bitmap original, ThemeSettings settings)
    {
        ArgumentNullException.ThrowIfNull(original);
        if (settings.Theme.Id == ThemeId.DefaultLight)
        {
            return original;
        }

        Color text = AvaloniaThemeResources.ResolveSystemColor(settings, KnownColor.WindowText);
        Color background = AvaloniaThemeResources.ResolveSystemColor(settings, KnownColor.Window);
        ConcurrentDictionary<(int text, int background), Lazy<Bitmap>> variants = _cache.GetValue(
            original,
            static _ => new());

        // Avalonia framework constraint: cache immutable shared bitmap variants instead of creating the original GDI clone at each call site.
        return variants.GetOrAdd(
            (text.ToArgb(), background.ToArgb()),
            _ => new Lazy<Bitmap>(
                () => CreateAdapted(original, text, background),
                LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    private static Bitmap CreateAdapted(Bitmap original, Color text, Color background)
    {
        // Avalonia framework constraint: use a locked framebuffer in place of the original GDI bitmap clone while retaining its calculation and alpha.
        WriteableBitmap clone = new(
            original.PixelSize,
            original.Dpi,
            PixelFormat.Bgra8888,
            AlphaFormat.Unpremul);
        using ILockedFramebuffer framebuffer = clone.Lock();
        original.CopyPixels(framebuffer);

        HslColor textColor = new(text);
        HslColor backgroundColor = new(background);
        Transform(framebuffer, textColor, backgroundColor);
        return clone;
    }

    private static unsafe void Transform(
        ILockedFramebuffer framebuffer,
        HslColor textColor,
        HslColor backgroundColor)
    {
        for (int y = 0; y < framebuffer.Size.Height; y++)
        {
            byte* row = (byte*)framebuffer.Address + (y * framebuffer.RowBytes);
            for (int x = 0; x < framebuffer.Size.Width; x++)
            {
                byte* pixel = row + (x * 4);
                Color rgb = Color.FromArgb(pixel[2], pixel[1], pixel[0]);
                HslColor hsl = rgb.ToPerceptedHsl();

                // mathematically near black color can have high saturation
                // practically though the hue (color) is not distinguishable
                // so "perceived" saturation is near 0
                double saturation = hsl.L > 0.1
                    ? hsl.S
                    : hsl.S * hsl.L / 0.1;
                double luminosity = textColor.L + (hsl.L * (backgroundColor.L - textColor.L));
                Color transformed = new HslColor(hsl.H, saturation, luminosity)
                    .ToActualHsl(rgb)
                    .ToColor();
                pixel[0] = transformed.B;
                pixel[1] = transformed.G;
                pixel[2] = transformed.R;
            }
        }
    }
}
