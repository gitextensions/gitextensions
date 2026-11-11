using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using GitCommands;
using GitUI.Properties;
using Color = Avalonia.Media.Color;

namespace GitUI.Compat;

/// <summary>
/// Carries the Windows taskbar features on the macOS Dock. The thumbnail toolbar buttons and the recent
/// repositories become the Dock menu, and the progress bar and the status overlay are drawn into the
/// Dock tile image.
/// </summary>
// Cross-platform constraint: macOS has no thumbnail toolbar, jump list or overlay icon; the Dock menu
// and the Dock tile image are its equivalents.
internal sealed unsafe partial class MacDockShell : ITaskbarShell
{
    private const string ObjectiveC = "/usr/lib/libobjc.A.dylib";

    // Windows lists ten recent items in a jump list by default.
    private const int RecentLimit = 10;
    private const int TileSize = 256;

    private static readonly Color NormalProgress = Color.FromRgb(0x06, 0xB0, 0x25);
    private static readonly Color ErrorProgress = Color.FromRgb(0xE8, 0x11, 0x23);
    private static readonly Color PausedProgress = Color.FromRgb(0xFF, 0xB9, 0x00);

    private readonly Action<byte[]?> _setTileImage;
    private readonly Action<string> _openRecent;
    private NativeMenu? _menu;
    private readonly Dictionary<ThumbnailToolBarButton, NativeMenuItem> _buttonItems = [];
    private readonly List<NativeMenuItem> _recentItems = [];
    private List<ThumbnailToolBarButton> _buttons = [];
    private TaskbarProgressBarState _progressState;
    private double _progress;
    private byte[]? _overlay;
    private bool _tileChanged;

    internal MacDockShell()
        : this(SetApplicationIconImage, OpenRepository)
    {
    }

    internal MacDockShell(Action<byte[]?> setTileImage, Action<string> openRecent)
    {
        _setTileImage = setTileImage;
        _openRecent = openRecent;
    }

    public bool IsPlatformSupported => true;

    public bool HasMainWindow => FindShownWindow() is not null;

    // Avalonia objects belong to the UI thread, so the menu is created on first use there.
    internal NativeMenu Menu => _menu ??= new NativeMenu();

    public nint GetOwnerHandle()
    {
        // The Dock belongs to the application, so any shown window stands for it like a Windows main window.
        Window window = FindShownWindow() ?? throw new InvalidOperationException("A valid active Window is needed to update the Taskbar.");
        return ApplicationForms.GetHandle(window);
    }

    // The bundle identifier names the Dock icon; an explicit application id has no macOS equivalent.
    public void SetApplicationId(string applicationId)
    {
    }

    public void AddButtons(nint windowHandle, IReadOnlyList<ThumbnailToolBarButton> buttons)
    {
        List<ThumbnailToolBarButton> added = [.. buttons];
        OnUIThread(() => SetButtons(windowHandle, added));
    }

    public void UpdateButton(nint windowHandle, ThumbnailToolBarButton button)
    {
        bool enabled = button.Enabled;
        OnUIThread(() =>
        {
            if (_buttonItems.TryGetValue(button, out NativeMenuItem? item))
            {
                item.IsEnabled = enabled;
            }
        });
    }

    public void RefreshJumpList(string? applicationId, JumpListKnownCategoryType knownCategory)
        => OnUIThread(() => SetRecentItems(knownCategory == JumpListKnownCategoryType.Recent ? GetRecentFiles() : []));

    // The Dock menu lists the recent repository files themselves, and writing one makes it the newest.
    public void AddToRecent(string path)
    {
    }

    // Dock tile images are drawn from the icon's image data, so no native icon handle exists.
    public nint CreateIcon(byte[] pngData, int size) => throw new PlatformNotSupportedException();

    public void DestroyIcon(nint icon) => throw new PlatformNotSupportedException();

    public void SetProgressState(nint windowHandle, TaskbarProgressBarState state)
        => OnUIThread(() =>
        {
            _progressState = state;
            UpdateTile();
        });

