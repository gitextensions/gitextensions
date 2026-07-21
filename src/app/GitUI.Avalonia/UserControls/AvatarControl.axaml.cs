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
    private readonly IAvatarProvider _avatarProvider = AvatarService.DefaultProvider;
    private readonly IAvatarCacheCleaner _avatarCacheCleaner = AvatarService.CacheCleaner;
    private Bitmap? _ownedImage;

    public AvatarControl()
    {
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
        avatarProviderToolStripMenuItem.SubmenuOpened += AvatarProviderToolStripMenuItem_SubmenuOpened;
        fallbackAvatarStyleToolStripMenuItem.SubmenuOpened += OnDefaultImageSubmenuOpened;
        DetachedFromVisualTree += (_, _) => _cancellationTokenSequence.CancelCurrent();

        RefreshImage(null);
        InitializeComplete();
    }

    public string? Email { get; private set; }

    public string? AuthorName { get; private set; }

    public void ClearCache()
    {
        ThreadHelper.FileAndForget(async () =>
        {
            AvatarService.UpdateAvatarProvider();
            await _avatarCacheCleaner.ClearCacheAsync();
            await UpdateAvatarAsync();
        });
    }

    public void LoadImage(string? email, string? name)
    {
        Email = email;
        AuthorName = name;
        ThreadHelper.FileAndForget(UpdateAvatarAsync);
    }

    private void AvatarProviderToolStripMenuItem_SubmenuOpened(object? sender, EventArgs e)
    {
        UpdateMenuItemSelection(avatarProviderToolStripMenuItem.Items, AppSettings.AvatarProvider);
    }

    private void OnClearCacheClick(object? sender, EventArgs e)
    {
        ClearCache();
    }

    private void OnDefaultImageSubmenuOpened(object? sender, EventArgs e)
    {
        UpdateMenuItemSelection(fallbackAvatarStyleToolStripMenuItem.Items, AppSettings.AvatarFallbackType);
    }

    private void OnRegisterGravatarClick(object? sender, EventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser("https://www.gravatar.com");
    }

    private void RefreshImage(Bitmap? image)
    {
        Bitmap? previous = _ownedImage;
        _ownedImage = image;
        _avatarImage.Source = image ?? Images.User80;
        previous?.Dispose();
    }

    private async Task UpdateAvatarAsync()
    {
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
        {
            double imageSize = AppSettings.AuthorImageSizeInCommitInfo;
            Width = imageSize;
            Height = imageSize;
            _avatarImage.Width = imageSize;
            _avatarImage.Height = imageSize;
        });

        string? email = Email;
        if (!AppSettings.ShowAuthorAvatarInCommitInfo || string.IsNullOrWhiteSpace(email))
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => RefreshImage(null));
            return;
        }

        CancellationToken token = _cancellationTokenSequence.Next();
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
