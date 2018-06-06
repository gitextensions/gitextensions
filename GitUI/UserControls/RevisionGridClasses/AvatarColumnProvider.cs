using System.Windows.Forms;
using GitCommands;
using GitUI.Properties;
using GitUI.RevisionGridClasses;
using Gravatar;

namespace GitUI
{
    internal sealed class AvatarColumnProvider
    {
        private readonly DataGridViewTextBoxColumn _column;
        private readonly IImageCache _avatarCache;
        private readonly IAvatarService _gravatarService;
        private readonly IImageNameProvider _avatarImageNameProvider;

        public AvatarColumnProvider(DvcsGraph graph, DataGridViewTextBoxColumn column)
        {
            _column = column;
            _avatarImageNameProvider = new AvatarImageNameProvider();
            _avatarCache = new DirectoryImageCache(AppSettings.GravatarCachePath, AppSettings.AuthorImageCacheDays);
            _avatarCache.Invalidated += (s, e) => graph.Invalidate();
            _gravatarService = new GravatarService(_avatarCache, _avatarImageNameProvider);
        }

        public int Index => _column.Index;

        public void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision)
        {
            var imageName = _avatarImageNameProvider.Get(revision.AuthorEmail);

            var gravatar = _avatarCache.GetImage(imageName, null);

            if (gravatar == null)
            {
                gravatar = Resources.User;

                // kick off download operation, will likely display the avatar during the next round of repaint
                _gravatarService
                    .GetAvatarAsync(revision.AuthorEmail, AppSettings.AuthorImageSize, AppSettings.GravatarDefaultImageType)
                    .FileAndForget();
            }

            var size = e.CellBounds.Height;
            var top = e.CellBounds.Top;
            var left = e.CellBounds.Left;

            _column.Width = size;

            e.Graphics.DrawImage(gravatar, left, top, size, size);
        }
    }
}