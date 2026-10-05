using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GitCommands;
using GitUI.Avatars;
using GitUI.Properties;
using GitUIPluginInterfaces;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class AvatarColumnProvider : ColumnProvider
{
    private readonly RevisionGridControl _revisionGridView;
    private readonly IAvatarProvider _avatarProvider;
    private readonly IAvatarCacheCleaner _avatarCacheCleaner;
    private static readonly int _padding = 2;
    private static readonly IImage _placeholderImage = Images.User80;

    private int _avatarSize = 20;
    private int _cacheVersion;

    public AvatarColumnProvider(
        RevisionGridControl revisionGridView,
        IAvatarProvider avatarProvider,
        IAvatarCacheCleaner avatarCacheCleaner)
        : base("Avatar", new GridLength(32), minimumWidth: 5, resizable: false)
    {
        _revisionGridView = revisionGridView;
        _avatarProvider = avatarProvider;
        _avatarCacheCleaner = avatarCacheCleaner;
        _ = new CacheRefreshSubscription(
            _revisionGridView,
            _avatarCacheCleaner,
            () => Interlocked.Increment(ref _cacheVersion));
    }

    public override void ApplySettings()
    {
        Column.IsVisible = AppSettings.ShowAuthorAvatarColumn;
    }

    internal void ApplyRowHeight(double rowHeight)
    {
        // Framework constraint: Avalonia rows measure separately, so mirror the original per-paint width assignment here.
        Column.Width = new GridLength(rowHeight);
        _avatarSize = Math.Max(1, (int)Math.Round(rowHeight - (_padding * 2), MidpointRounding.AwayFromZero));
    }

    private string? _email = null;
    private string? _author = null;
    private Task<byte[]?>? _getLastAvatarTask = null;

    public override Control CreateCell()
    {
        AvatarCell image = new(_avatarProvider, _placeholderImage) { Margin = new Thickness(_padding) };
        image.Classes.Add("revision-avatar-cell");
        return image;
    }

    public override void OnCellPainting(Control control, GitRevision revision)
    {
        AvatarCell image = (AvatarCell)control;
        if (revision.IsArtificial)
        {
            image.Clear();
        }
        else
        {
            _email = revision.AuthorEmail;
            _author = revision.Author;
            image.Load(
                revision.AuthorEmail ?? string.Empty,
                revision.Author,
                Volatile.Read(ref _cacheVersion),
                _avatarSize);
            _getLastAvatarTask = image.LastLoadTask;
        }
    }

    public override bool TryGetToolTip(GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        if (revision.IsArtificial)
        {
            toolTip = null;
            return false;
        }

        toolTip = AuthorNameColumnProvider.GetAuthorAndCommiterToolTip(revision);
        return true;
    }

    internal sealed class AvatarCell : Image
    {
        private readonly IAvatarProvider _avatarProvider;
        private readonly IImage _placeholderImage;
        private StreamGeometry? _cornerClip;
        private double _cornerClipScale;
        private Size _cornerClipSize;
        private int _cacheVersion = -1;
        private string? _email;
        private int _imageSize = -1;
        private string? _name;
        private int _requestVersion;

        internal Task<byte[]?>? LastLoadTask { get; private set; }

        public AvatarCell(IAvatarProvider avatarProvider, IImage placeholderImage)
        {
            _avatarProvider = avatarProvider;
            _placeholderImage = placeholderImage;
            Stretch = Avalonia.Media.Stretch.Uniform;
        }

        public AvatarCell(IAvatarProvider avatarProvider)
            : this(avatarProvider, Images.User80)
        {
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size arranged = base.ArrangeOverride(finalSize);
            double scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
            if (_cornerClip is null || _cornerClipSize != arranged || _cornerClipScale != scale)
            {
                _cornerClipSize = arranged;
                _cornerClipScale = scale;
                _cornerClip = CreateCornerClip(arranged, scale);
                Clip = _cornerClip;
            }

            return arranged;
        }

        public void Clear()
        {
            if (_email is null && _name is null && Source is null)
            {
                return;
            }

            _email = null;
            _name = null;
            _imageSize = -1;
            _cacheVersion = -1;
            _requestVersion++;
            ReplaceSource(null);
        }

        public void Load(string email, string? name, int cacheVersion, int imageSize = 20)
        {
            bool identityChanged = _email != email || _name != name || _imageSize != imageSize;
            if (!identityChanged && _cacheVersion == cacheVersion)
            {
                LastLoadTask = null;
                return;
            }

            _email = email;
            _name = name;
            _imageSize = imageSize;
            _cacheVersion = cacheVersion;
            int requestVersion = ++_requestVersion;
            LastLoadTask = LoadCoreAsync(email, name, imageSize, requestVersion, identityChanged);
        }

        private async Task<byte[]?> LoadCoreAsync(string email, string? name, int imageSize, int requestVersion, bool identityChanged)
        {
            Task<byte[]?> imageTask = _avatarProvider.GetAvatarAsync(email, name, imageSize);
            if (identityChanged && !imageTask.IsCompletedSuccessfully)
            {
                ReplaceSource(_placeholderImage);
            }

            byte[]? imageData = await imageTask.ConfigureAwait(false);
            Bitmap? bitmap = AvatarImage.Decode(imageData);

            // Framework constraint: completed cache hits paint inline like WinForms;
            // only a genuinely asynchronous load needs to return to the owner thread.
            if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
            {
                ApplyLoadedImage(bitmap, requestVersion);
            }
            else
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => ApplyLoadedImage(bitmap, requestVersion));
            }

            return imageData;
        }

        private void ApplyLoadedImage(Bitmap? bitmap, int requestVersion)
        {
            if (requestVersion != _requestVersion)
            {
                bitmap?.Dispose();
                return;
            }

            ReplaceSource(bitmap);
        }

        private static StreamGeometry CreateCornerClip(Size size, double scale)
        {
            // The source covers each corner with two perpendicular pixel blocks,
            // independent of DPI. Clipping leaves the actual row background visible.
            const int CornerWidth = 2;
            double pixel = 1 / scale;
            double corner = CornerWidth * pixel;
            double right = size.Width;
            double bottom = size.Height;
            StreamGeometry geometry = new();
            using (StreamGeometryContext path = geometry.Open())
            {
                path.BeginFigure(new Point(corner, 0), isFilled: true);
                path.LineTo(new Point(right - corner, 0));
                path.LineTo(new Point(right - corner, pixel));
                path.LineTo(new Point(right - pixel, pixel));
                path.LineTo(new Point(right - pixel, corner));
                path.LineTo(new Point(right, corner));
                path.LineTo(new Point(right, bottom - corner));
                path.LineTo(new Point(right - pixel, bottom - corner));
                path.LineTo(new Point(right - pixel, bottom - pixel));
                path.LineTo(new Point(right - corner, bottom - pixel));
                path.LineTo(new Point(right - corner, bottom));
                path.LineTo(new Point(corner, bottom));
                path.LineTo(new Point(corner, bottom - pixel));
                path.LineTo(new Point(pixel, bottom - pixel));
                path.LineTo(new Point(pixel, bottom - corner));
                path.LineTo(new Point(0, bottom - corner));
                path.LineTo(new Point(0, corner));
                path.LineTo(new Point(pixel, corner));
                path.LineTo(new Point(pixel, pixel));
                path.LineTo(new Point(corner, pixel));
                path.EndFigure(isClosed: true);
            }

            return geometry;
        }

        private void ReplaceSource(IImage? image)
        {
            Bitmap? previous = Source as Bitmap;
            Source = image;
            if (!ReferenceEquals(previous, image)
                && !ReferenceEquals(previous, _placeholderImage))
            {
                previous?.Dispose();
            }
        }
    }

    private sealed class CacheRefreshSubscription
    {
        private readonly IAvatarCacheCleaner _avatarCacheCleaner;
        private readonly Action _invalidateCacheVersion;
        private readonly WeakReference<RevisionGridControl> _revisionGridView;

        public CacheRefreshSubscription(
            RevisionGridControl revisionGridView,
            IAvatarCacheCleaner avatarCacheCleaner,
            Action invalidateCacheVersion)
        {
            _revisionGridView = new WeakReference<RevisionGridControl>(revisionGridView);
            _avatarCacheCleaner = avatarCacheCleaner;
            _invalidateCacheVersion = invalidateCacheVersion;
            _avatarCacheCleaner.CacheCleared += OnCacheCleared;
        }

        private void OnCacheCleared(object? sender, EventArgs e)
        {
            if (_revisionGridView.TryGetTarget(out RevisionGridControl? revisionGridView))
            {
                _invalidateCacheVersion();
                revisionGridView.RefreshRealizedRows();
                return;
            }

            _avatarCacheCleaner.CacheCleared -= OnCacheCleared;
        }
    }
}
