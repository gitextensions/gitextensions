using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.Properties;
using Gravatar;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class AvatarColumnProvider : ColumnProvider
    {
        private readonly IImageCache _avatarCache;
        private readonly IAvatarService _gravatarService;
        private readonly IImageNameProvider _avatarImageNameProvider;

        public AvatarColumnProvider(RevisionDataGridView revisionGridView)
            : base("Avatar")
        {
            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                HeaderText = "Avatar",
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Resizable = DataGridViewTriState.False,
                Width = DpiUtil.Scale(32)
            };

            _avatarImageNameProvider = new AvatarImageNameProvider();
            _avatarCache = new DirectoryImageCache(AppSettings.GravatarCachePath, AppSettings.AuthorImageCacheDays);
            _avatarCache.Invalidated += (s, e) => revisionGridView.Invalidate();
            _gravatarService = new GravatarService(_avatarCache, _avatarImageNameProvider);
        }

        public override void Refresh() => Column.Visible = AppSettings.ShowAuthorAvatarColumn;

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, (Brush backBrush, Color backColor, Color foreColor, Font normalFont, Font boldFont) style)
        {
            if (revision.IsArtificial)
            {
                return;
            }

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

            Column.Width = e.CellBounds.Height;

            const int padding = 3;

            var imageSize = e.CellBounds.Height - padding - padding;

            var rect = new Rectangle(
                e.CellBounds.Left + padding,
                e.CellBounds.Top + padding,
                imageSize,
                imageSize);

            var container = e.Graphics.BeginContainer();
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImage(gravatar, rect);
            e.Graphics.EndContainer(container);

            e.Graphics.FillRectangle(style.backBrush, rect.Left, rect.Top, 2, 1);
            e.Graphics.FillRectangle(style.backBrush, rect.Left, rect.Top, 1, 2);

            e.Graphics.FillRectangle(style.backBrush, rect.Right - 2, rect.Top, 2, 1);
            e.Graphics.FillRectangle(style.backBrush, rect.Right - 1, rect.Top, 1, 2);

            e.Graphics.FillRectangle(style.backBrush, rect.Left, rect.Bottom - 1, 2, 1);
            e.Graphics.FillRectangle(style.backBrush, rect.Left, rect.Bottom - 2, 1, 2);

            e.Graphics.FillRectangle(style.backBrush, rect.Right - 2, rect.Bottom - 1, 2, 1);
            e.Graphics.FillRectangle(style.backBrush, rect.Right - 1, rect.Bottom - 2, 1, 2);
        }
    }
}