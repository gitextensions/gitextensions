using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Avalonia;
using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitUI;
using GitUI.Compat;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class WindowsJumpListManagerTests
{
    private const nint WindowHandle = 0x5150;
    private AppSettingsTestScope _settingsScope = null!;
    private ITaskbarShell _previousShell = null!;
    private RecordingTaskbarShell _shell = null!;

    [SetUp]
    public void SetUp()
    {
        _settingsScope = new AppSettingsTestScope();
        _previousShell = TaskbarManager.TestAccessor.Shell;
        _shell = new RecordingTaskbarShell();
        TaskbarManager.TestAccessor.Shell = _shell;
    }

    [TearDown]
    public void TearDown()
    {
        TaskbarManager.TestAccessor.Shell = _previousShell;
        _settingsScope.Dispose();
    }

    [AvaloniaTest]
    public void CreateJumpList_should_add_the_original_thumbnail_buttons_and_show_recent_repositories()
    {
        using WindowsJumpListManager manager = new(Substitute.For<IRepositoryDescriptionProvider>());
        List<string> clicked = [];
        WindowsThumbnailToolbarButtons buttons = CreateButtons(clicked);

        manager.NeedsJumpListCreation.Should().BeTrue();
        manager.CreateJumpList(WindowHandle, buttons);

        manager.NeedsJumpListCreation.Should().BeFalse();
        _shell.JumpListRefreshes.Should().Equal(JumpListKnownCategoryType.Recent);
        _shell.AddedWindow.Should().Be(WindowHandle);
        _shell.AddedButtons.Select(button => button.Tooltip).Should().Equal("Commit", "Pull", "Push", "Close all");
        _shell.AddedButtons.Should().OnlyContain(button => button.Enabled && button.AddedToTaskbar && button.WindowHandle == WindowHandle);
        _shell.AddedButtons.Select(button => button.Icon!.Size).Should().AllBeEquivalentTo(48);
        _shell.AddedButtons.Select(button => button.Id).Should().OnlyHaveUniqueItems();

        foreach (ThumbnailToolBarButton button in _shell.AddedButtons)
        {
            ThumbnailToolBarManager.DispatchClick(_shell.AddedButtons, 0x0111, (0x1800 << 16) | (nint)button.Id).Should().BeTrue();
        }

        clicked.Should().Equal("commit", "pull", "push", "close all");

        manager.CreateJumpList(WindowHandle, buttons);
        _shell.Calls.Count(call => call == nameof(ITaskbarShell.AddButtons)).Should().Be(1);
    }

    [AvaloniaTest]
    public void CreateJumpList_should_wait_for_a_native_window()
    {
        using WindowsJumpListManager manager = new(Substitute.For<IRepositoryDescriptionProvider>());

        manager.CreateJumpList(IntPtr.Zero, CreateButtons([]));

        _shell.Calls.Should().BeEmpty();
        manager.NeedsJumpListCreation.Should().BeTrue();
    }

    [AvaloniaTest]
    public void AddToRecent_should_wait_for_the_toolbar_then_record_the_repository()
    {
        IRepositoryDescriptionProvider descriptions = Substitute.For<IRepositoryDescriptionProvider>();
        string workingDir = Path.Combine(Path.GetTempPath(), "jump list repository");
        descriptions.GetDescriptiveUnique(workingDir).Returns("Jump list repository");
        using WindowsJumpListManager manager = new(descriptions);

        manager.AddToRecent(workingDir);
        _shell.Calls.Should().BeEmpty();

        manager.CreateJumpList(WindowHandle, CreateButtons([]));

        string recentFile = Path.Join(AppSettings.ApplicationDataPath.Value!, "Recent", "Jump list repository.gitext");
        File.ReadAllText(recentFile).Should().Be(workingDir);
        _shell.RecentPaths.Should().Equal(recentFile);
        _shell.JumpListRefreshes.Should().Equal(JumpListKnownCategoryType.Recent, JumpListKnownCategoryType.Recent);
    }

    [AvaloniaTest]
    public void EnableThumbnailToolbar_should_keep_close_all_available()
    {
        using WindowsJumpListManager manager = new(Substitute.For<IRepositoryDescriptionProvider>());
        manager.EnableThumbnailToolbar(false);
        _shell.Calls.Should().BeEmpty();

        manager.CreateJumpList(WindowHandle, CreateButtons([]));
        manager.EnableThumbnailToolbar(false);

        _shell.AddedButtons.Select(button => button.Enabled).Should().Equal(false, false, false, true);
        _shell.UpdatedButtons.Select(button => button.Tooltip).Should().BeEquivalentTo("Commit", "Push", "Pull");

        manager.EnableThumbnailToolbar(true);
        _shell.AddedButtons.Should().OnlyContain(button => button.Enabled);
    }

    [AvaloniaTest]
    public void Shell_failures_should_be_traced_like_the_original()
    {
        _shell.RefreshFailure = new ShellException("refresh", unchecked((int)0x80004005));
        using WindowsJumpListManager manager = new(Substitute.For<IRepositoryDescriptionProvider>());

        manager.Invoking(m => m.CreateJumpList(WindowHandle, CreateButtons([]))).Should().NotThrow();

        _shell.Calls.Should().NotContain(nameof(ITaskbarShell.AddButtons));
        manager.NeedsJumpListCreation.Should().BeTrue();
    }

    [AvaloniaTest]
    public void Dispose_should_release_the_button_icons()
    {
        WindowsJumpListManager manager = new(Substitute.For<IRepositoryDescriptionProvider>());
        manager.CreateJumpList(WindowHandle, CreateButtons([]));
        List<nint> created = [.. _shell.CreatedIcons];

        manager.Dispose();

        _shell.DestroyedIcons.Should().BeEquivalentTo(created);
        created.Should().HaveCount(4);
    }

    [Test]
    public void Thumbnail_click_messages_should_reach_only_the_named_button()
    {
        List<string> clicked = [];
        ThumbnailToolBarButton first = CreateCompatButton("first", clicked, 7);
        ThumbnailToolBarButton second = CreateCompatButton("second", clicked, 8);
        ThumbnailToolBarButton[] buttons = [first, second];

        ThumbnailToolBarManager.DispatchClick(buttons, 0x0111, (0x1800 << 16) | 8).Should().BeTrue();
        ThumbnailToolBarManager.DispatchClick(buttons, 0x0111, (0x1800 << 16) | 9).Should().BeFalse();
        ThumbnailToolBarManager.DispatchClick(buttons, 0x0111, 7).Should().BeFalse();
        ThumbnailToolBarManager.DispatchClick(buttons, 0x0112, (0x1800 << 16) | 7).Should().BeFalse();

        clicked.Should().Equal("second");
    }

    [Test]
    public void Application_id_should_stay_separate_from_the_WinForms_application()
    {
        // A static constructor runs once per load, so a fresh copy of the assembly runs the real one here.
        // Its default shell is the production one: on Windows it changes only this test process's taskbar identity.
        AssemblyLoadContext context = new(nameof(Application_id_should_stay_separate_from_the_WinForms_application), isCollectible: true);
        try
        {
            Assembly isolated = context.LoadFromAssemblyPath(typeof(WindowsJumpListManager).Assembly.Location);
            Type manager = isolated.GetType(typeof(WindowsJumpListManager).FullName!, throwOnError: true)!;
            Type taskbar = isolated.GetType(typeof(TaskbarManager).FullName!, throwOnError: true)!;
            RuntimeHelpers.RunClassConstructor(manager.TypeHandle);

            object instance = taskbar.GetProperty(nameof(TaskbarManager.Instance), BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
            string? applicationId = (string?)taskbar.GetProperty(nameof(TaskbarManager.ApplicationId), BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(instance);
            bool supported = (bool)taskbar.GetProperty(nameof(TaskbarManager.IsPlatformSupported), BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
            supported.Should().Be(OperatingSystem.IsWindows() || OperatingSystem.IsMacOS(), "Windows has the taskbar and macOS the Dock");
            if (!supported)
            {
                applicationId.Should().BeNull();
                return;
            }

            string expected = $"{AppSettings.ApplicationId}.Avalonia";
            if (AppSettings.IsPortable())
            {
                applicationId.Should().StartWith($"{expected}.");
            }
            else
            {
                applicationId.Should().Be(expected);
            }

            if (!OperatingSystem.IsWindows())
            {
                // The macOS Dock names the application by its bundle identifier.
                return;
            }

            GetCurrentProcessExplicitAppUserModelID(out nint nativeId).Should().Be(0);
            try
            {
                Marshal.PtrToStringUni(nativeId).Should().Be(applicationId);
            }
            finally
            {
                Marshal.FreeCoTaskMem(nativeId);
            }
        }
        finally
        {
            context.Unload();
        }
    }

    [DllImport("shell32.dll", ExactSpelling = true)]
    private static extern int GetCurrentProcessExplicitAppUserModelID(out nint applicationId);

    [AvaloniaTest]
    public void Icons_should_keep_the_original_aspect_ratio()
    {
        using WindowsJumpListManager manager = new(Substitute.For<IRepositoryDescriptionProvider>());
        WindowsThumbnailToolbarButtons buttons = new(
            new WindowsThumbnailToolbarButton("Commit", CreateImage(32, 16), (_, _) => { }),
            new WindowsThumbnailToolbarButton("Pull", CreateImage(16, 16), (_, _) => { }),
            new WindowsThumbnailToolbarButton("Push", CreateImage(16, 16), (_, _) => { }),
            new WindowsThumbnailToolbarButton("Close all", CreateImage(16, 16), (_, _) => { }));

        manager.CreateJumpList(WindowHandle, buttons);

        TaskbarIcon icon = _shell.AddedButtons[0].Icon!;
        using Bitmap rendered = new(new MemoryStream(icon.PngData));
        rendered.PixelSize.Should().Be(new PixelSize(48, 48));
        byte[] pixels = new byte[48 * 48 * 4];
        GCHandle pinned = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            rendered.CopyPixels(new PixelRect(0, 0, 48, 48), pinned.AddrOfPinnedObject(), pixels.Length, 48 * 4);
        }
        finally
        {
            pinned.Free();
        }

        // A 2:1 image fills the full width and the middle 24 rows, as the original centres it.
        Alpha(pixels, x: 24, y: 5).Should().Be(0);
        Alpha(pixels, x: 24, y: 42).Should().Be(0);
        Alpha(pixels, x: 2, y: 24).Should().Be(255);
        Alpha(pixels, x: 45, y: 24).Should().Be(255);
    }

    [Test]
    public void Windows_shell_should_create_real_icons_or_report_no_support()
    {
        if (!OperatingSystem.IsWindows())
        {
            UnsupportedTaskbarShell unsupported = new();
            unsupported.IsPlatformSupported.Should().BeFalse();
            unsupported.HasMainWindow.Should().BeFalse();
            unsupported.Invoking(shell => shell.CreateIcon([], 48)).Should().Throw<PlatformNotSupportedException>();
            return;
        }

        WindowsTaskbarShell native = new();
        native.IsPlatformSupported.Should().BeTrue();
        nint handle = native.CreateIcon(Convert.FromBase64String(OnePixelPng), 1);
        handle.Should().NotBe(0);
        native.DestroyIcon(handle);
    }

    [Test]
    public void Windows_shell_should_follow_the_original_main_window_guard()
    {
        if (!OperatingSystem.IsWindows())
        {
            new UnsupportedTaskbarShell().Invoking(shell => shell.GetOwnerHandle()).Should().Throw<PlatformNotSupportedException>();
            return;
        }

        WindowsTaskbarShell native = new();
        native.HasMainWindow.Should().Be(GitCommands.Utils.EnvUtils.RunningOnWindowsWithMainWindow());
        native.HasMainWindow.Should().BeFalse("the headless test host has no native main window");
        InvalidOperationException? noOwner = null;
        try
        {
            native.GetOwnerHandle();
        }
        catch (InvalidOperationException exception)
        {
            noOwner = exception;
        }

        noOwner.Should().NotBeNull("the original taskbar owner needs a main window");

        using NativeMainWindowScope mainWindow = NativeMainWindowScope.Create();
        native.HasMainWindow.Should().BeTrue();
        native.GetOwnerHandle().Should().Be(mainWindow.Handle);
    }

    /// <summary>
    /// The original caches icons per image in a static dictionary, but disposing a manager destroys them,
    /// so a later manager in the same process reuses disposed icons and its toolbar silently fails to appear.
    /// The port keeps that upstream behavior; tests that use the shared toolbar images start from an empty cache.
    /// </summary>
    internal static void ClearSharedIconCache()
    {
        FieldInfo cache = typeof(WindowsJumpListManager).GetField("_iconByImage", BindingFlags.NonPublic | BindingFlags.Static)!;
        ((System.Collections.IDictionary)cache.GetValue(null)!).Clear();
    }

    private const string OnePixelPng = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==";

    private static int Alpha(byte[] pixels, int x, int y) => pixels[(((y * 48) + x) * 4) + 3];

    private static WindowsThumbnailToolbarButtons CreateButtons(List<string> clicked)
        => new(
            new WindowsThumbnailToolbarButton("Commit", CreateImage(16, 16), (_, _) => clicked.Add("commit")),
            new WindowsThumbnailToolbarButton("Pull", CreateImage(16, 16), (_, _) => clicked.Add("pull")),
            new WindowsThumbnailToolbarButton("Push", CreateImage(16, 16), (_, _) => clicked.Add("push")),
            new WindowsThumbnailToolbarButton("Close all", CreateImage(16, 16), (_, _) => clicked.Add("close all")));

    private static ThumbnailToolBarButton CreateCompatButton(string name, List<string> clicked, uint id)
    {
        ConstructorInfo constructor = typeof(TaskbarIcon).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, [typeof(byte[]), typeof(int)])!;
        ThumbnailToolBarButton button = new((TaskbarIcon)constructor.Invoke([Array.Empty<byte>(), 1]), name) { Id = id };
        button.Click += (_, _) => clicked.Add(name);
        return button;
    }

    private static WriteableBitmap CreateImage(int width, int height)
    {
        WriteableBitmap bitmap = new(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
        using ILockedFramebuffer buffer = bitmap.Lock();
        byte[] row = new byte[buffer.RowBytes];
        for (int x = 0; x < width; x++)
        {
            row[(x * 4) + 2] = 0xff;
            row[(x * 4) + 3] = 0xff;
        }

        for (int y = 0; y < height; y++)
        {
            Marshal.Copy(row, 0, buffer.Address + (y * buffer.RowBytes), row.Length);
        }

        return bitmap;
    }

    internal sealed class RecordingTaskbarShell : ITaskbarShell
    {
        private nint _nextIcon = 0x100;

        public List<string> Calls { get; } = [];
        public List<string> ApplicationIds { get; } = [];
        public List<JumpListKnownCategoryType> JumpListRefreshes { get; } = [];
        public List<string> RecentPaths { get; } = [];
        public List<ThumbnailToolBarButton> AddedButtons { get; } = [];
        public List<ThumbnailToolBarButton> UpdatedButtons { get; } = [];
        public List<nint> CreatedIcons { get; } = [];
        public List<nint> DestroyedIcons { get; } = [];
        public nint AddedWindow { get; private set; }
        public Exception? RefreshFailure { get; set; }

        /// <summary>Gets the owner the shell reports while it has a main window.</summary>
        public nint OwnerHandle { get; } = 0x7170;

        public bool IsPlatformSupported => true;

        /// <summary>Gets or sets whether a main window is shown; the headless host shows none by default.</summary>
        public bool HasMainWindow { get; set; }

        public nint GetOwnerHandle()
            => HasMainWindow ? OwnerHandle : throw new InvalidOperationException("A valid active Window is needed to update the Taskbar.");

        public void SetApplicationId(string applicationId)
        {
            Calls.Add(nameof(SetApplicationId));
            ApplicationIds.Add(applicationId);
        }

        public void AddButtons(nint windowHandle, IReadOnlyList<ThumbnailToolBarButton> buttons)
        {
            Calls.Add(nameof(AddButtons));
            AddedWindow = windowHandle;
            AddedButtons.AddRange(buttons);
            foreach (ThumbnailToolBarButton button in buttons)
            {
                button.Icon?.GetHandle();
            }
        }

        public void UpdateButton(nint windowHandle, ThumbnailToolBarButton button)
        {
            Calls.Add(nameof(UpdateButton));
            windowHandle.Should().Be(button.WindowHandle);
            UpdatedButtons.Add(button);
        }

        public void RefreshJumpList(string? applicationId, JumpListKnownCategoryType knownCategory)
        {
            Calls.Add(nameof(RefreshJumpList));
            if (RefreshFailure is not null)
            {
                throw RefreshFailure;
            }

            JumpListRefreshes.Add(knownCategory);
        }

        public void AddToRecent(string path)
        {
            Calls.Add(nameof(AddToRecent));
            RecentPaths.Add(path);
        }

        public nint CreateIcon(byte[] pngData, int size)
        {
            Calls.Add(nameof(CreateIcon));
            nint icon = _nextIcon++;
            CreatedIcons.Add(icon);
            return icon;
        }

        public void DestroyIcon(nint icon)
        {
            Calls.Add(nameof(DestroyIcon));
            DestroyedIcons.Add(icon);
        }

        public List<(nint Window, TaskbarProgressBarState State)> ProgressStates { get; } = [];
        public List<(nint Window, ulong Completed, ulong Total)> ProgressValues { get; } = [];
        public List<(nint Window, TaskbarIcon? Icon, string Description)> OverlayIcons { get; } = [];

        public void SetProgressState(nint windowHandle, TaskbarProgressBarState state)
        {
            Calls.Add(nameof(SetProgressState));
            ProgressStates.Add((windowHandle, state));
        }

        public void SetProgressValue(nint windowHandle, ulong completed, ulong total)
        {
            Calls.Add(nameof(SetProgressValue));
            ProgressValues.Add((windowHandle, completed, total));
        }

        public void SetOverlayIcon(nint windowHandle, TaskbarIcon? icon, string description)
        {
            Calls.Add(nameof(SetOverlayIcon));
            icon?.GetHandle();
            OverlayIcons.Add((windowHandle, icon, description));
        }
    }
}
