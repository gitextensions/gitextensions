using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GitCommands.UserRepositoryHistory;
using GitUI;
using GitUI.Compat;
using GitUI.UserControls;
using NSubstitute;
using ResourceManager;

namespace GitExtensionsTests;

/// <summary>
/// Drives the real taskbar twins through the Linux launcher shell. The session-bus signal is recorded
/// instead of sent, and the shared bookmark file is a temporary file.
/// </summary>
[TestFixture]
[NonParallelizable]
public sealed class LinuxLauncherShellTests
{
    private const nint WindowHandle = 0x5153;
    private const string DesktopId = "com.example.GitExtensionsLauncherTest";
    private const string OpenCommand = "gitextensions-test browse %f";
    private static readonly XNamespace Bookmark = "http://www.freedesktop.org/standards/desktop-bookmarks";
    private static readonly XNamespace Mime = "http://www.freedesktop.org/standards/shared-mime-info";

    private AppSettingsTestScope _settingsScope = null!;
    private ITaskbarShell _previousShell = null!;
    private Func<TopLevel, nint>? _previousHandle;
    private List<(string Uri, LauncherEntryUpdate Update)> _updates = null!;
    private string _directory = null!;
    private string _bookmarkFile = null!;
    private LinuxLauncherShell _shell = null!;
    private GitExtensionsFormBase? _window;

