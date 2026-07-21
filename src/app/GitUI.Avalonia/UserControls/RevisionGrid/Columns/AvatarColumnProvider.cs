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

    public AvatarColumnProvider(
        RevisionGridControl revisionGridView,
        IAvatarProvider avatarProvider,
        IAvatarCacheCleaner avatarCacheCleaner)
        : base("Avatar", new GridLength(32), minimumWidth: 32, resizable: false)
    {
        _avatarProvider = avatarProvider;
        _ = new CacheRefreshSubscription(revisionGridView, avatarCacheCleaner);
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
            image.Load(revision.AuthorEmail ?? string.Empty, revision.Author);
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

    private sealed class AvatarCell : Image
    {
        private readonly IAvatarProvider _avatarProvider;
        private string? _email;
        private string? _name;
        private int _requestVersion;

        public AvatarCell(IAvatarProvider avatarProvider)
        {
            _avatarProvider = avatarProvider;
            Stretch = Avalonia.Media.Stretch.Uniform;
            AttachedToVisualTree += (_, _) => Reload();
            DetachedFromVisualTree += (_, _) => ResetForDetach();
        }

        public void Clear()
        {
            _email = null;
            _name = null;
            _requestVersion++;
            ReplaceSource(null);
        }

        public void Load(string email, string? name)
        {
            _email = email;
            _name = name;
            int requestVersion = ++_requestVersion;
            ReplaceSource(null);
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

        private void Reload()
        {
            if (_email is not null)
            {
                Load(_email, _name);
            }
        }

        private void ResetForDetach()
        {
            _requestVersion++;
            ReplaceSource(null);
        }
    }

    private sealed class CacheRefreshSubscription
    {
        private readonly IAvatarCacheCleaner _avatarCacheCleaner;
        private readonly WeakReference<RevisionGridControl> _revisionGridView;

        public CacheRefreshSubscription(RevisionGridControl revisionGridView, IAvatarCacheCleaner avatarCacheCleaner)
        {
            _revisionGridView = new WeakReference<RevisionGridControl>(revisionGridView);
            _avatarCacheCleaner = avatarCacheCleaner;
            _avatarCacheCleaner.CacheCleared += OnCacheCleared;
        }

        private void OnCacheCleared(object? sender, EventArgs e)
        {
            if (_revisionGridView.TryGetTarget(out RevisionGridControl? revisionGridView))
            {
                revisionGridView.RefreshRealizedRows();
                return;
            }

            _avatarCacheCleaner.CacheCleared -= OnCacheCleared;
        }
    }
}
