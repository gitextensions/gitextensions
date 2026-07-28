using System.Security.Cryptography;
using System.Text;
using GitCommands;

namespace GitUI.Avatars;

public sealed class GravatarProvider : IAvatarProvider, IDisposable
{
    private const string _rating = "g";

    private readonly IAvatarDownloader _downloader;
    private readonly string _queryString;
    private readonly SHA256 _sha256 = SHA256.Create();

    public GravatarProvider(
        IAvatarDownloader downloader,
        AvatarFallbackType? fallback,
        bool forceFallback = false)
    {
        _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));

        string fallbackString = SerializeFallbackType(fallback) ?? "404";
        _queryString = $"r={_rating}&d={fallbackString}";

        if (forceFallback)
        {
            _queryString += "&f=y";
        }

        _queryString += "&s=";
    }

    public bool PerformsIo => true;

    public static bool IsFallbackSupportedByGravatar(AvatarFallbackType fallback)
        => SerializeFallbackType(fallback) is not null;

    /// <inheritdoc/>
    public Task<byte[]?> GetAvatarAsync(string email, string? name, int imageSize)
    {
        string hash = ComputeHash(email);
        UriBuilder uri = new("https", "www.gravatar.com")
        {
            Path = $"/avatar/{hash}",
            Query = _queryString + imageSize
        };

        return _downloader.DownloadImageAsync(uri.Uri);
    }

    public void Dispose()
    {
        _sha256.Dispose();
    }

    private string ComputeHash(string email)
    {
        byte[] emailBytes = Encoding.UTF8.GetBytes(email.Trim().ToLowerInvariant());
        byte[] hashBytes = _sha256.ComputeHash(emailBytes);
        return HexString.FromByteArray(hashBytes);
    }

    private static string? SerializeFallbackType(AvatarFallbackType? fallback)
    {
        return fallback switch
        {
            AvatarFallbackType.Identicon => "identicon",
            AvatarFallbackType.MonsterId => "monsterid",
            AvatarFallbackType.Wavatar => "wavatar",
            AvatarFallbackType.Retro => "retro",
            AvatarFallbackType.Robohash => "robohash",
            _ => null,
        };
    }
}
