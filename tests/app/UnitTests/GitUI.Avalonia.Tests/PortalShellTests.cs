using System.Runtime.CompilerServices;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using GitExtensions.Shims.WinForms;
using GitUI.Compat;
using NSubstitute;

namespace GitUI.Avalonia.Tests;

[TestFixture]
public sealed class PortalShellTests
{
    [TestCase(OsShellLaunchKind.Open, "/tmp/file.txt")]
    [TestCase(OsShellLaunchKind.OpenAs, "/tmp/file.txt")]
    [TestCase(OsShellLaunchKind.OpenDirectory, "/tmp/folder")]
    [TestCase(OsShellLaunchKind.ShowInDirectory, "/tmp/folder/file.txt")]
    [TestCase(OsShellLaunchKind.OpenUri, "https://example.com")]
    public async Task Linux_launches_route_to_xdg_desktop_portal(OsShellLaunchKind kind, string target)
    {
        IClassicDesktopStyleApplicationLifetime desktop = Substitute.For<IClassicDesktopStyleApplicationLifetime>();
        IXdgDesktopPortal portal = Substitute.For<IXdgDesktopPortal>();
        portal.TryLaunchAsync(target, kind).Returns(true);
        AvaloniaOsShell shell = new(desktop, portal, () => true);

        bool launched = await shell.TryLaunchAsync(target, kind);

        launched.Should().BeTrue();
        await portal.Received(1).TryLaunchAsync(target, kind);
    }

    [Test]
    public async Task Missing_portal_is_reported_as_failed_launch()
    {
        IClassicDesktopStyleApplicationLifetime desktop = Substitute.For<IClassicDesktopStyleApplicationLifetime>();
        IXdgDesktopPortal portal = Substitute.For<IXdgDesktopPortal>();
        portal.TryLaunchAsync(Arg.Any<string>(), Arg.Any<OsShellLaunchKind>()).Returns(false);
        AvaloniaOsShell shell = new(desktop, portal, () => true);

        bool launched = await shell.TryLaunchAsync("https://example.com", OsShellLaunchKind.OpenUri);

        launched.Should().BeFalse();
    }

    [TestCase(true, true)]
    [TestCase(false, false)]
    public async Task Picker_guard_allows_nonLinux_or_available_portal(bool isLinux, bool portalAvailable)
    {
        IXdgDesktopPortal portal = Substitute.For<IXdgDesktopPortal>();
        portal.IsInterfaceAvailableAsync(XdgDesktopPortal.FileChooserInterface).Returns(portalAvailable);
        int unavailableReports = 0;

        bool available = await PortalPickerGuard.IsAvailableAsync(portal, () => isLinux, () => unavailableReports++);

        available.Should().BeTrue();
        unavailableReports.Should().Be(0);
    }

    [Test]
    public async Task Picker_guard_reports_missing_Linux_portal_without_fallback()
    {
        IXdgDesktopPortal portal = Substitute.For<IXdgDesktopPortal>();
        portal.IsInterfaceAvailableAsync(XdgDesktopPortal.FileChooserInterface).Returns(false);
        int unavailableReports = 0;

        bool available = await PortalPickerGuard.IsAvailableAsync(portal, () => true, () => unavailableReports++);

        available.Should().BeFalse();
        unavailableReports.Should().Be(1);
    }

    [Test]
    public async Task Linux_open_file_picker_should_materialize_portal_selection()
    {
        IStorageProvider storageProvider = Substitute.For<IStorageProvider>();
        IStorageFile file = Substitute.For<IStorageFile>();
        storageProvider.TryGetFileFromPathAsync("/tmp/selected.txt").Returns(file);
        IXdgDesktopPortal portal = Substitute.For<IXdgDesktopPortal>();
        portal.ShowFileChooserAsync(Arg.Any<XdgFileChooserRequest>()).Returns(
            new XdgFileChooserResult(0, [new Uri("file:///tmp/selected.txt")]));

        IReadOnlyList<IStorageFile> files = await PortalPickerGuard.OpenFilePickerAsync(
            storageProvider,
            new FilePickerOpenOptions
            {
                AllowMultiple = true,
                FileTypeFilter = [new FilePickerFileType("Text") { Patterns = ["*.txt"] }],
                Title = "Choose a file",
            },
            portal,
            () => true);

        files.Should().Equal(file);
        await portal.Received(1).ShowFileChooserAsync(Arg.Is<XdgFileChooserRequest>(request =>
            request.Title == "Choose a file"
            && request.Multiple
            && !request.Directory
            && !request.Save
            && request.Filters.Single().Patterns.SequenceEqual(new[] { "*.txt" })));
    }

