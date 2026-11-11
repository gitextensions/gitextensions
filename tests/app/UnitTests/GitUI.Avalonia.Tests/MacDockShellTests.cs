using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitUI;
using GitUI.Compat;
using NSubstitute;
using ResourceManager;

namespace GitExtensionsTests;

/// <summary>
/// Drives the real taskbar twins through the macOS Dock shell. Only the two native AppKit boundaries,
/// the Dock tile image and the new-instance launch, are recorded instead of performed.
/// </summary>
[TestFixture]
[NonParallelizable]
public sealed class MacDockShellTests
{
    private const nint WindowHandle = 0x5152;
    private AppSettingsTestScope _settingsScope = null!;
    private ITaskbarShell _previousShell = null!;
    private Func<TopLevel, nint>? _previousHandle;
    private List<byte[]?> _tileImages = null!;
    private List<string> _opened = null!;
    private MacDockShell _shell = null!;
    private GitExtensionsFormBase? _window;

    [SetUp]
    public void SetUp()
    {
        _settingsScope = new AppSettingsTestScope();
        _previousShell = TaskbarManager.TestAccessor.Shell;
        _previousHandle = ApplicationForms.TestAccessor.Handle;
        _tileImages = [];
        _opened = [];
        _shell = new MacDockShell(_tileImages.Add, _opened.Add);
        TaskbarManager.TestAccessor.Shell = _shell;
        ApplicationForms.TestAccessor.Handle = window => ReferenceEquals(window, _window) ? WindowHandle : 0;

        // The application's open forms are tracked from the first use of the close-request helper on.
        _ = WindowsCloseRequests.GetOpenForms();
    }

    [TearDown]
    public void TearDown()
    {
        if (_window?.IsVisible is true)
        {
            _window.Close();
        }

        _window = null;
        if (Application.Current is { } application)
        {
            NativeDock.SetMenu(application, null);
        }

        TaskbarManager.TestAccessor.Shell = _previousShell;
        ApplicationForms.TestAccessor.Handle = _previousHandle;
        _settingsScope.Dispose();
    }

    [AvaloniaTest]
    public void Dock_menu_should_carry_the_recent_repositories_and_the_original_thumbnail_buttons()
    {
        string recentFolder = Path.Join(AppSettings.ApplicationDataPath.Value!, "Recent");
        Directory.CreateDirectory(recentFolder);
        DateTime oldest = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        for (int i = 0; i < 12; i++)
        {
            string file = Path.Join(recentFolder, $"repository {i:00}.gitext");
            File.WriteAllText(file, $"/repositories/{i}");
            File.SetLastWriteTimeUtc(file, oldest.AddMinutes(i));
        }

        IRepositoryDescriptionProvider descriptions = Substitute.For<IRepositoryDescriptionProvider>();
        descriptions.GetDescriptiveUnique("/repositories/new").Returns("new repository");
        List<string> clicked = [];
        using WindowsJumpListManager manager = new(descriptions);

        manager.NeedsJumpListCreation.Should().BeTrue();
        manager.CreateJumpList(WindowHandle, CreateButtons(clicked));

        NativeDock.GetMenu(Application.Current!).Should().BeSameAs(_shell.Menu);
        string[] newestTen = [.. Enumerable.Range(2, 10).Reverse().Select(i => $"repository {i:00}")];
        Headers().Should().Equal([.. newestTen, "-", "Commit", "Pull", "Push", "Close all"]);

        foreach (NativeMenuItem item in Items().TakeLast(4))
        {
            Click(item);
        }

        clicked.Should().Equal("commit", "pull", "push", "close all");

        Click(Items().First());
        _opened.Should().Equal(Path.Join(recentFolder, "repository 11.gitext"));

        // A repository opened in Browse becomes the newest recent entry, as in the Windows jump list.
        manager.AddToRecent("/repositories/new");
        Headers().First().Should().Be("new repository");
        Headers().Should().HaveCount(10 + 1 + 4);
    }

