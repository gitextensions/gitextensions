using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using GitCommands;
using GitUI.Avatars;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class AvatarColumnProvider : ColumnProvider
{
    private readonly IAvatarProvider _avatarProvider;
    private int _cacheVersion;

    public AvatarColumnProvider(
        RevisionGridControl revisionGridView,
        IAvatarProvider avatarProvider,
        IAvatarCacheCleaner avatarCacheCleaner)
        : base("Avatar", new GridLength(32), minimumWidth: 32, resizable: false)
    {
        _avatarProvider = avatarProvider;
        _ = new CacheRefreshSubscription(
            revisionGridView,
            avatarCacheCleaner,
            () => Interlocked.Increment(ref _cacheVersion));
    }

    public override void ApplySettings()
    {
        Column.IsVisible = AppSettings.ShowAuthorAvatarColumn;
    }

    public override Control CreateCell()
    {
        AvatarCell image = new(_avatarProvider) { Margin = new Thickness(2) };
        image.Classes.Add("revision-avatar-cell");
        return image;
    }

    public override void UpdateCell(Control control, GitRevision revision)
    {
        AvatarCell image = (AvatarCell)control;
        if (revision.IsArtificial)
        {
            image.Clear();
        }
        else
        {
            image.Load(
                revision.AuthorEmail ?? string.Empty,
                revision.Author,
                Volatile.Read(ref _cacheVersion));
        }

        UpdateToolTip(control, revision);
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
        private int _cacheVersion = -1;
        private string? _email;
        private string? _name;
        private int _requestVersion;

        public AvatarCell(IAvatarProvider avatarProvider)
        {
            _avatarProvider = avatarProvider;
            Stretch = Avalonia.Media.Stretch.Uniform;
        }

        public void Clear()
        {
            if (_email is null && _name is null && Source is null)
            {
                return;
            }

            _email = null;
            _name = null;
            _cacheVersion = -1;
            _requestVersion++;
            ReplaceSource(null);
        }

        public void Load(string email, string? name, int cacheVersion)
        {
            bool identityChanged = _email != email || _name != name;
            if (!identityChanged && _cacheVersion == cacheVersion)
            {
                return;
            }

            _email = email;
            _name = name;
            _cacheVersion = cacheVersion;
            int requestVersion = ++_requestVersion;
            if (identityChanged)
            {
                ReplaceSource(null);
            }

            ThreadHelper.FileAndForget(() => LoadAsync(email, name, requestVersion));
        }

        private async Task LoadAsync(string email, string? name, int requestVersion)
        {
            const int AvatarSize = 20;
            byte[]? imageData = await _avatarProvider.GetAvatarAsync(email, name, AvatarSize);
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
        }

        private void ReplaceSource(Bitmap? bitmap)
        {
            Bitmap? previous = Source as Bitmap;
            Source = bitmap;
            previous?.Dispose();
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
