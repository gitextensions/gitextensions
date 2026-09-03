using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace GitUI.Avatars;

internal static class AvatarImage
{
    public static Bitmap? Decode(byte[]? imageData)
    {
        if (imageData is not { Length: > 0 })
        {
            return null;
        }

        try
        {
            using MemoryStream stream = new(imageData, writable: false);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }

    public static byte[] Encode(Bitmap bitmap)
    {
        using MemoryStream stream = new();
        bitmap.Save(stream, PngBitmapEncoderOptions.Default);
        return stream.ToArray();
    }

    public static PixelSize? GetPixelSize(byte[]? imageData)
    {
        using Bitmap? bitmap = Decode(imageData);
        return bitmap?.PixelSize;
    }

    public static byte[] Resize(byte[] imageData, int imageSize)
    {
        using Bitmap? source = Decode(imageData);
        if (source is null || source.PixelSize == new PixelSize(imageSize, imageSize))
        {
            return imageData;
        }

        using RenderTargetBitmap resized = new(new PixelSize(imageSize, imageSize), new Vector(96, 96));
        using (DrawingContext context = resized.CreateDrawingContext())
        {
            context.DrawImage(
                source,
                new Rect(source.Size),
                new Rect(0, 0, imageSize, imageSize));
        }

        return Encode(resized);
    }
}