    public void SetProgressValue(nint windowHandle, ulong completed, ulong total)
        => OnUIThread(() =>
        {
            // Like the Windows taskbar, a value shows a normal bar unless an error or paused state is set.
            if (_progressState is not (TaskbarProgressBarState.Error or TaskbarProgressBarState.Paused))
            {
                _progressState = TaskbarProgressBarState.Normal;
            }

            _progress = total == 0 ? 0 : Math.Clamp((double)completed / total, 0, 1);
            UpdateTile();
        });

    public void SetOverlayIcon(nint windowHandle, TaskbarIcon? icon, string description)
    {
        byte[]? overlay = icon?.PngData;
        OnUIThread(() =>
        {
            _overlay = overlay;
            UpdateTile();
        });
    }

    private static IEnumerable<Window> GetWindows()
    {
        IEnumerable<Window> windows = WindowsCloseRequests.GetOpenForms();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            windows = desktop.Windows.Concat(windows).Distinct();
        }

        return windows;
    }

    private static Window? FindShownWindow() => GetWindows().FirstOrDefault(window => window.IsVisible);

    private static Window? FindWindow(nint windowHandle)
        => windowHandle == 0 ? null : GetWindows().FirstOrDefault(window => ApplicationForms.GetHandle(window) == windowHandle);

    private static IReadOnlyList<string> GetRecentFiles()
    {
        string recentFolder = Path.Join(AppSettings.ApplicationDataPath.Value!, "Recent");
        if (!Directory.Exists(recentFolder))
        {
            return [];
        }

        return Directory.EnumerateFiles(recentFolder, "*.gitext")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .Take(RecentLimit)
            .ToArray();
    }

    private static void OnUIThread(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
        }
        else
        {
            Dispatcher.UIThread.Post(action);
        }
    }

    private void SetButtons(nint windowHandle, List<ThumbnailToolBarButton> added)
    {
        // The Dock menu is shared by the whole application, so the last window's buttons replace earlier ones.
        _buttons = added;
        _buttonItems.Clear();
        foreach (ThumbnailToolBarButton button in _buttons)
        {
            // Dock menu items carry no images on macOS, so only the caption and enabled state follow.
            NativeMenuItem item = new(button.Tooltip) { IsEnabled = button.Enabled };
            item.Click += (_, _) => button.OnClick();
            _buttonItems[button] = item;
        }

        // Like thumbnail buttons, the items leave together with their window.
        if (FindWindow(windowHandle) is { } window)
        {
            window.Closed += (_, _) =>
            {
                if (ReferenceEquals(_buttons, added))
                {
                    _buttons = [];
                    _buttonItems.Clear();
                    Rebuild();
                }
            };
        }

        Rebuild();
    }

    private void SetRecentItems(IReadOnlyList<string> paths)
    {
        _recentItems.Clear();
        foreach (string path in paths)
        {
            NativeMenuItem item = new(Path.GetFileNameWithoutExtension(path));
            item.Click += (_, _) => _openRecent(path);
            _recentItems.Add(item);
        }

        Rebuild();
    }

    private void Rebuild()
    {
        NativeMenu menu = Menu;
        menu.Items.Clear();
        foreach (NativeMenuItem item in _recentItems)
        {
            menu.Items.Add(item);
        }

        if (_recentItems.Count > 0 && _buttons.Count > 0)
        {
            menu.Items.Add(new NativeMenuItemSeparator());
        }

        foreach (ThumbnailToolBarButton button in _buttons)
        {
            menu.Items.Add(_buttonItems[button]);
        }

        if (Application.Current is { } application && !ReferenceEquals(NativeDock.GetMenu(application), menu))
        {
            NativeDock.SetMenu(application, menu);
        }
    }

    private void UpdateTile()
    {
        if (_progressState == TaskbarProgressBarState.NoProgress && _overlay is null)
        {
            // Nothing to show restores the bundle icon, once something replaced it.
            if (_tileChanged)
            {
                _tileChanged = false;
                _setTileImage(null);
            }

            return;
        }

        _tileChanged = true;
        _setTileImage(RenderTile());
    }

    private byte[] RenderTile()
    {
        using RenderTargetBitmap tile = new(new PixelSize(TileSize, TileSize));
        using (DrawingContext context = tile.CreateDrawingContext())
        {
            context.DrawImage(Images.GitExtensionsLogo256, new Rect(0, 0, TileSize, TileSize));

            if (_overlay is not null)
            {
                // The Windows taskbar draws the overlay at half the button size in its bottom-right corner.
                using Bitmap overlay = new(new MemoryStream(_overlay));
                context.DrawImage(overlay, new Rect(TileSize / 2, TileSize / 2, TileSize / 2, TileSize / 2));
            }

            if (_progressState != TaskbarProgressBarState.NoProgress)
            {
                DrawProgress(context);
            }
        }

        using MemoryStream stream = new();
        tile.Save(stream, PngBitmapEncoderOptions.Default);
        return stream.ToArray();
    }

    private void DrawProgress(DrawingContext context)
    {
        Rect bar = new(24, TileSize - 56, TileSize - 48, 32);
        context.DrawRectangle(new SolidColorBrush(Color.FromArgb(0xC0, 0x20, 0x20, 0x20)), new Pen(Brushes.White, 2), bar, 8, 8);

        Rect inner = bar.Deflate(4);
        switch (_progressState)
        {
            case TaskbarProgressBarState.Indeterminate:
                // A Dock tile cannot animate a marquee, so the whole bar shows in a lighter normal color.
                context.DrawRectangle(new SolidColorBrush(NormalProgress, 0.5), null, inner, 4, 4);
                break;
            default:
                Color color = _progressState switch
                {
                    TaskbarProgressBarState.Error => ErrorProgress,
                    TaskbarProgressBarState.Paused => PausedProgress,
                    _ => NormalProgress,
                };

                if (_progress > 0)
                {
                    context.DrawRectangle(new SolidColorBrush(color), null, inner.WithWidth(inner.Width * _progress), 4, 4);
                }

                break;
        }
    }

    private static void OpenRepository(string path)
    {
        // Like the original's repository file association, a recent repository opens in a new instance.
        ProcessStartInfo start = new(Environment.ProcessPath!) { UseShellExecute = false };
        if (Path.GetFileNameWithoutExtension(start.FileName) == "dotnet")
        {
            // A framework-dependent run names the application assembly before its own arguments.
            start.ArgumentList.Add(Environment.GetCommandLineArgs()[0]);
        }

        start.ArgumentList.Add("openrepo");
        start.ArgumentList.Add(path);
        Process.Start(start)?.Dispose();
    }

    private static void SetApplicationIconImage(byte[]? image)
    {
        nint pool = Send(GetClass("NSAutoreleasePool"), GetSelector("new"));
        try
        {
            nint application = Send(GetClass("NSApplication"), GetSelector("sharedApplication"));
            nint nativeImage = 0;
            if (image is not null)
            {
                fixed (byte* bytes = image)
                {
                    nint data = SendBytes(GetClass("NSData"), GetSelector("dataWithBytes:length:"), bytes, (nuint)image.Length);
                    nativeImage = SendObject(Send(GetClass("NSImage"), GetSelector("alloc")), GetSelector("initWithData:"), data);
                }
            }

            // A nil image restores the application's bundle icon.
            SetObject(application, GetSelector("setApplicationIconImage:"), nativeImage);
            if (nativeImage != 0)
            {
                Send(nativeImage, GetSelector("release"));
            }
        }
        finally
        {
            if (pool != 0)
            {
                Send(pool, GetSelector("drain"));
            }
        }
    }

    [LibraryImport(ObjectiveC, EntryPoint = "objc_getClass", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint GetClass(string name);

    [LibraryImport(ObjectiveC, EntryPoint = "sel_registerName", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint GetSelector(string name);

    [LibraryImport(ObjectiveC, EntryPoint = "objc_msgSend")]
    private static partial nint Send(nint receiver, nint selector);

    [LibraryImport(ObjectiveC, EntryPoint = "objc_msgSend")]
    private static partial nint SendObject(nint receiver, nint selector, nint value);

    [LibraryImport(ObjectiveC, EntryPoint = "objc_msgSend")]
    private static partial nint SendBytes(nint receiver, nint selector, byte* bytes, nuint length);

    [LibraryImport(ObjectiveC, EntryPoint = "objc_msgSend")]
    private static partial void SetObject(nint receiver, nint selector, nint value);
}
