using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Color = Avalonia.Media.Color;
using Size = Avalonia.Size;

namespace GitUI.Compat;

/// <summary>
/// Preserves the original repository tree's GDI padding and native ImageList color conversion.
/// </summary>
internal sealed class NativeTreeImageListImage : IImage
{
    private static readonly ConditionalWeakTable<Bitmap, NativeTreeImageListImage> _cache = new();
    private readonly WriteableBitmap _bitmap;
    private readonly ConcurrentDictionary<Color, Lazy<WriteableBitmap>> _opaqueVariants = new();
    private readonly ConcurrentQueue<Color> _opaqueVariantOrder = new();
    private readonly NativeTreeImageListImage? _original;
    private readonly Func<Color?>? _getBackdrop;

    private NativeTreeImageListImage(Bitmap original)
    {
        AlphaFormat sourceAlpha = original.AlphaFormat ?? AlphaFormat.Premul;
        using WriteableBitmap source = new(original.PixelSize, original.Dpi, PixelFormat.Bgra8888, sourceAlpha);
        using ILockedFramebuffer sourceBuffer = source.Lock();
        original.CopyPixels(sourceBuffer);
        _bitmap = new WriteableBitmap(original.PixelSize, original.Dpi, PixelFormat.Bgra8888, AlphaFormat.Premul);
        using ILockedFramebuffer destination = _bitmap.Lock();
        Transform(sourceBuffer, destination);
    }

    private NativeTreeImageListImage(NativeTreeImageListImage original, Func<Color?> getBackdrop)
    {
        _original = original;
        _bitmap = original._bitmap;
        _getBackdrop = getBackdrop;
    }

    public Size Size => _bitmap.Size;

    internal int OpaqueVariantCount => (_original ?? this)._opaqueVariants.Count;

    internal static IImage Create(IImage original)
    {
        ArgumentNullException.ThrowIfNull(original);

        // Node.ApplyText reuses its existing header image. Do not run the native conversion
        // again when only the caption changes. Vector images have no original ImageList path.
        return original switch
        {
            NativeTreeImageListImage native => native._original ?? native,
            Bitmap bitmap => _cache.GetValue(bitmap, static value => new NativeTreeImageListImage(value)),
            _ => original,
        };
    }

    internal static IImage Create(IImage original, Func<Color?> getBackdrop)
        => Create(original) is NativeTreeImageListImage native
            ? new NativeTreeImageListImage(native, getBackdrop)
            : original;

    public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
    {
        NativeTreeImageListImage original = _original ?? this;
        Bitmap image = _getBackdrop?.Invoke() is Color { A: byte.MaxValue } backdrop
            ? original.GetOpaqueVariant(backdrop)
            : original._bitmap;
        ((IImage)image).Draw(context, sourceRect, destRect);
    }

    private WriteableBitmap GetOpaqueVariant(Color backdrop)
    {
        Lazy<WriteableBitmap> variant = _opaqueVariants.GetOrAdd(backdrop, color =>
        {
            _opaqueVariantOrder.Enqueue(color);
            return new Lazy<WriteableBitmap>(() => Composite(color), LazyThreadSafetyMode.ExecutionAndPublication);
        });
        while (_opaqueVariants.Count > 8 && _opaqueVariantOrder.TryDequeue(out Color oldest))
        {
            // Drop only the cache reference: a recorded Avalonia scene may still own
            // the bitmap's render lease. Live color editing must not grow this cache.
            _opaqueVariants.TryRemove(oldest, out _);
        }

        return variant.Value;
    }

    private unsafe WriteableBitmap Composite(Color backdrop)
    {
        WriteableBitmap bitmap = new(_bitmap.PixelSize, _bitmap.Dpi, PixelFormat.Bgra8888, AlphaFormat.Opaque);
        using ILockedFramebuffer source = _bitmap.Lock();
        using ILockedFramebuffer destination = bitmap.Lock();
        for (int y = 0; y < source.Size.Height; y++)
        {
            byte* sourceRow = (byte*)source.Address + (y * source.RowBytes);
            byte* destinationRow = (byte*)destination.Address + (y * destination.RowBytes);
            for (int x = 0; x < source.Size.Width; x++)
            {
                byte* pixel = sourceRow + (x * 4);
                byte* converted = destinationRow + (x * 4);
                int inverseAlpha = 255 - pixel[3];

                // GDI's rounded division by 255 differs from Skia source-over at some
                // channel values. Preserve the independently measured native result only
                // when the consumer supplies its actual uniform opaque paint backdrop.
                converted[0] = (byte)(pixel[0] + (((backdrop.B * inverseAlpha) + 127) / 255));
                converted[1] = (byte)(pixel[1] + (((backdrop.G * inverseAlpha) + 127) / 255));
                converted[2] = (byte)(pixel[2] + (((backdrop.R * inverseAlpha) + 127) / 255));
                converted[3] = byte.MaxValue;
            }
        }

        return bitmap;
    }

    private static unsafe void Transform(ILockedFramebuffer source, ILockedFramebuffer destination)
    {
        for (int y = 0; y < source.Size.Height; y++)
        {
            byte* sourceRow = (byte*)source.Address + (y * source.RowBytes);
            byte* destinationRow = (byte*)destination.Address + (y * destination.RowBytes);
            for (int x = 0; x < source.Size.Width; x++)
            {
                byte* pixel = sourceRow + (x * 4);
                byte* converted = destinationRow + (x * 4);
                byte alpha = source.AlphaFormat == AlphaFormat.Opaque ? byte.MaxValue : pixel[3];
                converted[3] = alpha;
                for (int channel = 0; channel < 3; channel++)
                {
                    converted[channel] = ConvertChannel(pixel[channel], alpha, source.AlphaFormat);
                }
            }
        }
    }

    private static byte ConvertChannel(byte channel, byte alpha, AlphaFormat sourceAlpha)
    {
        if (alpha == 0)
        {
            return 0;
        }

        // DrawImageUnscaled into the original transparent padding bitmap stores premultiplied
        // channels. Retain decoded premultiplied bytes rather than introducing another clone.
        int premultiplied = sourceAlpha == AlphaFormat.Premul ? channel : ((channel * alpha) + 127) / 255;
        int padded = (int)((premultiplied * (long)((255 << 16) / alpha)) >> 16);

        // ControlPaint.CreateHBitmapColorMask uses Bitmap.GetHbitmap(), whose explicit
        // default backdrop is Color.LightGray (211), not a configurable system/theme color.
        // Independent native DIB readback proves the padding reciprocal and atlas rounding.
        int colorMask = (((padded * alpha) + 127) / 255) + (((211 * (255 - alpha)) + 127) / 255);
        return (byte)(((colorMask * alpha) + 128) / 255);
    }
}