    [AvaloniaTest]
    public void Dock_menu_should_follow_the_thumbnail_enablement_and_leave_with_its_window()
    {
        _window = new GitExtensionsFormBase();
        _window.Show();
        using WindowsJumpListManager manager = new(Substitute.For<IRepositoryDescriptionProvider>());
        manager.CreateJumpList(WindowHandle, CreateButtons([]));

        manager.EnableThumbnailToolbar(false);
        Items().Select(item => item.IsEnabled).Should().Equal(false, false, false, true);

        manager.EnableThumbnailToolbar(true);
        Items().Should().OnlyContain(item => item.IsEnabled);

        _window.Close();
        Dispatcher.UIThread.RunJobs();
        _shell.Menu.Items.Should().BeEmpty();
    }

    [AvaloniaTest]
    public void Dock_tile_should_show_taskbar_progress_while_a_window_is_shown()
    {
        _window = new GitExtensionsFormBase();
        _window.Show();

        TaskbarProgress.SetProgress(TaskbarProgressBarState.Normal, 50, 100);
        using (Bitmap tile = Decode(_tileImages[^1]))
        {
            tile.PixelSize.Should().Be(new PixelSize(256, 256));
            Pixel(tile, 60, 216).Should().Be((0x25, 0xB0, 0x06, 0xFF), "the first half of the bar shows the normal progress color");
            Pixel(tile, 200, 216).Should().NotBe((0x25, 0xB0, 0x06, 0xFF), "the second half is the empty track");
        }

        TaskbarProgress.SetProgress(TaskbarProgressBarState.Error, 100, 100);
        using (Bitmap tile = Decode(_tileImages[^1]))
        {
            Pixel(tile, 200, 216).Should().Be((0x23, 0x11, 0xE8, 0xFF), "a failed command fills the bar in the error color");
        }

        TaskbarProgress.SetState(TaskbarProgressBarState.Indeterminate);
        using (Bitmap tile = Decode(_tileImages[^1]))
        {
            (byte b, byte g, byte r, byte a) = Pixel(tile, 200, 216);
            g.Should().BeGreaterThan(r, "the indeterminate bar is a lighter normal color across the track");
        }

        // A value without a blocking state shows a normal bar again, like the Windows taskbar.
        TaskbarManager.Instance.SetProgressValue(25, 100);
        using (Bitmap tile = Decode(_tileImages[^1]))
        {
            Pixel(tile, 60, 216).Should().Be((0x25, 0xB0, 0x06, 0xFF));
            Pixel(tile, 150, 216).Should().NotBe((0x25, 0xB0, 0x06, 0xFF));
        }

        TaskbarProgress.Clear();
        _tileImages[^1].Should().BeNull("clearing the progress restores the bundle icon");

        int count = _tileImages.Count;
        TaskbarProgress.Clear();
        _tileImages.Should().HaveCount(count, "an unchanged bundle icon is not set again");
    }

    [AvaloniaTest]
    public void Dock_tile_should_carry_the_status_overlay_in_the_taskbar_corner()
    {
        _window = new GitExtensionsFormBase();
        _window.Show();
        using RenderTargetBitmap red = new(new PixelSize(32, 32));
        using (DrawingContext context = red.CreateDrawingContext())
        {
            context.FillRectangle(Brushes.Red, new Rect(0, 0, 32, 32));
        }

        using TaskbarIcon overlay = TaskbarIcon.FromBitmap(red);
        TaskbarManager.Instance.SetOverlayIcon(overlay, "");

        using (Bitmap tile = Decode(_tileImages[^1]))
        {
            Pixel(tile, 192, 192).Should().Be((0x00, 0x00, 0xFF, 0xFF), "the overlay covers the bottom-right quarter");
            Pixel(tile, 64, 64).Should().NotBe((0x00, 0x00, 0xFF, 0xFF), "the application icon stays visible elsewhere");
        }

        TaskbarManager.Instance.SetOverlayIcon(null, "");
        _tileImages[^1].Should().BeNull();
    }

