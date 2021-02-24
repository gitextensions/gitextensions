using System;
using System.Drawing;
using System.Threading.Tasks;

namespace GitUI.Avatars
{
    public interface IAvatarDownloader
    {
        Task<Image?> DownloadImageAsync(Uri? imageUrl);
    }
}
