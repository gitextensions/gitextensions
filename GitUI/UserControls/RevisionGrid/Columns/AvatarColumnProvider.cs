using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.Avatars;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class AvatarColumnProvider : ColumnProvider
    {
        private readonly RevisionDataGridView _revisionGridView;
        private readonly IAvatarProvider _avatarProvider;
        private readonly IAvatarCacheCleaner _avatarCacheCleaner;
        private static readonly int _padding = DpiUtil.Scale(2);
        private static Bitmap _placeholderImage;

        public AvatarColumnProvider(RevisionDataGridView revisionGridView, IAvatarProvider avatarProvider, IAvatarCacheCleaner avatarCacheCleaner)
            : base("Avatar")
        {
            _revisionGridView = revisionGridView;
            _avatarProvider = avatarProvider;
            _avatarCacheCleaner = avatarCacheCleaner;

            _avatarCacheCleaner.CacheCleared += (sender, args) => _revisionGridView.Invalidate();

            Column = new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                HeaderText = "Avatar",
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Resizable = DataGridViewTriState.False,
                Width = DpiUtil.Scale(32),
                Visible = AppSettings.ShowAuthorAvatarColumn
            };
        }

        public override void ApplySettings()
        {
            Column.Visible = AppSettings.ShowAuthorAvatarColumn;
        }

        private string _email = null;
        private string _author = null;
        private Task<Image?> _getLastAvatarTask = null;

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style)
        {
            if (revision.IsArtificial || revision.AuthorEmail is null)
            {
                return;
            }

            Column.Width = e.CellBounds.Height;

            int imageSize = e.CellBounds.Height - _padding - _padding;

            Task<Image?> imageTask;

            if (_email == revision.AuthorEmail && _author == revision.Author)
            {
                imageTask = _getLastAvatarTask;
                if (imageTask.Status == TaskStatus.RanToCompletion)
                {
                    // Manage exceptional case where cached image have been cleaned by user
                    try
                    {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                        if (imageTask.Result.PixelFormat == System.Drawing.Imaging.PixelFormat.DontCare)
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
                        {
                            GetAvatar();
                        }
                    }
                    catch (Exception)
                    {
                        GetAvatar();
                    }
                }
            }
            else
            {
                GetAvatar();
                _email = revision.AuthorEmail;
                _author = revision.Author;
            }

            Rectangle rect = new(
                e.CellBounds.Left + _padding,
                e.CellBounds.Top + _padding,
                imageSize,
                imageSize);

            if (imageTask.Status != TaskStatus.RanToCompletion)
            {
                // Once the image has loaded, invalidate only the avatar area for repaint
                imageTask.ContinueWith(
                    t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                        {
                            _revisionGridView.Invalidate(rect);
                        }
                        else
                        {
                            // draw the placeholder
                            // First time, draw at the good size the placeholder image and cache it
                            if (_placeholderImage is null)
                            {
                                _placeholderImage = new Bitmap(imageSize, imageSize);
                                using Graphics g = Graphics.FromImage(_placeholderImage);
                                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                g.DrawImage(Images.User80, 0, 0, imageSize, imageSize);
                            }

                            e.Graphics.DrawImageUnscaled(_placeholderImage, rect);
                        }

                        imageTask.Dispose();
                    }, TaskScheduler.Current)
                    .FileAndForget();
                return;
            }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            Image? image = imageTask.Result;
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

            GraphicsContainer container = e.Graphics.BeginContainer();
            e.Graphics.DrawImage(image, rect);
            e.Graphics.EndContainer(container);

            // Draw above (so after) avatar image to round the corners
            // Algorithm not dependent of dpi scaling (because not working with scaling)
            // Top left corner
            e.Graphics.FillRectangle(style.BackBrush, rect.Left, rect.Top, 2, 1);
            e.Graphics.FillRectangle(style.BackBrush, rect.Left, rect.Top, 1, 2);

            // Top right corner
            e.Graphics.FillRectangle(style.BackBrush, rect.Right - 2, rect.Top, 2, 1);
            e.Graphics.FillRectangle(style.BackBrush, rect.Right - 1, rect.Top, 1, 2);

            // Bottom left corner
            e.Graphics.FillRectangle(style.BackBrush, rect.Left, rect.Bottom - 1, 2, 1);
            e.Graphics.FillRectangle(style.BackBrush, rect.Left, rect.Bottom - 2, 1, 2);

            // Bottom right corner
            e.Graphics.FillRectangle(style.BackBrush, rect.Right - 2, rect.Bottom - 1, 2, 1);
            e.Graphics.FillRectangle(style.BackBrush, rect.Right - 1, rect.Bottom - 2, 1, 2);

            return;

            void GetAvatar() =>
                _getLastAvatarTask = imageTask = _avatarProvider.GetAvatarAsync(revision.AuthorEmail, revision.Author, imageSize);
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
        {
            if (revision.ObjectId.IsArtificial)
            {
                toolTip = default;
                return false;
            }

            toolTip = AuthorNameColumnProvider.GetAuthorAndCommiterToolTip(revision);
            return true;
        }
    }
}
