using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using GitCommands;
using GitCommands.Utils;
using GitUI.Avatars;
using GitUI.Properties;
using ResourceManager;

namespace GitUI;

public sealed partial class AvatarControl : GitExtensionsControl
{
    private readonly CancellationTokenSequence _cancellationTokenSequence = new();
    private readonly IAvatarProvider _avatarProvider;
    private readonly IAvatarCacheCleaner _avatarCacheCleaner;
    private readonly Action<Func<Task>> _scheduleUpdate;
    private Bitmap? _ownedImage;
    private bool _isCacheClearSubscriptionActive;

    public AvatarControl()
        : this(AvatarService.DefaultProvider, AvatarService.CacheCleaner)
    {
    }

    // parity-scaffolding: offline dependencies and a per-control scheduler make cache/detach races deterministic.

    /// <summary>
    ///  Supplies offline providers and an optional queued-worker scheduler for lifecycle tests.
    /// </summary>
    internal AvatarControl(IAvatarProvider provider, IAvatarCacheCleaner cacheCleaner, Action<Func<Task>>? scheduleUpdate = null)
    {
        _avatarProvider = provider;
        _avatarCacheCleaner = cacheCleaner;
        _scheduleUpdate = scheduleUpdate ?? ThreadHelper.FileAndForget;
        InitializeComponent();

        foreach (AvatarProvider avatarProvider in EnumHelper.GetValues<AvatarProvider>())
        {
            MenuItem item = new()
            {
                Tag = avatarProvider,
                Header = avatarProvider.GetDescription(),
                ToggleType = MenuItemToggleType.Radio,
            };

            item.Click += (_, _) =>
            {
                AppSettings.AvatarProvider = avatarProvider;
                ClearCache();
            };

            avatarProviderToolStripMenuItem.Items.Add(item);
        }

        foreach (AvatarFallbackType fallbackType in EnumHelper.GetValues<AvatarFallbackType>())
        {
            MenuItem item = new()
            {
                Tag = fallbackType,
                Header = fallbackType.GetDescription(),
                ToggleType = MenuItemToggleType.Radio,
            };

            item.Click += (_, _) =>
            {
                AppSettings.AvatarFallbackType = fallbackType;
                ClearCache();
            };

            fallbackAvatarStyleToolStripMenuItem.Items.Add(item);
        }

        clearImagecacheToolStripMenuItem.Click += OnClearCacheClick;
        registerGravatarToolStripMenuItem.Click += OnRegisterGravatarClick;
        avatarProviderToolStripMenuItem.SubmenuOpened += avatarProviderToolStripMenuItem_DropDownOpening;
        fallbackAvatarStyleToolStripMenuItem.SubmenuOpened += OnDefaultImageDropDownOpening;
        RefreshImage(null);
        InitializeComplete();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        // Avalonia controls have no Dispose boundary; do not retain detached controls in the shared cache.
        lock (_cancellationTokenSequence)
        {
            _avatarCacheCleaner.CacheCleared += OnCacheCleared;
            _isCacheClearSubscriptionActive = true;
        }

        if (Email is not null)
        {
            OnCacheCleared(this, EventArgs.Empty);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        lock (_cancellationTokenSequence)
        {
            _avatarCacheCleaner.CacheCleared -= OnCacheCleared;
            _isCacheClearSubscriptionActive = false;
            _cancellationTokenSequence.CancelCurrent();
        }

        base.OnDetachedFromVisualTree(e);
    }

    public void ClearCache()
    {
        CancellationToken token = _cancellationTokenSequence.Next();
        _scheduleUpdate(async () =>
        {
            AvatarService.UpdateAvatarProvider();
            await _avatarCacheCleaner.ClearCacheAsync();
            if (!_isCacheClearSubscriptionActive)
            {
                await UpdateAvatarAsync(token);
            }
        });
    }

    public string? Email { get; private set; }

    public string? AuthorName { get; private set; }

    public void LoadImage(string? email, string? name)
    {
        Email = email;
        AuthorName = name;
        CancellationToken token = _cancellationTokenSequence.Next();
        _scheduleUpdate(() => UpdateAvatarAsync(token));
    }

    private void OnCacheCleared(object? sender, EventArgs e)
    {
        CancellationToken token;
        lock (_cancellationTokenSequence)
        {
            if (!_isCacheClearSubscriptionActive)
            {
                return;
            }

            token = _cancellationTokenSequence.Next();
        }

        _scheduleUpdate(() => UpdateAvatarAsync(token));
    }

    private void RefreshImage(Bitmap? image)
    {
        Bitmap? previous = _ownedImage;
        _ownedImage = image;
        _avatarImage.Source = image ?? Images.User80;
        previous?.Dispose();
    }

    private async Task UpdateAvatarAsync(CancellationToken token)
    {
        // Capture the token before queuing work so visual detachment also cancels a worker that has not started.
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            double imageSize = AppSettings.AuthorImageSizeInCommitInfo;
            Width = imageSize;
            Height = imageSize;
            _avatarImage.Width = imageSize;
            _avatarImage.Height = imageSize;
        }, Avalonia.Threading.DispatcherPriority.Normal, token);

        string? email = Email;
        if (!AppSettings.ShowAuthorAvatarInCommitInfo || string.IsNullOrWhiteSpace(email))
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => RefreshImage(null), Avalonia.Threading.DispatcherPriority.Normal, token);
            return;
        }

        // resize our control (I'm not using AutoSize for a reason)
        byte[]? imageData = await _avatarProvider.GetAvatarAsync(email, AuthorName, AppSettings.AuthorImageSizeInCommitInfo);
        Bitmap? image = AvatarImage.Decode(imageData);

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (token.IsCancellationRequested)
            {
                image?.Dispose();
                return;
            }

            RefreshImage(image);
        });
    }

    private void OnClearCacheClick(object? sender, EventArgs e)
    {
        ClearCache();
    }

    private void OnRegisterGravatarClick(object? sender, EventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser("https://www.gravatar.com");
    }

    private void OnDefaultImageDropDownOpening(object? sender, EventArgs e)
    {
        UpdateMenuItemSelection(fallbackAvatarStyleToolStripMenuItem.Items, AppSettings.AvatarFallbackType);
    }

    private void avatarProviderToolStripMenuItem_DropDownOpening(object? sender, EventArgs e)
    {
        UpdateMenuItemSelection(avatarProviderToolStripMenuItem.Items, AppSettings.AvatarProvider);
    }

    private static void UpdateMenuItemSelection<T>(IEnumerable<object?> menuItems, T currentValue)
    {
        foreach (MenuItem item in menuItems.OfType<MenuItem>())
        {
            item.IsChecked = Equals((T?)item.Tag, currentValue);
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(AvatarControl control)
    {
        public Image Image => control._avatarImage;

        public ContextMenu ContextMenu => control.contextMenuStrip;
    }
}
