namespace GitUI.Avatars;

public interface IAvatarDownloader
{
    Task<byte[]?> DownloadImageAsync(Uri? imageUrl);
}
