namespace GitUI.Avatars
{
    public interface IAvatarDownloader
    {
        Task<Image?> DownloadImageAsync(Uri? imageUrl);
    }
}
