using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace GitUI.Compat;

/// <summary>
///  Adapts bitmap menu icons to ToolStripRenderer's disabled-image paint boundary.
/// </summary>
public static class NativeMenuImage
{
    private const float RedWeight = 0.2125f;
    private const float GreenWeight = 0.2577f;
    private const float BlueWeight = 0.0361f;
    private const float LightTranslation = 0.38f;
    private const float DarkTranslation = -0.1f;
    private const float LightAlpha = 0.7f;
    private const float DarkAlpha = 0.8f;

    /// <summary>
    ///  Identifies images presented by a ToolStrip-shaped menu icon presenter.
    /// </summary>
    public static readonly AttachedProperty<bool> UseDisabledRenderingProperty =
        AvaloniaProperty.RegisterAttached<Image, bool>("UseDisabledRendering", typeof(NativeMenuImage));

    /// <summary>
    ///  Identifies the native system color mode at the original renderer boundary.
    /// </summary>
    public static readonly AttachedProperty<bool> IsDarkModeProperty =
        AvaloniaProperty.RegisterAttached<Image, bool>("IsDarkMode", typeof(NativeMenuImage));

    private static readonly ConditionalWeakTable<Image, ImageScope> Scopes = new();
    private static int _disabledImageDarkMode = -1;

    static NativeMenuImage()
    {
        UseDisabledRenderingProperty.Changed.AddClassHandler<Image>((image, change) =>
        {
            if (change.GetNewValue<bool>())
            {
                Scopes.GetValue(image, static owner => new ImageScope(owner));
            }
            else if (Scopes.TryGetValue(image, out ImageScope? scope))
            {
                Scopes.Remove(image);
                scope.Dispose();
            }
        });
    }

    /// <summary>
    ///  Gets whether the icon uses the source disabled-image renderer.
    /// </summary>
    /// <param name="image">The retained menu icon.</param>
    /// <returns>Whether the source renderer is enabled.</returns>
    public static bool GetUseDisabledRendering(Image image) => image.GetValue(UseDisabledRenderingProperty);

    /// <summary>
    ///  Sets whether the icon uses the source disabled-image renderer.
    /// </summary>
    /// <param name="image">The retained menu icon.</param>
    /// <param name="value">Whether to enable the source renderer.</param>
    public static void SetUseDisabledRendering(Image image, bool value) => image.SetValue(UseDisabledRenderingProperty, value);

    /// <summary>
    ///  Gets the source native system color mode, independently of configurable CSS colors.
    /// </summary>
    /// <param name="image">The retained menu icon.</param>
    /// <returns>Whether the native color mode is dark.</returns>
    public static bool GetIsDarkMode(Image image) => image.GetValue(IsDarkModeProperty);

    /// <summary>
    ///  Sets the source native system color mode.
    /// </summary>
    /// <param name="image">The retained menu icon.</param>
    /// <param name="value">Whether the native color mode is dark.</param>
    public static void SetIsDarkMode(Image image, bool value) => image.SetValue(IsDarkModeProperty, value);

    private sealed class ImageScope : IDisposable
    {
        private readonly Image _image;
        private DisabledImage? _disabled;
        private bool _attached;
        private bool _queued;
        private bool _disposed;

        public ImageScope(Image image)
        {
            _image = image;
            _attached = TopLevel.GetTopLevel(image) is not null;
            image.AttachedToVisualTree += OnAttached;
            image.DetachedFromVisualTree += OnDetached;
            image.PropertyChanged += OnImageChanged;
            QueueUpdate();
        }

        public void Dispose()
        {
            _disposed = true;
            _image.AttachedToVisualTree -= OnAttached;
            _image.DetachedFromVisualTree -= OnDetached;
            _image.PropertyChanged -= OnImageChanged;
            QueueUpdate();
        }

        private void OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            _attached = true;
            QueueUpdate();
        }