    [AvaloniaTest]
    public void Close_all_should_reach_this_instances_installed_windows()
    {
        Action<TopLevel, Win32Properties.CustomWndProcHookCallback>? previousAttach = WindowsCloseRequests.TestAccessor.Attach;
        Action<TopLevel, Win32Properties.CustomWndProcHookCallback>? previousDetach = WindowsCloseRequests.TestAccessor.Detach;
        List<(nint Window, uint Message)> received = [];
        try
        {
            WindowsCloseRequests.TestAccessor.Attach = (_, _) => { };
            WindowsCloseRequests.TestAccessor.Detach = (_, _) => { };
            _window = new GitExtensionsFormBase();
            WindowsCloseRequests.Install(_window, (nint handle, uint message, nint wordParameter, nint longParameter, ref bool handled) =>
            {
                received.Add((handle, message));
                return 0;
            });
            _window.Show();

            WindowsCloseRequests.DispatchToInstalledWindows(0xc124);
            received.Should().Equal((WindowHandle, 0xc124u));

            _window.Close();
            WindowsCloseRequests.DispatchToInstalledWindows(0xc124);
            received.Should().HaveCount(1, "a closed window no longer receives the request");
        }
        finally
        {
            WindowsCloseRequests.TestAccessor.Attach = previousAttach;
            WindowsCloseRequests.TestAccessor.Detach = previousDetach;
        }
    }

    [Test]
    public void Dock_shell_should_need_no_native_icons()
    {
        _shell.IsPlatformSupported.Should().BeTrue();
        _shell.Invoking(shell => shell.CreateIcon([], 48)).Should().Throw<PlatformNotSupportedException>();
        _shell.Invoking(shell => shell.DestroyIcon(1)).Should().Throw<PlatformNotSupportedException>();
    }

    private IEnumerable<NativeMenuItem> Items() => _shell.Menu.Items.OfType<NativeMenuItem>();

    private IEnumerable<string> Headers()
        => _shell.Menu.Items.Select(item => item is NativeMenuItem menuItem ? menuItem.Header! : "-");

    private static void Click(NativeMenuItem item) => ((INativeMenuItemExporterEventsImplBridge)item).RaiseClicked();

    private static WindowsThumbnailToolbarButtons CreateButtons(List<string> clicked)
        => new(
            new WindowsThumbnailToolbarButton("Commit", CreateImage(), (_, _) => clicked.Add("commit")),
            new WindowsThumbnailToolbarButton("Pull", CreateImage(), (_, _) => clicked.Add("pull")),
            new WindowsThumbnailToolbarButton("Push", CreateImage(), (_, _) => clicked.Add("push")),
            new WindowsThumbnailToolbarButton("Close all", CreateImage(), (_, _) => clicked.Add("close all")));

    private static RenderTargetBitmap CreateImage()
    {
        RenderTargetBitmap bitmap = new(new PixelSize(16, 16));
        using DrawingContext context = bitmap.CreateDrawingContext();
        context.FillRectangle(Brushes.Blue, new Rect(0, 0, 16, 16));
        return bitmap;
    }

    private static Bitmap Decode(byte[]? image)
    {
        image.Should().NotBeNull();
        return new Bitmap(new MemoryStream(image!));
    }

    private static (byte B, byte G, byte R, byte A) Pixel(Bitmap bitmap, int x, int y)
    {
        byte[] pixel = new byte[4];
        GCHandle pinned = GCHandle.Alloc(pixel, GCHandleType.Pinned);
        try
        {
            bitmap.CopyPixels(new PixelRect(x, y, 1, 1), pinned.AddrOfPinnedObject(), 4, 4);
        }
        finally
        {
            pinned.Free();
        }

        return (pixel[0], pixel[1], pixel[2], pixel[3]);
    }
}
