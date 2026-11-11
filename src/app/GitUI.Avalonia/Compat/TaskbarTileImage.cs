using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GitUI.Properties;
using Color = Avalonia.Media.Color;

namespace GitUI.Compat;

/// <summary>
/// Draws the application icon with the Windows taskbar progress bar and overlay icon on top, for
/// platforms that show taskbar state only through the application's own icon image.
/// </summary>
internal static class TaskbarTileImage
{
    internal const int Size = 256;

    private static readonly Color NormalProgress = Color.FromRgb(0x06, 0xB0, 0x25);
    private static readonly Color ErrorProgress = Color.FromRgb(0xE8, 0x11, 0x23);
    private static readonly Color PausedProgress = Color.FromRgb(0xFF, 0xB9, 0x00);

    /// <summary>
    /// Renders the icon as PNG data.
    /// </summary>
    /// <param name="state">The progress state; <see cref="TaskbarProgressBarState.NoProgress"/> draws no bar.</param>
    /// <param name="progress">The completed fraction, from 0 to 1.</param>
    /// <param name="overlay">The overlay icon's PNG data, or <see langword="null"/> for none.</param>
    internal static byte[] Render(TaskbarProgressBarState state, double progress, byte[]? overlay)
    {
        using RenderTargetBitmap tile = new(new PixelSize(Size, Size));
        using (DrawingContext context = tile.CreateDrawingContext())
        {
            context.DrawImage(Images.GitExtensionsLogo256, new Rect(0, 0, Size, Size));

            if (overlay is not null)
            {
                // The Windows taskbar draws the overlay at half the button size in its bottom-right corner.
                using Bitmap overlayImage = new(new MemoryStream(overlay));
                context.DrawImage(overlayImage, new Rect(Size / 2, Size / 2, Size / 2, Size / 2));
            }

            if (state != TaskbarProgressBarState.NoProgress)
            {
                DrawProgress(context, state, progress);
            }
        }

        using MemoryStream stream = new();
        tile.Save(stream, PngBitmapEncoderOptions.Default);
        return stream.ToArray();
    }

    private static void DrawProgress(DrawingContext context, TaskbarProgressBarState state, double progress)
    {
        Rect bar = new(24, Size - 56, Size - 48, 32);
        context.DrawRectangle(new SolidColorBrush(Color.FromArgb(0xC0, 0x20, 0x20, 0x20)), new Pen(Brushes.White, 2), bar, 8, 8);

        Rect inner = bar.Deflate(4);
        switch (state)
        {
            case TaskbarProgressBarState.Indeterminate:
                // A still image cannot animate a marquee, so the whole bar shows in a lighter normal color.
                context.DrawRectangle(new SolidColorBrush(NormalProgress, 0.5), null, inner, 4, 4);
                break;
            default:
                Color color = state switch
                {
                    TaskbarProgressBarState.Error => ErrorProgress,
                    TaskbarProgressBarState.Paused => PausedProgress,
                    _ => NormalProgress,
                };

                if (progress > 0)
                {
                    context.DrawRectangle(new SolidColorBrush(color), null, inner.WithWidth(inner.Width * progress), 4, 4);
                }

                break;
        }
    }
}
