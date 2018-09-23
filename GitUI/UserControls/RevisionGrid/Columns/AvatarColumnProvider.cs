using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.Avatars;
using GitUI.Properties;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class AvatarColumnProvider : ColumnProvider
    {
        private readonly RevisionDataGridView _revisionGridView;
        private readonly IAvatarProvider _avatarProvider;

        public AvatarColumnProvider(RevisionDataGridView revisionGridView, IAvatarProvider avatarProvider)
            : base("Avatar")
        {
            _revisionGridView = revisionGridView;
            _avatarProvider = avatarProvider;

            _avatarProvider.CacheCleared += _revisionGridView.Invalidate;

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

        public override void Refresh(int rowHeight, in VisibleRowRange range) => Column.Visible = AppSettings.ShowAuthorAvatarColumn;

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style)
        {
            if (revision.IsArtificial)
            {
                return;
            }

            Column.Width = e.CellBounds.Height;

            var padding = DpiUtil.Scale(2);
            var imageSize = e.CellBounds.Height - padding - padding;

            Image image;
            var imageTask = _avatarProvider.GetAvatarAsync(revision.AuthorEmail, imageSize);

            if (imageTask.Status == TaskStatus.RanToCompletion)
            {
                #pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                image = imageTask.Result;
                #pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            }
            else
            {
                // Image is not yet loaded -- use a placeholder for now
                image = Images.User80;

                // One the image has loaded, invalidate for repaint
                imageTask.ContinueWith(
                    t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion)
                        {
                            _revisionGridView.Invalidate();
                        }
                    }, TaskScheduler.Current)
                    .FileAndForget();
            }

            var rect = new Rectangle(
                e.CellBounds.Left + padding,
                e.CellBounds.Top + padding,
                imageSize,
                imageSize);

            var container = e.Graphics.BeginContainer();
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImage(image, rect);
            e.Graphics.EndContainer(container);

            e.Graphics.FillRectangle(style.BackBrush, rect.Left, rect.Top, 2, 1);
            e.Graphics.FillRectangle(style.BackBrush, rect.Left, rect.Top, 1, 2);

            e.Graphics.FillRectangle(style.BackBrush, rect.Right - 2, rect.Top, 2, 1);
            e.Graphics.FillRectangle(style.BackBrush, rect.Right - 1, rect.Top, 1, 2);

            e.Graphics.FillRectangle(style.BackBrush, rect.Left, rect.Bottom - 1, 2, 1);
            e.Graphics.FillRectangle(style.BackBrush, rect.Left, rect.Bottom - 2, 1, 2);

            e.Graphics.FillRectangle(style.BackBrush, rect.Right - 2, rect.Bottom - 1, 2, 1);
            e.Graphics.FillRectangle(style.BackBrush, rect.Right - 1, rect.Bottom - 2, 1, 2);
        }
    }
}