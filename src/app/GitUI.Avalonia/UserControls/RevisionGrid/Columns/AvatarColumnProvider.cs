using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GitCommands;
using GitUI.Avatars;
using GitUI.Properties;
using GitUIPluginInterfaces;

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
            if (identityChanged)
            {
                ReplaceSource(_placeholderImage);
            }

            LastLoadTask = LoadCoreAsync(email, name, imageSize, requestVersion);
        }

        private async Task<byte[]?> LoadCoreAsync(string email, string? name, int imageSize, int requestVersion)
        {
            byte[]? imageData = await _avatarProvider.GetAvatarAsync(email, name, imageSize);
            Bitmap? bitmap = AvatarImage.Decode(imageData);

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (requestVersion != _requestVersion)
                {
                    bitmap?.Dispose();
                    return;
                }

                ReplaceSource(bitmap);
            });
            return imageData;
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
