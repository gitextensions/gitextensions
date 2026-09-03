using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Plugins.Gource;
using GitExtUtils;
using GitUI;
using GitUI.Avatars;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class GourcePluginTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;
    private StubMessageBoxHost _messageBoxes = null!;
    private WinFormsShims.IMessageBoxHost? _originalMessageBoxHost;
    private WinFormsShims.IOsShell? _originalOsShell;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _workingDirectory = Path.Combine(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.GourceTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);

        _originalMessageBoxHost = TryGetMessageBoxHost();
        _originalOsShell = TryGetOsShell();
        _messageBoxes = new StubMessageBoxHost();
        WinFormsShims.ShimHost.MessageBoxHost = _messageBoxes;
        WinFormsShims.ShimHost.OsShell = new StubOsShell();
    }

    [TearDown]
    public void TearDown()
    {
        WinFormsShims.ShimHost.MessageBoxHost = _originalMessageBoxHost ?? new StubMessageBoxHost();
        WinFormsShims.ShimHost.OsShell = _originalOsShell ?? new StubOsShell();
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [TestCase(false, true)]
    [TestCase(true, false)]
    public void IsLaunchAvailable_should_reject_host_executable_in_Flatpak(bool isFlatpak, bool expected)
    {
        GourceStart.IsLaunchAvailable(isFlatpak).Should().Be(expected);
    }

    [AvaloniaTest]
    public void Gource_form_should_construct_with_original_layout_and_translation_keys()
    {
        using GourceStart form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(574.4);
        form.Height.Should().Be(132);
        form.FindControl<Grid>("tableLayoutPanel1")!.Height.Should().Be(79.2);
        form.FindControl<Button>("button1")!.Width.Should().Be(75.2);
        form.FindControl<TextBox>("Arguments").Should().NotBeNull();
        form.FindControl<TextBox>("GourcePath").Should().NotBeNull();
        form.FindControl<TextBox>("WorkingDir").Should().NotBeNull();
        KeyboardNavigation.GetTabIndex(form.FindControl<Button>("button1")!).Should().Be(0);
        KeyboardNavigation.GetTabIndex(form.FindControl<TextBox>("Arguments")!).Should().Be(2);
        KeyboardNavigation.GetTabIndex(form.FindControl<TextBox>("GourcePath")!).Should().Be(5);
        KeyboardNavigation.GetTabIndex(form.FindControl<TextBox>("WorkingDir")!).Should().Be(7);
        KeyboardNavigation.GetTabIndex(form.FindControl<Button>("GourceBrowse")!).Should().Be(8);
        KeyboardNavigation.GetTabIndex(form.FindControl<Button>("WorkingDirBrowse")!).Should().Be(9);
        KeyboardNavigation.GetTabIndex(form.FindControl<HyperlinkButton>("linkLabel1")!).Should().Be(11);
        KeyboardNavigation.GetTabIndex(form.FindControl<HyperlinkButton>("linkLabel2")!).Should().Be(12);
        AutomationProperties.GetAutomationId(form.FindControl<Button>("GourceBrowse")!).Should().Be("GourceBrowse");

        AssertTranslation(translation, "$this", "Gource");
        AssertTranslation(translation, "ArgumentsLabel", "Arguments");
        AssertTranslation(translation, "GourceBrowse", "Browse");
        AssertTranslation(translation, "WorkingDirBrowse", "Browse");
        AssertTranslation(translation, "button1", "Start");
        AssertTranslation(translation, "label1", "Path to Gource");
        AssertTranslation(translation, "label2", "Repository");
        AssertTranslation(translation, "linkLabel1", "Gource project");
        AssertTranslation(translation, "linkLabel2", "Gource command line");

        static void AssertTranslation(ITranslation translation, string item, string text)
        {
            translation.Received(1).AddTranslationItem(nameof(GourceStart), item, "Text", text);
        }
    }

    [AvaloniaTest]
    public void Gource_plugin_should_expose_its_icon_and_native_string_settings()
    {
        GourcePlugin plugin = new();
        ISetting[] settings = plugin.GetSettings().ToArray();

        plugin.Id.Should().Be(new Guid("F0A6A769-6DCC-4452-9A43-343347015EEC"));
        PluginIconProvider.GetIcon(plugin).Should().NotBeNull();
        settings.Select(setting => setting.Name).Should().Equal("Path to Gource", "Arguments");
        settings.Should().AllBeOfType<StringSetting>();
        settings.Select(PluginSettingControlFactory.Create)
            .Select(binding => binding.Control)
            .Should().AllBeOfType<TextBox>();
    }

    [AvaloniaTest]
    public void Gource_plugin_should_reject_invalid_repository_and_reset_missing_settings_only_when_confirmed()
    {
        GourcePlugin plugin = new();
        DictionarySettingsSource settings = new();
        IGitPluginSettingsContainer settingsContainer = Substitute.For<IGitPluginSettingsContainer>();
        settingsContainer.GetSettingsSource().Returns(settings);
        plugin.SettingsContainer = settingsContainer;
        GourcePlugin.TestAccessor accessor = plugin.GetTestAccessor();
        string missingPath = Path.Combine(_workingDirectory, "missing-gource");
        accessor.GourcePath[settings] = missingPath;
        accessor.GourceArguments[settings] = "--seconds-per-day 0.5";

        accessor.GourcePath.ValueOrDefault(settings).Should().Be(missingPath);
        accessor.GourceArguments.ValueOrDefault(settings).Should().Be("--seconds-per-day 0.5");

        _messageBoxes.Result = WinFormsShims.DialogResult.No;
        accessor.ResetMissingConfiguredPath(null, missingPath).Should().Be(missingPath);
        accessor.GourcePath.ValueOrDefault(settings).Should().Be(missingPath);

        _messageBoxes.Result = WinFormsShims.DialogResult.Yes;
        accessor.ResetMissingConfiguredPath(null, missingPath).Should().BeEmpty();
        accessor.GourcePath.ValueOrDefault(settings).Should().BeEmpty();

        _messageBoxes.Messages.Clear();
        IGitModule module = Substitute.For<IGitModule>();
        module.IsValidGitWorkingDir().Returns(false);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        plugin.Execute(new GitUIEventArgs(ownerForm: null, commands)).Should().BeFalse();
        _messageBoxes.Messages.Should().ContainSingle()
            .Which.ReplaceLineEndings("\n").Should().Be(
                "The current directory is not a valid git repository.\n\n"
                + "Gource can be only be started from a valid git repository.");
    }

    [AvaloniaTest]
    public async Task Gource_picker_contract_should_preserve_filters_start_locations_selection_and_cancel()
    {
        using GourceStart form = new();
        GourceStart.TestAccessor accessor = form.GetTestAccessor();
        IStorageProvider storageProvider = Substitute.For<IStorageProvider>();
        IStorageFolder executableFolder = Substitute.For<IStorageFolder>();
        IStorageFolder repositoryFolder = Substitute.For<IStorageFolder>();
        string executablePath = Path.Combine(_workingDirectory, "tools", OperatingSystem.IsWindows() ? "gource.exe" : "gource");
        string executableDirectory = Path.GetDirectoryName(executablePath)!;
        accessor.GourcePath.Text = executablePath;
        accessor.WorkingDir.Text = _workingDirectory;
        storageProvider.TryGetFolderFromPathAsync(executableDirectory).Returns(executableFolder);
        storageProvider.TryGetFolderFromPathAsync(_workingDirectory).Returns(repositoryFolder);

        FilePickerOpenOptions executableOptions = await accessor.CreateGourcePickerOptionsAsync(storageProvider);
        FolderPickerOpenOptions repositoryOptions = await accessor.CreateWorkingDirectoryPickerOptionsAsync(storageProvider);

        executableOptions.AllowMultiple.Should().BeFalse();
        executableOptions.FileTypeFilter.Should().ContainSingle().Which.Patterns
            .Should().Equal(OperatingSystem.IsWindows() ? "gource.exe" : "gource");
        executableOptions.SuggestedStartLocation.Should().BeSameAs(executableFolder);
        repositoryOptions.AllowMultiple.Should().BeFalse();
        repositoryOptions.SuggestedStartLocation.Should().BeSameAs(repositoryFolder);

        accessor.ApplyGourceSelection(null);
        accessor.ApplyWorkingDirectorySelection(string.Empty);
        accessor.GourcePath.Text.Should().Be(executablePath);
        accessor.WorkingDir.Text.Should().Be(_workingDirectory);

        string selectedExecutable = Path.Combine(_workingDirectory, "selected-gource");
        string selectedRepository = Path.Combine(_workingDirectory, "selected-repository");
        accessor.ApplyGourceSelection(selectedExecutable);
        accessor.ApplyWorkingDirectorySelection(selectedRepository);
        accessor.GourcePath.Text.Should().Be(selectedExecutable);
        accessor.WorkingDir.Text.Should().Be(selectedRepository);
    }

    [AvaloniaTest]
    public void Gource_start_should_validate_missing_and_confined_executables()
    {
        using GourceStart form = new();
        GourceStart.TestAccessor accessor = form.GetTestAccessor();
        IAvatarProvider avatars = Substitute.For<IAvatarProvider>();

        accessor.GourcePath.Text = Path.Combine(_workingDirectory, "missing-gource");
        accessor.StartGource(false, _ => null, avatars);
        _messageBoxes.Messages.Should().ContainSingle().Which.Should().StartWith("Cannot find Gource.");

        _messageBoxes.Messages.Clear();
        string existingPath = Path.Combine(_workingDirectory, "gource");
        File.WriteAllText(existingPath, string.Empty);
        accessor.GourcePath.Text = existingPath;
        accessor.StartGource(true, _ => null, avatars);
        _messageBoxes.Messages.Should().ContainSingle()
            .Which.Should().Contain("Gource is not available in this Flatpak installation.");
    }

    [AvaloniaTest]
    public void Gource_start_should_persist_fields_substitute_avatars_and_report_launch_errors()
    {
        GitModule module = CreateRepository();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        GitUIEventArgs eventArgs = new(ownerForm: null, commands);
        string executablePath = Path.Combine(_workingDirectory, "gource");
        File.WriteAllText(executablePath, string.Empty);
        IAvatarProvider avatars = Substitute.For<IAvatarProvider>();
        avatars.GetAvatarAsync(Arg.Any<string>(), Arg.Any<string>(), 90).Returns(Encoding.UTF8.GetBytes("avatar"));
        using GourceStart form = new(executablePath, eventArgs, "--user-image-dir \"$(AVATARS)\"");
        GourceStart.TestAccessor accessor = form.GetTestAccessor();
        ProcessStartInfo? launched = null;

        accessor.StartGource(
            false,
            startInfo =>
            {
                launched = startInfo;
                return null;
            },
            avatars);

        launched.Should().NotBeNull();
        launched!.FileName.Should().Be(executablePath);
        launched.WorkingDirectory.Should().Be(module.WorkingDir);
        launched.Arguments.Should().NotContain("$(AVATARS)");
        launched.Arguments.Should().Contain(Path.Combine(Path.GetTempPath(), "GitAvatars"));
        form.PathToGource.Should().Be(executablePath);
        form.GitWorkingDir.Should().Be(module.WorkingDir);
        form.GourceArguments.Should().Be("--user-image-dir \"$(AVATARS)\"");
        File.ReadAllBytes(Path.Combine(Path.GetTempPath(), "GitAvatars", "Alice.png"))
            .Should().Equal(Encoding.UTF8.GetBytes("avatar"));

        _messageBoxes.Messages.Clear();
        using GourceStart failingForm = new(executablePath, eventArgs, "--hide filenames");
        failingForm.GetTestAccessor().StartGource(
            false,
            _ => throw new InvalidOperationException("launch failed"),
            avatars);
        _messageBoxes.Messages.Should().ContainSingle("launch failed");
    }

    [AvaloniaTest]
    public void Gource_links_should_use_the_portable_shell_boundary()
    {
        StubOsShell shell = new();
        WinFormsShims.ShimHost.OsShell = shell;
        using GourceStart form = new();
        GourceStart.TestAccessor accessor = form.GetTestAccessor();

        accessor.OpenProjectLink();
        accessor.OpenCommandLineLink();

        shell.Launches.Should().Equal(
            ("https://github.com/acaudwell/Gource/", WinFormsShims.OsShellLaunchKind.OpenUri),
            ("https://github.com/acaudwell/Gource#readme", WinFormsShims.OsShellLaunchKind.OpenUri));
    }

    [Test]
    public async Task Gource_release_search_and_download_should_cover_success_fallback_and_failure()
    {
        RecordingHttpHandler handler = new(request =>
        {
            string json = request.RequestUri!.AbsoluteUri.EndsWith("/latest", StringComparison.Ordinal)
                ? "{\"assets\":[{\"name\":\"gource-1.0.src.zip\",\"browser_download_url\":\"https://example.invalid/src\"}]}"
                : "{\"assets\":[{\"name\":\"gource-0.53.win64.zip\",\"browser_download_url\":\"https://example.invalid/gource.zip\"}]}";
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
        });
        using HttpClient httpClient = new(handler);

        string url = await GourcePlugin.TestAccessor.SearchForGourceUrlAsync(null, httpClient);

        url.Should().Be("https://example.invalid/gource.zip");
        handler.Requests.Should().HaveCount(2);
        handler.Requests.Should().OnlyContain(request => request.Headers.UserAgent.ToString() == "GitExtensions");

        byte[] content = Encoding.UTF8.GetBytes("gource archive");
        using HttpClient downloadClient = new(new RecordingHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(content),
        }));
        string downloadPath = Path.Combine(_workingDirectory, "gource.zip");
        int downloaded = await GourcePlugin.TestAccessor.DownloadFileAsync(
            null,
            "https://example.invalid/gource.zip",
            downloadPath,
            downloadClient);
        downloaded.Should().Be(content.Length);
        File.ReadAllBytes(downloadPath).Should().Equal(content);

        _messageBoxes.Messages.Clear();
        using HttpClient failingClient = new(new RecordingHttpHandler(_ => throw new HttpRequestException("network failed")));
        string missingUrl = await GourcePlugin.TestAccessor.SearchForGourceUrlAsync(null, failingClient);
        missingUrl.Should().BeEmpty();
        _messageBoxes.Messages.Should().ContainSingle("network failed");

        _messageBoxes.Messages.Clear();
        string failedDownloadPath = Path.Combine(_workingDirectory, "failed.zip");
        int failedDownload = await GourcePlugin.TestAccessor.DownloadFileAsync(
            null,
            "https://example.invalid/gource.zip",
            failedDownloadPath,
            failingClient);
        failedDownload.Should().Be(0);
        File.Exists(failedDownloadPath).Should().BeFalse();
        _messageBoxes.Messages.Should().ContainSingle("network failed");
    }

    [Test]
    public void Gource_archive_extraction_should_skip_ini_and_traversal_entries_and_delete_the_zip()
    {
        string archivePath = Path.Combine(_workingDirectory, "archive.zip");
        string outputDirectory = Path.Combine(_workingDirectory, "output");
        using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
        {
            WriteEntry("gource.exe", "binary");
            WriteEntry("settings.ini", "ignored");
            WriteEntry("../escape.txt", "blocked");

            void WriteEntry(string name, string content)
            {
                using StreamWriter writer = new(archive.CreateEntry(name).Open());
                writer.Write(content);
            }
        }

        GourcePlugin.TestAccessor.UnZipFiles(null, archivePath, outputDirectory, deleteZipFile: true);

        File.ReadAllText(Path.Combine(outputDirectory, "gource.exe")).Should().Be("binary");
        File.Exists(Path.Combine(outputDirectory, "settings.ini")).Should().BeFalse();
        File.Exists(Path.Combine(_workingDirectory, "escape.txt")).Should().BeFalse();
        File.Exists(archivePath).Should().BeFalse();
        _messageBoxes.Messages.Should().BeEmpty();
    }

    [Test]
    public void Gource_process_should_launch_without_a_shell_or_executable_quoting()
    {
        ProcessStartInfo startInfo = GourceStart.CreateProcessStartInfo(
            "/opt/Gource App/gource",
            "--hide filenames --seconds-per-day 0.5",
            "/tmp/repository path");

        startInfo.FileName.Should().Be("/opt/Gource App/gource");
        startInfo.Arguments.Should().Be("--hide filenames --seconds-per-day 0.5");
        startInfo.WorkingDirectory.Should().Be("/tmp/repository path");
        startInfo.UseShellExecute.Should().BeFalse();
    }

    [Test]
    public void Gource_plugin_should_offer_the_windows_archive_only_on_Windows()
    {
        GourcePlugin.ShouldOfferAutomaticDownload.Should().Be(OperatingSystem.IsWindows());
    }

    [Test]
    public void Avalonia_build_graph_should_include_every_bundled_plugin_project()
    {
        string repositoryRoot = FindRepositoryRoot();
        string[] expected = Directory.GetFiles(
                Path.Join(repositoryRoot, "src", "plugins"),
                "*.csproj",
                SearchOption.AllDirectories)
            .Select(path => NormalizeRelativePath(repositoryRoot, path))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        XDocument solution = XDocument.Load(Path.Join(repositoryRoot, "GitExtensions.Avalonia.slnx"));
        string[] solutionProjects = solution.Descendants("Project")
            .Select(project => project.Attribute("Path")?.Value)
            .Where(path => path?.StartsWith("src/plugins/", StringComparison.OrdinalIgnoreCase) == true)
            .Select(path => path!)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        solutionProjects.Should().Equal(expected);

        string entryProjectPath = Path.Join(
            repositoryRoot,
            "src",
            "app",
            "GitExtensions.Avalonia",
            "GitExtensions.Avalonia.csproj");
        string entryProjectDirectory = Path.GetDirectoryName(entryProjectPath)!;
        XDocument entryProject = XDocument.Load(entryProjectPath);
        string[] buildOnlyPluginProjects = entryProject.Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(path => path?.Contains("plugins", StringComparison.OrdinalIgnoreCase) == true)
            .Select(path => ResolveProjectReference(repositoryRoot, entryProjectDirectory, path!))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        string[] expectedBuildOnlyProjects = expected
            .Where(path => !path.EndsWith("GitUIPluginInterfaces.csproj", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        buildOnlyPluginProjects.Should().Equal(expectedBuildOnlyProjects);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !Directory.Exists(Path.Join(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("The test checkout root was not found.");
    }

    private static string NormalizeRelativePath(string repositoryRoot, string path)
        => Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/');

    private static string ResolveProjectReference(string repositoryRoot, string projectDirectory, string include)
    {
        string nativeInclude = include
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
        return NormalizeRelativePath(repositoryRoot, Path.GetFullPath(Path.Combine(projectDirectory, nativeInclude)));
    }

    private GitModule CreateRepository()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "main" }).Should().BeTrue();
        module.SetSetting("user.name", "Alice");
        module.SetSetting("user.email", "alice@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "readme.txt"), "first\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "readme.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        module.SetSetting("user.name", "Bad/Name");
        module.SetSetting("user.email", "bad@example.com");
        File.AppendAllText(Path.Combine(_workingDirectory, "readme.txt"), "second\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-am", "second" }).Should().BeTrue();
        return module;
    }

    private static WinFormsShims.IMessageBoxHost? TryGetMessageBoxHost()
    {
        try
        {
            return WinFormsShims.ShimHost.MessageBoxHost;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static WinFormsShims.IOsShell? TryGetOsShell()
    {
        try
        {
            return WinFormsShims.ShimHost.OsShell;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public List<string> Messages { get; } = [];

        public WinFormsShims.DialogResult Result { get; set; } = WinFormsShims.DialogResult.OK;

        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
        {
            Messages.Add(text ?? string.Empty);
            return Result;
        }
    }

    private sealed class StubOsShell : WinFormsShims.IOsShell
    {
        public List<(string Target, WinFormsShims.OsShellLaunchKind Kind)> Launches { get; } = [];

        public bool TryLaunch(string target, WinFormsShims.OsShellLaunchKind kind)
        {
            Launches.Add((target, kind));
            return true;
        }
    }

    private sealed class RecordingHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(responseFactory(request));
        }
    }

    private sealed class DictionarySettingsSource : SettingsSource
    {
        private readonly Dictionary<string, string?> _values = [];

        public override SettingLevel SettingLevel => SettingLevel.Local;

        public override string? GetValue(string name) => _values.GetValueOrDefault(name);

        public override void SetValue(string name, string? value) => _values[name] = value;
    }
}