        private void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            _attached = false;
            QueueUpdate();
        }

        private void OnImageChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Image.SourceProperty || e.Property == InputElement.IsEffectivelyEnabledProperty
                || e.Property == IsDarkModeProperty)
            {
                QueueUpdate();
            }
        }

        private void QueueUpdate()
        {
            if (_queued)
            {
                return;
            }

            // Template styles can change inside Avalonia's value-frame enumeration.
            // Keep source replacement outside that enumeration and before the render job.
            _queued = true;
            Dispatcher.UIThread.Post(Update, DispatcherPriority.Render);
        }

        private void Update()
        {
            _queued = false;
            if (_disposed || !_attached || _image.IsEffectivelyEnabled)
            {
                if (_image.Source is DisabledImage current
                    && (ReferenceEquals(current, _disabled) || (!_disposed && _attached && _image.IsEffectivelyEnabled)))
                {
                    _image.SetCurrentValue(Image.SourceProperty, current.Original);
                }

                RetireDisabledImage();
                return;
            }

            IImage? source = _image.Source is DisabledImage converted ? converted.Original : _image.Source;
            if (source is not Bitmap bitmap)
            {
                RetireDisabledImage();
                return;
            }

            if (_disabled is null || !ReferenceEquals(_disabled.Original, bitmap))
            {
                RetireDisabledImage();
                _disabled = new DisabledImage(bitmap);
            }

            _disabled.IsDarkMode = GetIsDarkMode(_image);
            if (!ReferenceEquals(_image.Source, _disabled))
            {
                // SetCurrentValue preserves a caller's live source binding. Enabled icons
                // retain the exact original image and never pass through bitmap conversion.
                _image.SetCurrentValue(Image.SourceProperty, _disabled);
            }

            _image.InvalidateVisual();
        }

        private void RetireDisabledImage()
        {
            // Bitmap.Draw records a reference-counted platform bitmap lease. Retiring
            // our owned bitmap releases this wrapper, not an already recorded scene's
            // lease or the globally shared original asset.
            _disabled?.Dispose();
            _disabled = null;
        }
    }

    private sealed class DisabledImage(Bitmap original) : IImage, IDisposable
    {
        private WriteableBitmap? _bitmap;
        private bool _disposed;

        public Bitmap Original { get; } = original;

        public bool IsDarkMode { get; set; }

        public Avalonia.Size Size => Original.Size;

        public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            // WinForms caches its matrix on the first actual disabled draw, not on
            // attachment or theme loading. Preserve that process-lifetime quirk.
            int requestedMode = IsDarkMode ? 1 : 0;
            int previousMode = Interlocked.CompareExchange(ref _disabledImageDarkMode, requestedMode, -1);
            _bitmap ??= CreateDisabledBitmap(Original, (previousMode == -1 ? requestedMode : previousMode) == 1);
            ((IImage)_bitmap).Draw(context, sourceRect, destRect);
        }

        public void Dispose()
        {
            _disposed = true;
            _bitmap?.Dispose();
            _bitmap = null;
        }
    }

    private static unsafe WriteableBitmap CreateDisabledBitmap(Bitmap original, bool dark)
    {
        using WriteableBitmap source = new(original.PixelSize, original.Dpi, PixelFormat.Bgra8888, AlphaFormat.Unpremul);
        using ILockedFramebuffer sourcePixels = source.Lock();
        original.CopyPixels(sourcePixels);
        WriteableBitmap converted = new(original.PixelSize, original.Dpi, PixelFormat.Bgra8888, AlphaFormat.Premul);
        using ILockedFramebuffer destination = converted.Lock();
        for (int y = 0; y < sourcePixels.Size.Height; y++)
        {
            byte* row = (byte*)sourcePixels.Address + (y * sourcePixels.RowBytes);
            byte* convertedRow = (byte*)destination.Address + (y * destination.RowBytes);
            for (int x = 0; x < sourcePixels.Size.Width; x++)
            {
                byte* pixel = row + (x * 4);
                byte* result = convertedRow + (x * 4);

                // These are ToolStripRenderer.DisabledImageColorMatrix's coefficients,
                // not CSS GrayText or an opacity applied to the enabled color image.
                float gray = (pixel[2] * RedWeight) + (pixel[1] * GreenWeight) + (pixel[0] * BlueWeight)
                    + ((dark ? DarkTranslation : LightTranslation) * byte.MaxValue);
                byte channel = (byte)MathF.Round(Math.Clamp(gray, 0, byte.MaxValue), MidpointRounding.ToEven);

                // GDI+ rounds the alpha after promoting the matrix's single-precision
                // coefficient to double. Rounding a float product changes half cases.
                // Retain its premultiplied bytes directly rather than adding a lossy
                // GetPixel-style reciprocal/unpremultiply and a second Skia conversion.
                byte alpha = (byte)Math.Round(pixel[3] * (double)(dark ? DarkAlpha : LightAlpha), MidpointRounding.ToEven);
                byte premultiplied = (byte)Math.Round(channel * (double)alpha / byte.MaxValue, MidpointRounding.ToEven);
                result[0] = premultiplied;
                result[1] = premultiplied;
                result[2] = premultiplied;
                result[3] = alpha;
            }
        }

        return converted;
    }
}
