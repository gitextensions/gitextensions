using System.Diagnostics;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace GitUI.Avatars;

/// <summary>
/// A provider proxy that makes sure that the requested image size is reasonable.
/// </summary>
/// <remarks>
/// If the image size is less than one it will be set to a default value (64px)
/// and the upper size is limited to 512px to prevent unreasonable avatar sizes.
///
/// If the inner provider crashes or returns null an "emergency fallback" is provided.
/// </remarks>
public sealed class SafetynetAvatarProvider : IAvatarProvider
{
    private const int _upperSizeLimit = 512;
    private const int _defaultSize = 64;

    private readonly IAvatarProvider _avatarProvider;
    private readonly Lazy<byte[]> _safetyNetFallback = new(GenerateSafetynetFallback);

    public SafetynetAvatarProvider(IAvatarProvider avatarProvider)
    {
        _avatarProvider = avatarProvider ?? throw new ArgumentNullException(nameof(avatarProvider));
    }

    public bool PerformsIo => _avatarProvider.PerformsIo;

    public async Task<byte[]?> GetAvatarAsync(string email, string? name, int imageSize)
    {
        imageSize = Math.Clamp(imageSize < 1 ? _defaultSize : imageSize, 1, _upperSizeLimit);

        try
        {
            byte[]? image = await _avatarProvider.GetAvatarAsync(email, name, imageSize);

            if (image is not null)
            {
                return image;
            }
        }
        catch (Exception ex)
        {
            // Something went wrong. Log, ignore and proceed with fallback.
            Trace.WriteLine(ex.Message);
        }

        return _safetyNetFallback.Value;
    }

    private static byte[] GenerateSafetynetFallback()
    {
        using RenderTargetBitmap bitmap = new(new PixelSize(1, 1), new Vector(96, 96));
        using (DrawingContext context = bitmap.CreateDrawingContext())
        {
            context.FillRectangle(Brushes.Red, new Rect(0, 0, 1, 1));
        }

        return AvatarImage.Encode(bitmap);
    }
}