    [TestCase(1u)]
    [TestCase(2u)]
    public async Task Linux_file_picker_cancel_or_dismiss_should_complete_without_selection(uint response)
    {
        IStorageProvider storageProvider = Substitute.For<IStorageProvider>();
        IXdgDesktopPortal portal = Substitute.For<IXdgDesktopPortal>();
        portal.ShowFileChooserAsync(Arg.Any<XdgFileChooserRequest>()).Returns(new XdgFileChooserResult(response, []));

        IReadOnlyList<IStorageFolder> folders = await PortalPickerGuard.OpenFolderPickerAsync(
            storageProvider,
            new FolderPickerOpenOptions(),
            portal,
            () => true);

        folders.Should().BeEmpty();
        await storageProvider.DidNotReceiveWithAnyArgs().TryGetFolderFromPathAsync(default!);
    }

    [Test]
    public async Task NonLinux_picker_should_retain_native_storage_provider()
    {
        IStorageProvider storageProvider = Substitute.For<IStorageProvider>();
        IStorageFile file = Substitute.For<IStorageFile>();
        FilePickerSaveOptions options = new() { SuggestedFileName = "saved.txt" };
        storageProvider.SaveFilePickerAsync(options).Returns(file);
        IXdgDesktopPortal portal = Substitute.For<IXdgDesktopPortal>();

        IStorageFile? result = await PortalPickerGuard.SaveFilePickerAsync(
            storageProvider,
            options,
            portal,
            () => false);

        result.Should().BeSameAs(file);
        await portal.DidNotReceiveWithAnyArgs().ShowFileChooserAsync(default!);
    }

    [Test]
    public void Every_product_picker_call_is_guarded_against_managed_Linux_fallback()
    {
        string repositoryRoot = FindRepositoryRoot();
        string[] sourceRoots =
        [
            Path.Combine(repositoryRoot, "src", "app", "GitUI.Avalonia"),
            Path.Combine(repositoryRoot, "src", "plugins"),
        ];
        string[] pickerMethods =
        [
            "OpenFilePickerAsync",
            "OpenFolderPickerAsync",
            "SaveFilePickerAsync",
        ];
        List<string> pickerCalls = [];
        List<string> unguardedCalls = [];

        foreach (string sourceRoot in sourceRoots)
        {
            foreach (string file in Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
                         .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                             && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                             && !path.EndsWith($"{Path.DirectorySeparatorChar}PortalPickerGuard.cs", StringComparison.Ordinal)))
            {
                string[] lines = File.ReadAllLines(file);
                for (int index = 0; index < lines.Length; index++)
                {
                    if (!pickerMethods.Any(lines[index].Contains))
                    {
                        continue;
                    }

                    string relativePath = Path.GetRelativePath(repositoryRoot, file);
                    string location = $"{relativePath}:{index + 1}";
                    pickerCalls.Add(location);
                    bool guarded = lines[index].Contains("PortalPickerGuard.", StringComparison.Ordinal);
                    if (!guarded)
                    {
                        unguardedCalls.Add(location);
                    }
                }
            }
        }

        pickerCalls.Should().NotBeEmpty();
        unguardedCalls.Should().BeEmpty();
    }

    private static string FindRepositoryRoot([CallerFilePath] string startPath = "")
    {
        DirectoryInfo? directory = new FileInfo(startPath).Directory;
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GitExtensions.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