    [SetUp]
    public void SetUp()
    {
        _settingsScope = new AppSettingsTestScope();
        _previousShell = TaskbarManager.TestAccessor.Shell;
        _previousHandle = ApplicationForms.TestAccessor.Handle;
        _updates = [];
        _directory = Path.Join(Path.GetTempPath(), $"launcher-shell-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_directory);
        _bookmarkFile = Path.Join(_directory, "data", "recently-used.xbel");
        _shell = new LinuxLauncherShell((uri, update) => _updates.Add((uri, update)), () => _bookmarkFile, DesktopId, OpenCommand);
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
        TaskbarManager.TestAccessor.Shell = _previousShell;
        ApplicationForms.TestAccessor.Handle = _previousHandle;
        _settingsScope.Dispose();
        Directory.Delete(_directory, recursive: true);
    }

    [AvaloniaTest]
    public void Launcher_entry_should_follow_the_taskbar_progress()
    {
        ShowWindow();

        TaskbarProgress.SetProgress(TaskbarProgressBarState.Normal, 50, 100);
        TaskbarProgress.SetProgress(TaskbarProgressBarState.Error, 100, 100);
        TaskbarProgress.SetState(TaskbarProgressBarState.Indeterminate);

        // A value without a blocking state shows a normal bar again, like the Windows taskbar.
        TaskbarManager.Instance.SetProgressValue(25, 100);
        TaskbarProgress.Clear();

        _updates.Select(sent => sent.Uri).Should().AllBe($"application://{DesktopId}.desktop");
        _updates.Select(sent => sent.Update).Should().Equal(
            new LauncherEntryUpdate(0, ProgressVisible: true, Urgent: false),
            new LauncherEntryUpdate(0.5, ProgressVisible: true, Urgent: false),
            new LauncherEntryUpdate(1, ProgressVisible: true, Urgent: false),
            new LauncherEntryUpdate(1, ProgressVisible: false, Urgent: false),
            new LauncherEntryUpdate(0.25, ProgressVisible: true, Urgent: false),
            new LauncherEntryUpdate(0, ProgressVisible: false, Urgent: false));
    }

    [AvaloniaTest]
    public void Status_overlay_should_request_attention_unless_clean_and_mark_the_window_icon()
    {
        ShowWindow();
        WindowIcon? originalIcon = _window!.Icon;

        // The window is named by its handle, as the taskbar owner names it; windows other tests left
        // open do not take its place.
        using TaskbarIcon untracked = DrawBrowseOverlay(RepoStateVisualiser.UntrackedOnly.Item2);
        _shell.SetOverlayIcon(WindowHandle, untracked, "");
        _updates[^1].Update.Urgent.Should().BeTrue("a working directory with changes requests attention");
        _window.Icon.Should().NotBeSameAs(originalIcon, "the window icon carries the status dot");

        using TaskbarIcon clean = DrawBrowseOverlay(RepoStateVisualiser.Clean.Item2);
        _shell.SetOverlayIcon(WindowHandle, clean, "");
        _updates[^1].Update.Urgent.Should().BeFalse();
        _window.Icon.Should().NotBeSameAs(originalIcon, "the clean status still shows its dot");

        int sent = _updates.Count;
        _shell.SetOverlayIcon(WindowHandle, null, "");
        _updates.Should().HaveCount(sent, "an unchanged launcher state is not sent again");
        _window.Icon.Should().BeSameAs(originalIcon, "removing the overlay restores the window icon");
    }

    [AvaloniaTest]
    public void Clean_status_should_be_read_back_from_the_Browse_overlay_colors()
    {
        (IBrush Brush, bool Clean)[] cases =
        [
            (RepoStateVisualiser.Clean.Item2, true),
            (RepoStateVisualiser.Dirty.Item2, false),
            (RepoStateVisualiser.DirtySubmodules.Item2, false),
            (RepoStateVisualiser.Mixed.Item2, false),
            (RepoStateVisualiser.Staged.Item2, false),
            (RepoStateVisualiser.Unknown.Item2, false),
            (RepoStateVisualiser.UntrackedOnly.Item2, false),
        ];

        foreach ((IBrush brush, bool expected) in cases)
        {
            using TaskbarIcon overlay = DrawBrowseOverlay(brush);
            LinuxLauncherShell.ShowsCleanStatus(overlay.PngData).Should().Be(expected, "{0} is {1}", brush, expected ? "clean" : "not clean");
        }
    }

    [AvaloniaTest]
    public void AddToRecent_should_record_the_repository_in_the_shared_bookmark_file()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_bookmarkFile)!);
        File.WriteAllText(_bookmarkFile, """
            <?xml version="1.0" encoding="UTF-8"?>
            <xbel version="1.0" xmlns:bookmark="http://www.freedesktop.org/standards/desktop-bookmarks" xmlns:mime="http://www.freedesktop.org/standards/shared-mime-info">
              <bookmark href="file:///home/user/notes.txt" added="2026-01-01T00:00:00.000000Z" modified="2026-01-01T00:00:00.000000Z" visited="2026-01-01T00:00:00.000000Z">
                <info>
                  <metadata owner="http://freedesktop.org">
                    <mime:mime-type type="text/plain"/>
                    <bookmark:applications>
                      <bookmark:application name="gedit" exec="&apos;gedit %u&apos;" modified="2026-01-01T00:00:00.000000Z" count="1"/>
                    </bookmark:applications>
                  </metadata>
                </info>
              </bookmark>
            </xbel>
            """);
        string workingDir = Path.Join(_directory, "launcher repository");
        Directory.CreateDirectory(workingDir);
        IRepositoryDescriptionProvider descriptions = Substitute.For<IRepositoryDescriptionProvider>();
        descriptions.GetDescriptiveUnique(workingDir).Returns("Launcher repository");
        using WindowsJumpListManager manager = new(descriptions);

        manager.CreateJumpList(WindowHandle, CreateButtons());
        manager.AddToRecent(workingDir);
        manager.AddToRecent(workingDir);

        XElement root = XDocument.Load(_bookmarkFile).Root!;
        root.Elements("bookmark").Select(element => (string?)element.Attribute("href"))
            .Should().Equal("file:///home/user/notes.txt", new Uri(workingDir).AbsoluteUri);
        XElement bookmark = root.Elements("bookmark").Last();
        XElement title = bookmark.Elements().First();
        title.Name.LocalName.Should().Be("title", "the bookmark format lists the title first");
        title.Value.Should().Be("Launcher repository");
        XElement metadata = bookmark.Element("info")!.Element("metadata")!;
        ((string?)metadata.Attribute("owner")).Should().Be("http://freedesktop.org");
        ((string?)metadata.Element(Mime + "mime-type")!.Attribute("type")).Should().Be("inode/directory");
        XElement application = metadata.Element(Bookmark + "applications")!.Elements(Bookmark + "application").Should().ContainSingle().Subject;
        ((string?)application.Attribute("name")).Should().Be(DesktopId);
        ((string?)application.Attribute("exec")).Should().Be($"'{OpenCommand}'");
        ((string?)application.Attribute("count")).Should().Be("2");
        ((string?)bookmark.Attribute("added")).Should().MatchRegex(@"^\d{4}-\d\d-\d\dT\d\d:\d\d:\d\d\.\d{6}Z$");
        Directory.EnumerateFiles(Path.GetDirectoryName(_bookmarkFile)!).Should().ContainSingle("the temporary file is replaced into place");
    }

    [AvaloniaTest]
    public void AddToRecent_should_leave_a_damaged_bookmark_file_unchanged()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_bookmarkFile)!);
        File.WriteAllText(_bookmarkFile, "not a bookmark file");
        string workingDir = Path.Join(_directory, "repository");
        Directory.CreateDirectory(workingDir);
        IRepositoryDescriptionProvider descriptions = Substitute.For<IRepositoryDescriptionProvider>();
        descriptions.GetDescriptiveUnique(workingDir).Returns("repository");
        using WindowsJumpListManager manager = new(descriptions);
        manager.CreateJumpList(WindowHandle, CreateButtons());

        manager.Invoking(m => m.AddToRecent(workingDir)).Should().NotThrow();

        File.ReadAllText(_bookmarkFile).Should().Be("not a bookmark file");
    }

    [AvaloniaTest]
    public void Windows_without_a_platform_handle_should_keep_a_stable_stand_in_handle()
    {
        ApplicationForms.TestAccessor.Handle = null;
        GitExtensionsFormBase first = new();
        GitExtensionsFormBase second = new();

        nint handle = ApplicationForms.GetHandle(first);

        handle.Should().NotBe(0, "a WinForms form always has a handle");
        ApplicationForms.GetHandle(first).Should().Be(handle);
        ApplicationForms.GetHandle(second).Should().NotBe(handle);
    }

    [Test]
    public void Launcher_shell_should_need_no_native_icons()
    {
        _shell.IsPlatformSupported.Should().BeTrue();
        _shell.Invoking(shell => shell.CreateIcon([], 48)).Should().Throw<PlatformNotSupportedException>();
        _shell.Invoking(shell => shell.DestroyIcon(1)).Should().Throw<PlatformNotSupportedException>();
    }

    private void ShowWindow()
    {
        _window = new GitExtensionsFormBase();
        _window.Show();
    }

    /// <summary>
    /// Draws the overlay the way Browse draws its working-directory status dot.
    /// </summary>
    private static TaskbarIcon DrawBrowseOverlay(IBrush brush)
    {
        const int imgDim = 32;
        const int dotDim = 15;
        const int pad = 2;
        using RenderTargetBitmap bitmap = new(new PixelSize(imgDim, imgDim));
        using (DrawingContext context = bitmap.CreateDrawingContext())
        {
            context.DrawEllipse(brush, null, new Rect(imgDim - dotDim - pad, imgDim - dotDim - pad, dotDim, dotDim));
        }

        return TaskbarIcon.FromBitmap(bitmap);
    }

    private static WindowsThumbnailToolbarButtons CreateButtons()
        => new(
            new WindowsThumbnailToolbarButton("Commit", CreateImage(), (_, _) => { }),
            new WindowsThumbnailToolbarButton("Pull", CreateImage(), (_, _) => { }),
            new WindowsThumbnailToolbarButton("Push", CreateImage(), (_, _) => { }),
            new WindowsThumbnailToolbarButton("Close all", CreateImage(), (_, _) => { }));

    private static RenderTargetBitmap CreateImage()
    {
        RenderTargetBitmap bitmap = new(new PixelSize(16, 16));
        using DrawingContext context = bitmap.CreateDrawingContext();
        context.FillRectangle(Brushes.Blue, new Rect(0, 0, 16, 16));
        return bitmap;
    }
}
